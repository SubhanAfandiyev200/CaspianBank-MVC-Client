(function () {
  const root = document.getElementById("dash");
  const skel = document.getElementById("dash-skel");

  function greeting(name) {
    const hour = new Date().getHours();
    const hello = hour < 12 ? "Good morning" : hour < 18 ? "Good afternoon" : "Good evening";
    return hello + ", " + name;
  }

  function render() {
    const state = CaspianStore.get();
    const first = state.profile?.firstName || "there";
    document.getElementById("dash-title").textContent = greeting(first);
    if (!state.cards.length) {
      root.innerHTML = BankUI.emptyHtml(
        "Add your first cards",
        "Your account includes a Main card and a Cashback card. Create them to see a balance and move money.",
        '<a class="btn btn-primary" href="add-card.html">Create my cards</a>'
      );
      return;
    }
    const recent = state.transactions.slice(0, 6);
    const activity = recent.length
      ? recent.map((tx) => `<article class="activity">
          <div class="activity-icon">${BankUI.categoryIcon(tx.category)}</div>
          <div>
            <div class="activity-desc">${BankUI.esc(tx.description)}</div>
            <div class="activity-meta">${BankUI.formatDate(tx.at)} · ${BankUI.esc(BankUI.cardLabel(state, tx.cardId))}</div>
          </div>
          <div>${BankUI.amountHtml(tx.type, tx.amount)}</div>
        </article>`).join("")
      : `<div class="empty"><h3>No activity yet</h3><p>Transactions on your cards will show up here.</p></div>`;

    root.innerHTML = `
      <div class="dash-top">
        <div class="deck">
          <div class="deck-stage" data-deck tabindex="0" aria-label="Your cards">
            ${state.cards.map((card) => BankUI.plasticHtml(card)).join("")}
          </div>
          <div class="deck-bar">
            <button type="button" class="deck-arrow" data-deck-prev aria-label="Previous card">‹</button>
            <div class="deck-dots" data-deck-dots></div>
            <button type="button" class="deck-arrow" data-deck-next aria-label="Next card">›</button>
          </div>
          <a class="deck-add" href="add-card.html">Add a card</a>
        </div>
      </div>
      <div class="quick-actions mt-5">
        <a class="btn btn-secondary" href="transfer.html">${BankUI.icon("arrows")} Transfer</a>
        <a class="btn btn-secondary" href="bill-payment.html">${BankUI.icon("zap")} Pay a Bill</a>
        <a class="btn btn-secondary" href="loan-application.html">${BankUI.icon("loan")} Request a Loan</a>
      </div>
      <section class="surface mt-5">
        <div class="spread">
          <h2>Recent activity</h2>
          <a href="transaction-history.html">View all</a>
        </div>
        <div>${activity}</div>
      </section>`;
    BankUI.mountDeck(root.querySelector("[data-deck]"));
  }

  // TODO: GET /api/dashboard
  BankUI.request("/api/dashboard").then(() => {
    skel.hidden = true;
    root.hidden = false;
    render();
  });
})();
