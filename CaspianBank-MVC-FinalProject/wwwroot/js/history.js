(function () {
  const body = document.getElementById("history-body");
  const cardSelect = document.getElementById("card");
  const typeSelect = document.getElementById("type");
  const fromInput = document.getElementById("from");
  const toInput = document.getElementById("to");
  let period = "all";

  function selected() {
    const state = CaspianStore.get();
    return state.transactions.filter((tx) => {
      if (cardSelect.value !== "all" && tx.cardId !== cardSelect.value) return false;
      if (typeSelect.value !== "all" && tx.type !== typeSelect.value) return false;
      return BankUI.inRange(tx.at, fromInput.value, toInput.value);
    });
  }

  function render() {
    const state = CaspianStore.get();
    const rows = selected();
    const stats = document.getElementById("history-stats");
    if (stats) {
      const income = rows.filter((tx) => tx.type === "income").reduce((sum, tx) => sum + tx.amount, 0);
      const expense = rows.filter((tx) => tx.type !== "income").reduce((sum, tx) => sum + tx.amount, 0);
      stats.innerHTML = `
        <article class="pulse"><span>Movements</span><strong>${rows.length}</strong></article>
        <article class="pulse"><span>Money in</span><strong class="amount-in">${BankUI.formatMoney(income)}</strong></article>
        <article class="pulse"><span>Money out</span><strong class="amount-out">${BankUI.formatMoney(expense)}</strong></article>`;
    }
    body.innerHTML = rows.length ? rows.map((tx) => {
      const signed = tx.type === "income" ? tx.amount : -tx.amount;
      const card = state.cards.find((item) => item.id === tx.cardId);
      const label = BankUI.cardLabel(state, tx.cardId);
      const kind = tx.type === "income" ? "is-in" : "is-out";
      return `<tr>
        <td data-value="${BankUI.esc(tx.at)}">${BankUI.formatDateTime(tx.at)}</td>
        <td data-value="${BankUI.esc(tx.description)}"><span class="tx-line"><span class="tx-mark ${kind}" aria-hidden="true"></span><span>${BankUI.esc(tx.description)}</span></span></td>
        <td data-value="${BankUI.esc(label)}"><span class="tx-card">${BankUI.cardThumb(card, "row")}<span>${BankUI.esc(label)}</span></span></td>
        <td data-value="${tx.type}"><span class="badge ${tx.type === "income" ? "badge-success" : "badge-danger"}">${tx.type === "income" ? "Income" : "Expense"}</span></td>
        <td class="num" data-value="${signed}">${BankUI.amountHtml(tx.type, tx.amount)}</td>
        <td class="num" data-value="${tx.balanceAfter}">${BankUI.formatMoney(tx.balanceAfter)}</td>
        <td class="actions"><button type="button" class="btn btn-secondary btn-sm" data-report="${BankUI.esc(tx.id)}">Report</button></td>
      </tr>`;
    }).join("") : `<tr class="empty-row"><td colspan="7"><div class="empty"><h3>No transactions</h3><p>Nothing matches these filters.</p></div></td></tr>`;
  }

  function fillCards() {
    const state = CaspianStore.get();
    const current = cardSelect.value || "all";
    cardSelect.innerHTML = `<option value="all">All cards</option>` + state.cards.map((card) =>
      `<option value="${card.id}">${BankUI.esc(card.label)} · •••• ${card.number.slice(-4)}</option>`
    ).join("");
    cardSelect.value = [...cardSelect.options].some((option) => option.value === current) ? current : "all";
  }

  document.getElementById("period").addEventListener("click", (event) => {
    const button = event.target.closest("[data-period]");
    if (!button) return;
    period = button.getAttribute("data-period");
    document.querySelectorAll("#period button").forEach((item) => item.classList.toggle("is-active", item === button));
    const bounds = BankUI.periodBounds(period);
    fromInput.value = bounds.from;
    toInput.value = bounds.to;
    render();
  });

  [fromInput, toInput, typeSelect, cardSelect].forEach((el) => el.addEventListener("change", () => {
    if (el === fromInput || el === toInput) {
      period = "all";
      document.querySelectorAll("#period button").forEach((item) => item.classList.toggle("is-active", item.dataset.period === "all"));
    }
    render();
  }));

  document.getElementById("statement-open").addEventListener("click", () => {
    const bounds = fromInput.value ? { from: fromInput.value, to: toInput.value } : BankUI.periodBounds("month");
    document.getElementById("statement-from").value = bounds.from;
    document.getElementById("statement-to").value = bounds.to || bounds.from;
    BankUI.openModal("statement-modal");
  });

  document.getElementById("statement-download").addEventListener("click", async () => {
    return;
    const from = document.getElementById("statement-from").value;
    const to = document.getElementById("statement-to").value;
    if (!from || !to) {
      BankUI.toast("Choose a start and end date.", "error");
      return;
    }
    const button = document.getElementById("statement-download");
    BankUI.setBusy(button, true);
    // TODO: GET /api/statements?from=&to=&cardId=
    await BankUI.request("/api/statements?from=" + from + "&to=" + to);
    const state = CaspianStore.get();
    const rows = state.transactions.filter((tx) => {
      if (cardSelect.value !== "all" && tx.cardId !== cardSelect.value) return false;
      return BankUI.inRange(tx.at, from, to);
    });
    const csv = BankUI.toCsv([
      ["Date", "Description", "Card", "Type", "Amount", "Balance after"],
      ...rows.map((tx) => [
        BankUI.formatDateTime(tx.at),
        tx.description,
        BankUI.cardLabel(state, tx.cardId),
        tx.type,
        (tx.type === "income" ? "" : "-") + tx.amount.toFixed(2),
        tx.balanceAfter.toFixed(2)
      ])
    ]);
    BankUI.downloadText("caspian-statement-" + from + "-to-" + to + ".csv", csv);
    CaspianStore.update((next) => CaspianStore.logAudit(next, "Statement generated", from + " to " + to));
    BankUI.setBusy(button, false);
    BankUI.closeModal(document.getElementById("statement-modal"));
    BankUI.toast("Statement downloaded.");
  });

  fillCards();
  render();
  BankUI.enhanceTable(document.getElementById("history-table"));
})();
