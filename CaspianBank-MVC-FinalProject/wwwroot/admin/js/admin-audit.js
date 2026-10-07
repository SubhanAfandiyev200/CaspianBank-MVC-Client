(function () {
  const body = document.getElementById("audit-body");
  const query = document.getElementById("q");
  const action = document.getElementById("action");
  const from = document.getElementById("from");
  const to = document.getElementById("to");

  const actions = [...new Set(CaspianStore.get().audit.map((row) => row.action))].sort();
  action.innerHTML = `<option value="all">All actions</option>` + actions.map((name) => `<option>${BankUI.esc(name)}</option>`).join("");

  function render() {
    const q = query.value.trim().toLowerCase();
    const rows = CaspianStore.get().audit.filter((row) => {
      if (action.value !== "all" && row.action !== action.value) return false;
      if (!BankUI.inRange(row.at, from.value, to.value)) return false;
      if (!q) return true;
      return (row.user + " " + row.action + " " + row.details).toLowerCase().includes(q);
    });
    body.innerHTML = rows.length ? rows.map((row) => `<tr>
      <td data-value="${BankUI.esc(row.at)}">${BankUI.formatDateTime(row.at)}</td>
      <td data-value="${BankUI.esc(row.user)}">${BankUI.esc(row.user)}</td>
      <td data-value="${BankUI.esc(row.action)}">${BankUI.esc(row.action)}</td>
      <td data-value="${BankUI.esc(row.details)}">${BankUI.esc(row.details)}</td>
    </tr>`).join("") : `<tr class="empty-row"><td colspan="4"><div class="empty"><h3>No entries</h3><p>Nothing matches these filters.</p></div></td></tr>`;
  }

  query.addEventListener("input", render);
  action.addEventListener("change", render);
  from.addEventListener("change", render);
  to.addEventListener("change", render);
  render();
  BankUI.enhanceTable(document.getElementById("audit-table"));
})();
