(function () {
  const body = document.getElementById("ticket-body");
  const status = document.getElementById("status");

  function render() {
    const rows = CaspianStore.get().tickets.filter((ticket) => status.value === "all" || ticket.status === status.value);
    body.innerHTML = rows.length ? rows.map((ticket) => `<tr>
      <td data-value="${BankUI.esc(ticket.name)}">${BankUI.esc(ticket.name)}</td>
      <td data-value="${BankUI.esc(ticket.subject)}"><a href="admin-contact-tickets.html?id=${ticket.id}">${BankUI.esc(ticket.subject)}</a></td>
      <td data-value="${BankUI.esc(ticket.created)}">${BankUI.formatDateTime(ticket.created)}</td>
      <td data-value="${BankUI.esc(ticket.status)}">${BankUI.badge(ticket.status)}</td>
    </tr>`).join("") : `<tr class="empty-row"><td colspan="4"><div class="empty"><h3>No tickets</h3><p>Nothing in this status.</p></div></td></tr>`;
  }

  status.addEventListener("change", render);
  render();
  BankUI.enhanceTable(document.getElementById("ticket-table"));
})();
