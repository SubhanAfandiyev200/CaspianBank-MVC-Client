(function () {
  const userId = new URLSearchParams(location.search).get("id") || "";
  if (!userId) return;
  document.getElementById("user-list")?.setAttribute("hidden", "");
  document.getElementById("user-detail")?.removeAttribute("hidden");
  const root = document.getElementById("user-root");
  const button = document.getElementById("restrict-btn");

  function snapshotCards(user, state) {
    if (user.id === "u1" && state.profile.email.toLowerCase() === CaspianDemo.knownEmail.toLowerCase()) {
      return { cards: state.cards, transactions: state.transactions };
    }
    if (user.id === "u1") {
      const fresh = CaspianDemo.build();
      return { cards: fresh.cards, transactions: fresh.transactions };
    }
    const n = user.name.length;
    const holder = user.name.toUpperCase();
    const cards = [
      { id: user.id + "a", tier: "standard", label: "Standard", number: "453200001111" + String(1000 + n).slice(-4), cvv: String(100 + (n * 17) % 900), balance: 800 + n * 25, expiry: "11/28", holder, accountNo: "CB-41" + user.id, blocked: user.status === "Restricted" },
      { id: user.id + "b", tier: "cashback", label: "Cashback", number: "453211112222" + String(2000 + n).slice(-4), cvv: String(100 + (n * 29) % 900), balance: 240, expiry: "06/29", holder, accountNo: "CB-42" + user.id, blocked: false }
    ];
    const transactions = [
      { id: user.id + "t1", cardId: cards[0].id, description: "Bravo Hypermarket", type: "expense", amount: 36.4, category: "groceries", at: new Date().toISOString(), balanceAfter: cards[0].balance },
      { id: user.id + "t2", cardId: cards[1].id, description: "Salary payment", type: "income", amount: 1500, category: "income", at: new Date(Date.now() - 86400000).toISOString(), balanceAfter: cards[1].balance }
    ];
    return { cards, transactions };
  }

  function render() {
    const state = CaspianStore.get();
    const user = state.users.find((item) => item.id === userId);
    if (!user) {
      document.getElementById("user-name").textContent = "User not found";
      button.hidden = true;
      root.innerHTML = `<div class="empty"><h3>No such customer</h3><p>Return to the user list and choose another row.</p></div>`;
      return;
    }
    document.getElementById("user-name").textContent = user.name;
    document.getElementById("user-meta").textContent = user.email + " · " + user.phone;
    button.hidden = false;
    button.textContent = user.status === "Restricted" ? "Unfreeze account" : "Restrict account";
    button.className = user.status === "Restricted" ? "btn btn-secondary" : "btn btn-danger";
    const data = snapshotCards(user, state);
    root.innerHTML = `
      <div class="meta-grid">
        <div class="meta-item"><span>Status</span><strong>${user.status}</strong></div>
        <div class="meta-item"><span>Registered</span><strong>${BankUI.formatDate(user.registered + "T12:00:00")}</strong></div>
        <div class="meta-item"><span>Total sent</span><strong>${BankUI.formatMoney(user.sent)}</strong></div>
        <div class="meta-item"><span>Total spent</span><strong>${BankUI.formatMoney(user.spent)}</strong></div>
      </div>
      <div class="card-stack">${data.cards.map((card) => BankUI.plasticHtml(card, { link: false })).join("")}</div>
      <section class="surface surface-tight">
        <div class="toolbar"><h2>Recent activity</h2></div>
        <div class="table-wrap">
          <table class="data-table">
            <thead><tr><th>Date</th><th>Description</th><th>Card</th><th class="num">Amount</th></tr></thead>
            <tbody>
              ${data.transactions.slice(0, 8).map((tx) => `<tr>
                <td>${BankUI.formatDateTime(tx.at)}</td>
                <td>${BankUI.esc(tx.description)}</td>
                <td>${BankUI.esc((data.cards.find((card) => card.id === tx.cardId)?.label) || "Card")}</td>
                <td class="num">${BankUI.amountHtml(tx.type, tx.amount)}</td>
              </tr>`).join("")}
            </tbody>
          </table>
        </div>
      </section>`;
  }

  button.addEventListener("click", async () => {
    return;
    const user = CaspianStore.get().users.find((item) => item.id === userId);
    if (!user) return;
    const restrict = user.status !== "Restricted";
    const yes = await BankUI.confirm({
      title: restrict ? "Restrict this account?" : "Restore this account?",
      message: restrict
        ? user.name + " will be unable to move money until the account is restored."
        : user.name + " will be able to transact again.",
      confirmText: restrict ? "Restrict account" : "Unfreeze",
      danger: restrict
    });
    if (!yes) return;
    // TODO: POST /api/admin/users/{id}/status
    await BankUI.request("/api/admin/users/" + userId + "/status", { method: "POST" });
    CaspianStore.update((state) => {
      const target = state.users.find((item) => item.id === userId);
      target.status = restrict ? "Restricted" : "Active";
      CaspianStore.logAudit(state, restrict ? "Account restricted" : "Account restored", target.name);
    });
    render();
    BankUI.toast(restrict ? "Account restricted." : "Account restored.");
  });

  render();
})();
