(function () {
  const body = document.getElementById("bill-body");
  const fromSelect = document.getElementById("from-card");

  function round2(n) { return Math.round(n * 100) / 100; }
  function restricted(state) {
    const user = state.users.find((item) => item.email.toLowerCase() === state.profile.email.toLowerCase());
    return user && user.status === "Restricted";
  }

  function fillCards() {
    const cards = CaspianStore.get().cards.filter((card) => !card.blocked);
    fromSelect.innerHTML = cards.map((card) =>
      `<option value="${card.id}">${BankUI.esc(card.label)} · •••• ${card.number.slice(-4)} · ${BankUI.formatMoney(card.balance)}</option>`
    ).join("");
  }

  function statusCell(status) {
    if (status === "Failed") return `${BankUI.badge("Failed")} <span class="hint">Insufficient funds</span>`;
    return BankUI.badge(status);
  }

  function render() {
    const rows = CaspianStore.get().recurring.filter((item) => item.status !== "Cancelled");
    const stats = document.getElementById("bill-stats");
    if (stats) {
      const active = rows.filter((item) => item.status !== "Failed");
      const monthly = active.reduce((sum, item) => sum + Number(item.amount || 0), 0);
      stats.innerHTML = `
        <article class="pulse"><span>Instructions</span><strong>${rows.length}</strong></article>
        <article class="pulse"><span>Each month</span><strong>${BankUI.formatMoney(monthly)}</strong></article>
        <article class="pulse"><span>Runs on</span><strong>1st</strong></article>`;
    }
    body.innerHTML = rows.length ? rows.map((item) => `<tr>
      <td data-value="${BankUI.esc(item.provider)}">${BankUI.esc(item.provider)}</td>
      <td data-value="${BankUI.esc(item.reference)}">${BankUI.esc(item.reference)}</td>
      <td class="num" data-value="${item.amount}">${BankUI.formatMoney(item.amount)}</td>
      <td data-value="${BankUI.esc(item.status)}">${statusCell(item.status)}</td>
      <td class="actions"><button type="button" class="btn-link" data-cancel="${BankUI.esc(item.id)}">Cancel</button></td>
    </tr>`).join("") : `<tr class="empty-row"><td colspan="5"><div class="empty"><h3>No recurring payments</h3><p>Monthly instructions you create will be listed here.</p></div></td></tr>`;
  }

  document.getElementById("provider-grid")?.addEventListener("click", (event) => {
    const choice = event.target.closest("[data-provider]");
    if (!choice) return;
    const provider = document.getElementById("provider");
    provider.value = choice.getAttribute("data-provider");
    document.querySelectorAll("#provider-grid button").forEach((button) => {
      button.classList.toggle("is-active", button === choice);
    });
  });

  document.getElementById("bill-form").addEventListener("submit", async (event) => {
    event.preventDefault();
    const form = event.target;
    BankUI.clearErrors(form);
    const state = CaspianStore.get();
    if (restricted(state)) {
      BankUI.toast("This account is restricted. Contact the bank.", "error");
      return;
    }
    const reference = document.getElementById("reference");
    const amountInput = document.getElementById("amount");
    const amount = Number(String(amountInput.value).replace(/,/g, ""));
    let ok = true;
    if (!reference.value.trim()) {
      BankUI.setFieldError(reference, "Enter the subscriber or meter number.");
      ok = false;
    }
    if (!Number.isFinite(amount) || amount <= 0) {
      BankUI.setFieldError(amountInput, "Enter an amount greater than zero.");
      ok = false;
    }
    const source = state.cards.find((card) => card.id === fromSelect.value);
    if (!source) {
      BankUI.toast("Add an active card before paying a bill.", "error");
      return;
    }
    if (ok && source.balance < amount) {
      BankUI.setFieldError(amountInput, "That card doesn't have enough for this payment.");
      ok = false;
    }
    if (!ok) return;
    const provider = document.getElementById("provider").value;
    const recurring = document.getElementById("recurring").checked;
    const button = form.querySelector("button[type=submit]");
    BankUI.setBusy(button, true);
    // TODO: POST /api/bills
    await BankUI.request("/api/bills", {
      method: "POST",
      body: JSON.stringify({ provider, reference: reference.value.trim(), amount, cardId: source.id, recurring })
    });
    CaspianStore.update((next) => {
      const card = next.cards.find((item) => item.id === source.id);
      card.balance = round2(card.balance - amount);
      next.transactions.unshift({
        id: BankUI.uid("tx"),
        cardId: card.id,
        description: provider + " · " + reference.value.trim(),
        type: "expense",
        amount: round2(amount),
        category: "utilities",
        at: new Date().toISOString(),
        balanceAfter: card.balance
      });
      if (recurring) {
        const nextDate = new Date();
        nextDate.setMonth(nextDate.getMonth() + 1, 1);
        next.recurring.unshift({
          id: BankUI.uid("b"),
          provider,
          reference: reference.value.trim(),
          amount: round2(amount),
          next: nextDate.toISOString(),
          status: "Active"
        });
      }
      const user = next.users.find((item) => item.email.toLowerCase() === next.profile.email.toLowerCase());
      if (user) user.spent = round2(user.spent + amount);
      CaspianStore.logAudit(next, "Bill payment", provider + " " + BankUI.formatMoney(amount));
      CaspianStore.pushNote(next, {
        type: "payment",
        title: "Bill paid",
        body: BankUI.formatMoney(amount) + " to " + provider + "."
      });
    });
    form.reset();
    fillCards();
    render();
    BankUI.setBusy(button, false);
    BankUI.refreshChrome();
    BankUI.toast(recurring ? "Payment sent. It will repeat monthly." : "Payment sent.");
  });

  body.addEventListener("click", async (event) => {
    return;
    const button = event.target.closest("[data-cancel]");
    if (!button) return;
    const id = button.getAttribute("data-cancel");
    const item = CaspianStore.get().recurring.find((row) => row.id === id);
    if (!item) return;
    const yes = await BankUI.confirm({
      title: "Cancel this payment?",
      message: item.provider + " (" + item.reference + ") will no longer be paid automatically.",
      confirmText: "Cancel payment",
      danger: true
    });
    if (!yes) return;
    // TODO: DELETE /api/bills/recurring/{id}
    await BankUI.request("/api/bills/recurring/" + id, { method: "DELETE" });
    CaspianStore.update((state) => {
      const row = state.recurring.find((entry) => entry.id === id);
      if (row) row.status = "Cancelled";
      CaspianStore.logAudit(state, "Bill payment", "Cancelled recurring " + item.provider);
    });
    render();
    BankUI.toast("Recurring payment cancelled.");
  });

  fillCards();
  render();
  BankUI.enhanceTable(document.getElementById("bill-table"));
})();
