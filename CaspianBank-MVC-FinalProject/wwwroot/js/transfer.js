(function () {
  const form = document.getElementById("transfer-form");
  const review = document.getElementById("transfer-review");
  const done = document.getElementById("transfer-done");
  const fromSelect = document.getElementById("from-card");
  const toSelect = document.getElementById("to-card");
  const ownField = document.getElementById("own-field");
  const otherField = document.getElementById("other-field");
  const hint = document.getElementById("limit-hint");
  let mode = "own";
  let draft = null;
  let lastDigits = null;

  function round2(n) { return Math.round(n * 100) / 100; }
  function parseAmount(value) {
    const n = Number(String(value).replace(/,/g, "").trim());
    if (!Number.isFinite(n) || n <= 0) return null;
    return round2(n);
  }
  function restricted(state) {
    const user = state.users.find((item) => item.email.toLowerCase() === state.profile.email.toLowerCase());
    return user && user.status === "Restricted";
  }
  function activeCards(state, exceptId) {
    return state.cards.filter((card) => !card.blocked && card.id !== exceptId);
  }
  function option(card) {
    return `<option value="${card.id}">${BankUI.esc(card.label)} · •••• ${card.number.slice(-4)} · ${BankUI.formatMoney(card.balance)}</option>`;
  }

  function fillSelects() {
    const state = CaspianStore.get();
    const sources = activeCards(state);
    const fromValue = fromSelect.value;
    fromSelect.innerHTML = sources.map(option).join("");
    if ([...fromSelect.options].some((item) => item.value === fromValue)) fromSelect.value = fromValue;
    const destinations = activeCards(state, fromSelect.value);
    const toValue = toSelect.value;
    toSelect.innerHTML = destinations.map(option).join("");
    if ([...toSelect.options].some((item) => item.value === toValue)) toSelect.value = toValue;
    const source = state.cards.find((card) => card.id === fromSelect.value);
    hint.textContent = source ? BankUI.limitSentence(source.tier) : "";
    paintCard(false);
    if (!sources.length) {
      form.hidden = true;
      review.hidden = true;
      if (!document.getElementById("transfer-empty")) {
        form.insertAdjacentHTML("beforebegin", `<div id="transfer-empty">${BankUI.emptyHtml("No card available", "Add an active card before sending money.", '<a class="btn btn-primary" href="add-card.html">Add a card</a>')}</div>`);
      }
    }
  }

  function setMode(next) {
    mode = next;
    document.getElementById("to-own").classList.toggle("is-active", mode === "own");
    document.getElementById("to-other").classList.toggle("is-active", mode === "other");
    ownField.hidden = mode !== "own";
    otherField.hidden = mode !== "other";
    paintCard(false);
  }

  function cardDigits() {
    const state = CaspianStore.get();
    if (mode === "other") return document.getElementById("to-number").value.replace(/\D/g, "").slice(0, 16);
    const destination = state.cards.find((card) => card.id === toSelect.value);
    return destination ? String(destination.number || "").replace(/\D/g, "").slice(0, 16) : "";
  }

  function paintNumber(digits) {
    const numberBox = document.getElementById("live-number");
    const chars = Array.from({ length: 16 }, (_, index) => digits[index] || "•");
    const previous = lastDigits;
    const firstPaint = previous == null;
    if (numberBox.childElementCount !== 16) {
      numberBox.innerHTML = chars.map((ch, index) => {
        const gap = index > 0 && index % 4 === 0 ? " pay-pan-gap" : "";
        return `<span class="pay-pan-digit${gap}">${ch}</span>`;
      }).join("");
    }
    const fresh = [];
    chars.forEach((ch, index) => {
      const span = numberBox.children[index];
      if (span.textContent !== ch) span.textContent = ch;
      span.classList.toggle("is-on", ch !== "•");
      if (!firstPaint && ch !== "•" && previous[index] !== ch) fresh.push(span);
    });
    fresh.forEach((span, order) => {
      window.setTimeout(() => {
        if (!span.isConnected) return;
        span.classList.remove("is-typed");
        void span.offsetWidth;
        span.classList.add("is-typed");
      }, fresh.length > 1 ? order * 55 : 0);
    });
    lastDigits = digits;
  }

  function paintCard(shine) {
    const numberBox = document.getElementById("live-number");
    if (!numberBox) return;
    const state = CaspianStore.get();
    paintNumber(cardDigits());

    const note = document.getElementById("note").value.trim();
    const destination = state.cards.find((card) => card.id === toSelect.value);
    const holder = note || (mode === "own" && destination ? destination.label : "") || "Recipient";
    document.getElementById("live-holder").textContent = holder;
    const card = document.getElementById("pay-card");
    card.classList.remove("plastic-lg", "plastic-main", "plastic-cashback", "plastic-standard", "plastic-silver");
    card.classList.add("plastic-gold");
    document.getElementById("live-tier").textContent = "Gold";

    const amount = parseAmount(document.getElementById("amount").value);
    const source = state.cards.find((card) => card.id === fromSelect.value);
    const fee = source && amount !== null ? BankUI.commission(source, amount) : 0;
    const total = amount === null ? 0 : round2(amount + fee);
    const shown = amount === null ? 0 : amount;
    document.getElementById("live-amount").textContent = BankUI.formatMoney(shown);
    document.getElementById("live-order-amount").textContent = BankUI.formatMoney(shown);
    document.getElementById("live-fee").textContent = BankUI.formatMoney(fee);
    document.getElementById("live-total").textContent = BankUI.formatMoney(total);
    document.getElementById("live-from").textContent = source ? BankUI.cardLabel(state, source.id) : "—";

    if (shine && card) {
      card.classList.remove("is-shine");
      void card.offsetWidth;
      card.classList.add("is-shine");
    }
  }

  function focusStep(step) {
    document.querySelectorAll("#pay-steps span").forEach((item) => {
      item.classList.toggle("is-on", item.getAttribute("data-step") === step);
    });
    const card = document.getElementById("pay-card");
    if (!card) return;
    card.classList.toggle("is-number", step === "to");
    card.classList.toggle("is-holder", step === "note");
    card.classList.toggle("is-amount", step === "amount");
  }

  fromSelect.addEventListener("change", fillSelects);
  toSelect.addEventListener("change", () => paintCard(true));
  document.getElementById("to-own").addEventListener("click", () => setMode("own"));
  document.getElementById("to-other").addEventListener("click", () => setMode("other"));
  document.getElementById("to-number").addEventListener("input", () => {
    const input = document.getElementById("to-number");
    const digits = input.value.replace(/\D/g, "").slice(0, 16);
    input.value = digits.replace(/(\d{4})(?=\d)/g, "$1 ").trim();
    paintCard(true);
  });
  document.getElementById("amount").addEventListener("input", () => paintCard(false));
  document.getElementById("note").addEventListener("input", () => paintCard(false));
  fromSelect.addEventListener("focus", () => focusStep("from"));
  toSelect.addEventListener("focus", () => focusStep("to"));
  document.getElementById("to-number").addEventListener("focus", () => focusStep("to"));
  document.getElementById("amount").addEventListener("focus", () => focusStep("amount"));
  document.getElementById("note").addEventListener("focus", () => focusStep("note"));
  document.getElementById("note").addEventListener("blur", () => focusStep("from"));

  form.addEventListener("submit", (event) => {
    event.preventDefault();
    BankUI.clearErrors(form);
    const state = CaspianStore.get();
    if (restricted(state)) {
      BankUI.toast("This account is restricted. Contact the bank.", "error");
      return;
    }
    const source = state.cards.find((card) => card.id === fromSelect.value);
    const amount = parseAmount(document.getElementById("amount").value);
    const note = document.getElementById("note").value.trim();
    const amountInput = document.getElementById("amount");
    let ok = true;
    if (!source) {
      BankUI.toast("Choose a card to send from.", "error");
      return;
    }
    if (amount === null) {
      BankUI.setFieldError(amountInput, "Enter an amount greater than zero.");
      ok = false;
    }
    let destination = null;
    let recipient = "";
    if (mode === "own") {
      destination = state.cards.find((card) => card.id === toSelect.value);
      if (!destination) {
        BankUI.setFieldError(toSelect, "Choose another of your cards, or send to another customer.");
        ok = false;
      }
    } else {
      recipient = document.getElementById("to-number").value.replace(/\s/g, "");
      if (!/^\d{16}$/.test(recipient)) {
        BankUI.setFieldError(document.getElementById("to-number"), "Enter a 16-digit card number.");
        ok = false;
      }
      if (recipient && recipient === source.number) {
        BankUI.setFieldError(document.getElementById("to-number"), "Choose a different card from the one you're sending from.");
        ok = false;
      }
    }
    if (!ok) return;
    const fee = BankUI.commission(source, amount);
    const total = round2(amount + fee);
    if (source.balance < total) {
      BankUI.setFieldError(amountInput, "The source card doesn't cover the amount and commission.");
      return;
    }
    draft = { sourceId: source.id, destinationId: destination?.id || "", recipient, amount, fee, total, note };
    const toLabel = destination
      ? BankUI.cardLabel(state, destination.id)
      : "•••• " + recipient.slice(-4);
    document.getElementById("transfer-summary").innerHTML = `
      <div><dt>From</dt><dd>${BankUI.esc(BankUI.cardLabel(state, source.id))}</dd></div>
      <div><dt>To</dt><dd>${BankUI.esc(toLabel)}</dd></div>
      <div><dt>Amount</dt><dd>${BankUI.formatMoney(amount)}</dd></div>
      <div><dt>Commission</dt><dd>${BankUI.formatMoney(fee)}</dd></div>
      ${note ? `<div><dt>Note</dt><dd>${BankUI.esc(note)}</dd></div>` : ""}
      <div><dt>Total debit</dt><dd>${BankUI.formatMoney(total)}</dd></div>`;
    form.hidden = true;
    review.hidden = false;
  });

  document.getElementById("transfer-back").addEventListener("click", () => {
    review.hidden = true;
    form.hidden = false;
  });

  document.getElementById("transfer-confirm").addEventListener("click", async () => {
    if (!draft) return;
    const button = document.getElementById("transfer-confirm");
    BankUI.setBusy(button, true);
    // TODO: POST /api/transfers
    await BankUI.request("/api/transfers", { method: "POST", body: JSON.stringify(draft) });
    const ref = BankUI.uid("TR").toUpperCase();
    let receiptTo = "";
    CaspianStore.update((state) => {
      const source = state.cards.find((card) => card.id === draft.sourceId);
      if (draft.fee > 0) {
        source.balance = round2(source.balance - draft.fee);
        state.transactions.unshift({
          id: BankUI.uid("tx"),
          cardId: source.id,
          description: "Transfer commission",
          type: "expense",
          amount: draft.fee,
          category: "fee",
          at: new Date().toISOString(),
          balanceAfter: source.balance
        });
      }
      if (draft.destinationId) {
        const destination = state.cards.find((card) => card.id === draft.destinationId);
        destination.balance = round2(destination.balance + draft.amount);
        state.transactions.unshift({
          id: BankUI.uid("tx"),
          cardId: destination.id,
          description: "Transfer from " + source.label,
          type: "income",
          amount: draft.amount,
          category: "transfer",
          at: new Date().toISOString(),
          balanceAfter: destination.balance
        });
        receiptTo = BankUI.cardLabel(state, destination.id);
      } else {
        receiptTo = "•••• " + draft.recipient.slice(-4);
      }
      source.balance = round2(source.balance - draft.amount);
      const description = draft.note || (draft.destinationId ? "Transfer to own card" : "Transfer to •••• " + draft.recipient.slice(-4));
      state.transactions.unshift({
        id: BankUI.uid("tx"),
        cardId: source.id,
        description,
        type: "expense",
        amount: draft.amount,
        category: "transfer",
        at: new Date().toISOString(),
        balanceAfter: source.balance
      });
      const user = state.users.find((item) => item.email.toLowerCase() === state.profile.email.toLowerCase());
      if (user) user.sent = round2(user.sent + draft.amount);
      CaspianStore.logAudit(state, "Transfer", BankUI.formatMoney(draft.amount) + " · " + ref);
      CaspianStore.pushNote(state, {
        type: "payment",
        title: "Transfer sent",
        body: BankUI.formatMoney(draft.amount) + " to " + receiptTo + "."
      });
    });
    document.getElementById("transfer-ref").textContent = "Reference " + ref;
    document.getElementById("transfer-receipt").innerHTML = document.getElementById("transfer-summary").innerHTML;
    review.hidden = true;
    done.hidden = false;
    BankUI.setBusy(button, false);
    BankUI.refreshChrome();
    BankUI.toast("Transfer completed.");
  });

  document.getElementById("transfer-another").addEventListener("click", () => {
    draft = null;
    form.reset();
    done.hidden = true;
    form.hidden = false;
    fillSelects();
    setMode("own");
    lastDigits = null;
    paintCard(false);
  });

  fillSelects();
  setMode("own");
})();
