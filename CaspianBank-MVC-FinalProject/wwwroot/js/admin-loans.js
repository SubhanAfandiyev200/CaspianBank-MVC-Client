(function () {
  const body = document.getElementById("loan-body");
  const status = document.getElementById("status");

  function render() {
    const rows = CaspianStore.get().adminLoans.filter((loan) => status.value === "all" || loan.status === status.value);
    body.innerHTML = rows.length ? rows.map((loan) => `<tr>
      <td data-value="${BankUI.esc(loan.applicant)}">${BankUI.esc(loan.applicant)}</td>
      <td class="num" data-value="${loan.amount}">${BankUI.formatMoney(loan.amount)}</td>
      <td data-value="${BankUI.esc(loan.purpose || "")}">${BankUI.esc(loan.purpose || "—")}</td>
      <td data-value="${loan.term}">${loan.term} months</td>
      <td data-value="${BankUI.esc(loan.submitted)}">${BankUI.formatDate(loan.submitted)}</td>
      <td data-value="${BankUI.esc(loan.status)}">${BankUI.badge(loan.status)}</td>
      <td class="actions">${loan.status === "Pending" ? `
        <button type="button" class="btn btn-primary btn-sm" data-approve="${loan.id}">Approve</button>
        <button type="button" class="btn btn-danger btn-sm" data-decline="${loan.id}">Decline</button>` : ""}</td>
    </tr>`).join("") : `<tr class="empty-row"><td colspan="7"><div class="empty"><h3>No applications</h3><p>Nothing in this status.</p></div></td></tr>`;
  }

  body.addEventListener("click", async (event) => {
    return;
    const approve = event.target.closest("[data-approve]");
    const decline = event.target.closest("[data-decline]");
    const button = approve || decline;
    if (!button) return;
    const id = button.getAttribute(approve ? "data-approve" : "data-decline");
    const loan = CaspianStore.get().adminLoans.find((item) => item.id === id);
    if (!loan) return;
    const nextStatus = approve ? "Approved" : "Declined";
    const yes = await BankUI.confirm({
      title: nextStatus === "Approved" ? "Approve this loan?" : "Decline this loan?",
      message: loan.applicant + " requested " + BankUI.formatMoney(loan.amount) + " over " + loan.term + " months.",
      confirmText: nextStatus === "Approved" ? "Approve" : "Decline",
      danger: nextStatus === "Declined"
    });
    if (!yes) return;
    // TODO: POST /api/admin/loans/{id}  { status }
    await BankUI.request("/api/admin/loans/" + id, { method: "POST", body: JSON.stringify({ status: nextStatus }) });
    CaspianStore.update((state) => {
      const target = state.adminLoans.find((item) => item.id === id);
      target.status = nextStatus;
      const customer = state.loans.find((item) => item.id === id);
      if (customer) customer.status = nextStatus;
      CaspianStore.logAudit(state, "Loan decision", nextStatus + " " + BankUI.formatMoney(loan.amount) + " for " + loan.applicant);
      if (state.session && state.session.name === loan.applicant) {
        CaspianStore.pushNote(state, {
          type: "loan",
          title: "Loan " + nextStatus.toLowerCase(),
          body: "Your request for " + BankUI.formatMoney(loan.amount) + " was " + nextStatus.toLowerCase() + "."
        });
      }
    });
    render();
    BankUI.toast("Loan " + nextStatus.toLowerCase() + ".");
  });

  status.addEventListener("change", render);
  render();
  BankUI.enhanceTable(document.getElementById("loan-table"));
})();
