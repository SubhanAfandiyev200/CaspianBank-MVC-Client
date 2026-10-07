(function () {
  const body = document.getElementById("report-body");
  const status = document.getElementById("status");

  function render() {
    const rows = CaspianStore.get().reports.filter((report) => status.value === "all" || report.status === status.value);
    body.innerHTML = rows.length ? rows.map((report) => `<tr>
      <td data-value="${BankUI.esc(report.reporter)}">${BankUI.esc(report.reporter)}</td>
      <td data-value="${BankUI.esc(report.reference)}">${BankUI.esc(report.reference)}</td>
      <td data-value="${BankUI.esc(report.reason)}">${BankUI.esc(report.reason)}</td>
      <td data-value="${BankUI.esc(report.at)}">${BankUI.formatDateTime(report.at)}</td>
      <td data-value="${BankUI.esc(report.status)}">${BankUI.badge(report.status)}${report.reviewed && report.status === "Pending" ? ' <span class="hint">Reviewed</span>' : ""}</td>
      <td class="actions">${report.status === "Pending" ? `
        <button type="button" class="btn btn-secondary btn-sm" data-review="${report.id}" ${report.reviewed ? "disabled" : ""}>Mark reviewed</button>
        <button type="button" class="btn btn-danger btn-sm" data-action="${report.id}">Take action</button>
        <button type="button" class="btn btn-secondary btn-sm" data-dismiss="${report.id}">Dismiss</button>` : ""}</td>
    </tr>`).join("") : `<tr class="empty-row"><td colspan="6"><div class="empty"><h3>No reports</h3><p>Nothing in this status.</p></div></td></tr>`;
  }

  body.addEventListener("click", async (event) => {
    return;
    const review = event.target.closest("[data-review]");
    const action = event.target.closest("[data-action]");
    const dismiss = event.target.closest("[data-dismiss]");
    const button = review || action || dismiss;
    if (!button) return;
    const id = button.getAttribute("data-review") || button.getAttribute("data-action") || button.getAttribute("data-dismiss");
    const report = CaspianStore.get().reports.find((item) => item.id === id);
    if (!report) return;

    if (review) {
      // TODO: POST /api/admin/reports/{id}/review
      await BankUI.request("/api/admin/reports/" + id + "/review", { method: "POST" });
      CaspianStore.update((state) => {
        const target = state.reports.find((item) => item.id === id);
        target.reviewed = true;
        CaspianStore.logAudit(state, "Suspicious report", "Reviewed " + target.reference);
      });
      render();
      BankUI.toast("Marked as reviewed.");
      return;
    }

    const take = !!action;
    const yes = await BankUI.confirm({
      title: take ? "Restrict the linked account?" : "Dismiss this report?",
      message: take
        ? "Taking action restricts " + report.reporter + "'s account. Reference " + report.reference + "."
        : "The report will be closed with no change to the account.",
      confirmText: take ? "Take action" : "Dismiss",
      danger: take
    });
    if (!yes) return;
    // TODO: POST /api/admin/reports/{id}
    await BankUI.request("/api/admin/reports/" + id, { method: "POST", body: JSON.stringify({ status: take ? "Action Taken" : "Dismissed" }) });
    CaspianStore.update((state) => {
      const target = state.reports.find((item) => item.id === id);
      target.status = take ? "Action Taken" : "Dismissed";
      target.reviewed = true;
      if (take) {
        const user = state.users.find((item) => item.id === target.userId);
        if (user) user.status = "Restricted";
      }
      CaspianStore.logAudit(state, take ? "Account restricted" : "Suspicious report", (take ? "Action on " : "Dismissed ") + target.reference);
    });
    render();
    BankUI.toast(take ? "Account restricted." : "Report dismissed.");
  });

  status.addEventListener("change", render);
  render();
  BankUI.enhanceTable(document.getElementById("report-table"));
})();
