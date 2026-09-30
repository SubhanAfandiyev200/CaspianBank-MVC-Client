(function () {
  const cardId = new URLSearchParams(location.search).get("id") || "";
  const root = document.getElementById("card-root");
  const skel = document.getElementById("card-skel");
  const blockBtn = document.getElementById("block-open");
  let period = "month";
  let type = "all";
  let range = BankUI.periodBounds("month");
  let historyOpen = false;

  function currentCard() {
    return CaspianStore.get().cards.find((card) => card.id === cardId) || null;
  }

  function rowsFor(card) {
    return CaspianStore.get().transactions.filter((tx) => {
      if (tx.cardId !== card.id) return false;
      if (type !== "all" && tx.type !== type) return false;
      return BankUI.inRange(tx.at, range.from, range.to);
    });
  }

  function render() {
    const card = currentCard();
    if (!card) {
      blockBtn.hidden = true;
      root.innerHTML = BankUI.emptyHtml("Card not found", "This card isn't on the account.", '<a class="btn btn-secondary" href="app.html">Back to cards</a>');
      return;
    }
    const rule = CaspianDemo.limits[card.tier];
    document.getElementById("card-heading").textContent = card.label;
    blockBtn.hidden = false;
    blockBtn.disabled = !!card.blocked;
    blockBtn.textContent = card.blocked ? "Card is blocked" : "Block this card";
    const transactions = rowsFor(card);
    root.innerHTML = `
      ${BankUI.plasticHtml(card, { large: true, link: false })}
      <div class="meta-grid">
        <div class="meta-item"><span>Status</span><strong>${card.blocked ? "Blocked" : "Active"}</strong></div>
        <div class="meta-item"><span>Expires</span><strong>${BankUI.esc(card.expiry)}</strong></div>
        <div class="meta-item"><span>Cashback</span><strong>${BankUI.formatPercent(rule.cashbackRate)}</strong></div>
        <div class="meta-item"><span>Transfer limit</span><strong>${BankUI.formatMoney(rule.transferLimit)}</strong></div>
        <div class="meta-item"><span>Commission after limit</span><strong>${BankUI.formatPercent(rule.commissionRate)}</strong></div>
        <div class="meta-item"><span>Opening fee</span><strong>${BankUI.formatMoney(rule.openingFee)}</strong></div>
        <div class="meta-item"><span>Card number</span><div class="copy-line"><strong class="num">${BankUI.formatCard(card.number)}</strong><button type="button" class="btn-copy" data-copy="${BankUI.esc(card.number)}" data-copy-label="Card number">Copy</button></div></div>
        <div class="meta-item"><span>CVV</span><div class="copy-line"><strong class="num">${BankUI.esc(BankUI.cardCvv(card))}</strong><button type="button" class="btn-copy" data-copy="${BankUI.esc(BankUI.cardCvv(card))}" data-copy-label="CVV">Copy</button></div></div>
      </div>
      <button type="button" class="btn btn-secondary" id="history-toggle" aria-expanded="${historyOpen ? "true" : "false"}" aria-controls="card-history">History</button>
      <section class="surface surface-tight" id="card-history"${historyOpen ? "" : " hidden"}>
        <div class="toolbar filters">
          <div class="field grow">
            <span id="period-label">Period</span>
            <div class="segment" id="period" role="group" aria-labelledby="period-label">
              ${["day", "week", "month", "year", "all"].map((key) =>
                `<button type="button" data-period="${key}" class="${key === period ? "is-active" : ""}">${key[0].toUpperCase() + key.slice(1)}</button>`
              ).join("")}
            </div>
          </div>
          <div class="field">
            <label for="tx-from">From</label>
            <input class="input" id="tx-from" type="date" value="${range.from}" />
          </div>
          <div class="field">
            <label for="tx-to">To</label>
            <input class="input" id="tx-to" type="date" value="${range.to}" />
          </div>
          <div class="field">
            <label for="tx-type">Type</label>
            <select class="select" id="tx-type">
              <option value="all"${type === "all" ? " selected" : ""}>All</option>
              <option value="income"${type === "income" ? " selected" : ""}>Income</option>
              <option value="expense"${type === "expense" ? " selected" : ""}>Expense</option>
            </select>
          </div>
        </div>
        <div class="table-wrap">
          <table class="data-table" id="card-tx">
            <thead>
              <tr>
                <th data-sort data-type="date" aria-sort="descending">Date</th>
                <th data-sort>Description</th>
                <th data-sort>Type</th>
                <th data-sort data-type="number" class="num">Amount</th>
                <th data-sort data-type="number" class="num">Balance after</th>
                <th class="actions">Report</th>
              </tr>
            </thead>
            <tbody>
              ${transactions.length ? transactions.map(txRow).join("") : `<tr class="empty-row"><td colspan="6"><div class="empty"><h3>No transactions</h3><p>Nothing on this card for the selected period.</p></div></td></tr>`}
            </tbody>
          </table>
        </div>
      </section>`;
    const table = document.getElementById("card-tx");
    delete table.dataset.enhanced;
    BankUI.enhanceTable(table);
  }

  function txRow(tx) {
    const signed = tx.type === "income" ? tx.amount : -tx.amount;
    return `<tr>
      <td data-value="${BankUI.esc(tx.at)}">${BankUI.formatDateTime(tx.at)}</td>
      <td data-value="${BankUI.esc(tx.description)}">${BankUI.esc(tx.description)}</td>
      <td data-value="${tx.type}">${tx.type === "income" ? "Income" : "Expense"}</td>
      <td class="num" data-value="${signed}">${BankUI.amountHtml(tx.type, tx.amount)}</td>
      <td class="num" data-value="${tx.balanceAfter}">${BankUI.formatMoney(tx.balanceAfter)}</td>
      <td class="actions"><button type="button" class="btn btn-secondary btn-sm" data-report="${BankUI.esc(tx.id)}">Report</button></td>
    </tr>`;
  }

  root.addEventListener("click", (event) => {
    if (event.target.closest("#history-toggle")) {
      historyOpen = !historyOpen;
      render();
      return;
    }
    const periodBtn = event.target.closest("[data-period]");
    if (periodBtn) {
      period = periodBtn.getAttribute("data-period");
      range = BankUI.periodBounds(period);
      render();
    }
  });
  root.addEventListener("change", (event) => {
    if (event.target.id === "tx-from" || event.target.id === "tx-to") {
      period = "all";
      range = {
        from: document.getElementById("tx-from").value,
        to: document.getElementById("tx-to").value
      };
    }
    if (event.target.id === "tx-type") type = event.target.value;
    if (event.target.id === "tx-from" || event.target.id === "tx-to" || event.target.id === "tx-type") render();
  });

  blockBtn.addEventListener("click", () => {
    const card = currentCard();
    if (!card || card.blocked) return;
    BankUI.openModal("block-modal");
  });

  document.getElementById("block-form").addEventListener("submit", async (event) => {
    event.preventDefault();
    const card = currentCard();
    const email = document.getElementById("block-email");
    const password = document.getElementById("block-password");
    const state = CaspianStore.get();
    BankUI.clearErrors(event.target);
    let ok = true;
    if (email.value.trim().toLowerCase() !== (state.session?.email || "").toLowerCase()) {
      BankUI.setFieldError(email, "Enter the email on this account.");
      ok = false;
    }
    if (!state.session?.credential || password.value !== state.session.credential) {
      BankUI.setFieldError(password, "Password doesn't match.");
      ok = false;
    }
    if (!ok || !card) return;
    const button = document.getElementById("block-submit");
    BankUI.setBusy(button, true);
    // TODO: POST /api/cards/{id}/block  { email, password }
    await BankUI.request("/api/cards/" + card.id + "/block", { method: "POST", body: JSON.stringify({ email: email.value.trim() }) });
    CaspianStore.update((next) => {
      const target = next.cards.find((item) => item.id === card.id);
      if (target) target.blocked = true;
      CaspianStore.logAudit(next, "Card blocked", (target?.label || "Card") + " ending " + (target?.number || "").slice(-4));
      CaspianStore.pushNote(next, {
        type: "alert",
        title: "Card blocked",
        body: (target?.label || "Card") + " ending " + (target?.number || "").slice(-4) + " can no longer be used."
      });
    });
    BankUI.closeModal(document.getElementById("block-modal"));
    BankUI.toast("The card is blocked.");
    BankUI.refreshChrome();
    email.value = "";
    password.value = "";
    BankUI.setBusy(button, false);
    render();
  });

  // TODO: GET /api/cards/{id}
  BankUI.request("/api/cards/" + cardId).then(() => {
    skel.hidden = true;
    root.hidden = false;
    render();
    if (location.hash === "#block") {
      const card = currentCard();
      if (card && !card.blocked) BankUI.openModal("block-modal");
    }
  });
})();
