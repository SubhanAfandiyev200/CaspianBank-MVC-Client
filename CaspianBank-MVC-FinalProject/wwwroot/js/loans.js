(function () {
  const body = document.getElementById("loan-body");

  function paintQuote() {
    const amount = Number(String(document.getElementById("amount").value).replace(/,/g, ""));
    const term = Number(document.getElementById("term").value);
    const monthly = document.getElementById("loan-monthly");
    const note = document.getElementById("loan-monthly-note");
    if (!monthly || !note) return;
    if (!Number.isFinite(amount) || amount <= 0 || !term) {
      monthly.textContent = "—";
      note.textContent = "Enter an amount to see the split, before interest.";
      return;
    }
    monthly.textContent = BankUI.formatMoney(Math.round((amount / term) * 100) / 100);
    note.textContent = "Split over " + term + " months, before interest.";
  }

  function render() {
    const loans = CaspianStore.get().loans;
    const stats = document.getElementById("loan-stats");
    if (stats) {
      const pending = loans.filter((loan) => loan.status === "Pending").length;
      const requested = loans.reduce((sum, loan) => sum + Number(loan.amount || 0), 0);
      stats.innerHTML = `
        <article class="pulse"><span>Applications</span><strong>${loans.length}</strong></article>
        <article class="pulse"><span>Requested</span><strong>${BankUI.formatMoney(requested)}</strong></article>
        <article class="pulse"><span>Pending</span><strong>${pending}</strong></article>`;
    }
    body.innerHTML = loans.length ? loans.map((loan) => `<tr>
      <td data-value="${BankUI.esc(loan.submitted)}">${BankUI.formatDate(loan.submitted)}</td>
      <td class="num" data-value="${loan.amount}">${BankUI.formatMoney(loan.amount)}</td>
      <td data-value="${BankUI.esc(loan.purpose || "")}">${BankUI.esc(loan.purpose || "—")}</td>
      <td data-value="${loan.term}">${loan.term} months</td>
      <td data-value="${BankUI.esc(loan.status)}">${BankUI.badge(loan.status)}</td>
    </tr>`).join("") : `<tr class="empty-row"><td colspan="5"><div class="empty"><h3>No applications</h3><p>When you apply, the status will appear here.</p></div></td></tr>`;
  }

  document.getElementById("loan-form").addEventListener("submit", async (event) => {
    event.preventDefault();
    const form = event.target;
    const amountInput = document.getElementById("amount");
    const amount = Number(String(amountInput.value).replace(/,/g, ""));
    BankUI.setFieldError(amountInput, "");
    if (!Number.isFinite(amount) || amount < 100) {
      BankUI.setFieldError(amountInput, "Enter at least 100.00 AZN.");
      return;
    }
    const purpose = document.getElementById("purpose").value.trim();
    const term = Number(document.getElementById("term").value);
    const button = form.querySelector("button[type=submit]");
    BankUI.setBusy(button, true);
    // TODO: POST /api/loans
    await BankUI.request("/api/loans", { method: "POST", body: JSON.stringify({ amount, purpose, term }) });
    CaspianStore.update((state) => {
      const loan = {
        id: BankUI.uid("l"),
        applicant: state.session.name,
        userId: state.users.find((user) => user.email.toLowerCase() === state.profile.email.toLowerCase())?.id || "u1",
        amount: Math.round(amount * 100) / 100,
        purpose,
        term,
        status: "Pending",
        submitted: new Date().toISOString()
      };
      state.loans.unshift(loan);
      state.adminLoans.unshift({ ...loan });
      CaspianStore.logAudit(state, "Loan submitted", BankUI.formatMoney(loan.amount) + " · " + term + " months");
      CaspianStore.pushNote(state, {
        type: "loan",
        title: "Loan application received",
        body: "Your request for " + BankUI.formatMoney(loan.amount) + " is pending review."
      });
    });
    form.reset();
    paintQuote();
    render();
    BankUI.setBusy(button, false);
    BankUI.refreshChrome();
    BankUI.toast("Application submitted.");
  });

  document.getElementById("amount").addEventListener("input", paintQuote);
  document.getElementById("term").addEventListener("change", paintQuote);
  paintQuote();
  render();
  BankUI.enhanceTable(document.getElementById("loan-table"));
})();
