(function () {
  const state = CaspianStore.get();
  const users = state.users;
  const volume = users.reduce((sum, user) => sum + user.sent + user.received + user.spent, 0);
  const pendingLoans = state.adminLoans.filter((loan) => loan.status === "Pending").length;
  const pendingReports = state.reports.filter((report) => report.status === "Pending").length;
  const pendingTickets = state.tickets.filter((ticket) => ticket.status === "Pending").length;
  const stats = [
    ["Total users", String(users.length), "Registered customers", "admin-users.html"],
    ["Transaction volume", BankUI.formatMoney(volume), "Sent, received, and card spend", ""],
    ["Pending loans", String(pendingLoans), "Waiting for a decision", "admin-loans.html"],
    ["Pending reports", String(pendingReports), "Suspicious transactions", "admin-reports.html"],
    ["Pending tickets", String(pendingTickets), "Contact messages", "admin-contact-tickets.html"]
  ];
  document.getElementById("stat-grid").innerHTML = stats.map(([label, value, hint, href]) => `
    <article class="stat">
      <p class="stat-label">${BankUI.esc(label)}</p>
      <p class="stat-value">${value}</p>
      <p class="hint">${BankUI.esc(hint)}</p>
      ${href ? `<a href="${href}">View</a>` : ""}
    </article>`).join("");

  document.getElementById("agg-body").innerHTML = users.map((user) => `<tr>
    <td data-value="${BankUI.esc(user.name)}"><a href="admin-users.html?id=${user.id}">${BankUI.esc(user.name)}</a></td>
    <td class="num" data-value="${user.sent}">${BankUI.formatMoney(user.sent)}</td>
    <td class="num" data-value="${user.received}">${BankUI.formatMoney(user.received)}</td>
    <td class="num" data-value="${user.spent}">${BankUI.formatMoney(user.spent)}</td>
  </tr>`).join("");
  BankUI.enhanceTable(document.getElementById("agg-table"));
})();
