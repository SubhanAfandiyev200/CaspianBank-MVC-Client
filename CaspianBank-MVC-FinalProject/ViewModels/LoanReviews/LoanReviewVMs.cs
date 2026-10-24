using CaspianBank_MVC_FinalProject.ViewModels.Loans;

namespace CaspianBank_MVC_FinalProject.ViewModels.LoanReviews
{
    // İşçinin kredit siyahısındakı sətir (api/admin/loans)
    public class LoanReviewVM
    {
        public int Id { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public string ApplicantName { get; set; } = string.Empty;
        public string? Fin { get; set; }
        public string Purpose { get; set; } = string.Empty;
        public decimal RequestedAmount { get; set; }
        public decimal? ApprovedAmount { get; set; }
        public int TermMonths { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool RequiresSuperAdmin { get; set; }
    }

    public class LoanReviewPageVM
    {
        public List<LoanReviewVM> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int PendingCount { get; set; }
        public int AwaitingSuperAdminCount { get; set; }

        // Süzgəc (səhifə yenilənəndə formada qalır)
        public string? Status { get; set; }
        public string? Search { get; set; }
        public bool Unavailable { get; set; }

        public int TotalPages
        {
            get
            {
                return PageSize <= 0 ? 1 : Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
            }
        }
    }

    public class LoanApplicantVM
    {
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string? FatherName { get; set; }
        public string? Fin { get; set; }
        public DateTime BirthDate { get; set; }
        public int Age { get; set; }
        public string? ResidenceAddress { get; set; }
        public string? PhotoUrl { get; set; }       // controller tam ünvana çevirir
        public bool IsBlacklisted { get; set; }
        public string? BlacklistReason { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public bool AccountRestricted { get; set; }
    }

    public class ApplicantEmploymentVM
    {
        public string Workplace { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public decimal MonthlySalary { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? LeaveReason { get; set; }
        public bool IsCurrent { get; set; }
    }

    public class ApplicantExternalLoanVM
    {
        public string Bank { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public decimal BorrowedAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal MonthlyPayment { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsClosed { get; set; }
        public int DelayCount { get; set; }
        public int MaxDelayDays { get; set; }
        public decimal OverdueAmount { get; set; }
        public bool WentToCourt { get; set; }
    }

    public class ExternalLoanTotalsVM
    {
        public decimal TotalBorrowed { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalRemaining { get; set; }
        public decimal TotalOverdue { get; set; }
        public int OpenCount { get; set; }
        public int TotalDelays { get; set; }
        public int CourtCount { get; set; }
        public decimal OpenMonthlyPayments { get; set; }
    }

    public class ApplicantCaspianLoanVM
    {
        public int Id { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal OutstandingPrincipal { get; set; }
        public decimal MonthlyPayment { get; set; }
        public int OverdueInstallments { get; set; }
        public decimal OverdueAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool ClosedEarly { get; set; }
    }

    // Qaydalara görə hesablanmış rəqəmlər: işçinin qərarına kömək edir, məcbur etmir
    public class LoanReviewRulesVM
    {
        public decimal CurrentIncome { get; set; }
        public int DelayMultiplier { get; set; }
        public decimal MaxByIncome { get; set; }
        public decimal? MaxByPaymentLimit { get; set; }
        public decimal SuggestedMax { get; set; }
        public decimal SuggestedApproval { get; set; }
        public decimal ExistingMonthlyPayments { get; set; }
        public decimal PaymentToIncomePercent { get; set; }
        public decimal? PaymentLimitPercent { get; set; }
        public int ActiveLoanCount { get; set; }
        public int MaxActiveLoans { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public decimal RequestedMonthlyPayment { get; set; }
        public List<string> Warnings { get; set; } = new();
    }

    // İşçinin kredit səhifəsi (api/admin/loans/{id})
    public class LoanReviewDetailVM
    {
        public LoanDetailUIVM Loan { get; set; } = new();
        public LoanApplicantVM Applicant { get; set; } = new();
        public List<ApplicantEmploymentVM> Employments { get; set; } = new();
        public List<ApplicantExternalLoanVM> ExternalLoans { get; set; } = new();
        public ExternalLoanTotalsVM ExternalTotals { get; set; } = new();
        public List<ApplicantCaspianLoanVM> CaspianLoans { get; set; } = new();
        public LoanReviewRulesVM Rules { get; set; } = new();
        public string? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? RejectReason { get; set; }
        public bool CanDecide { get; set; }

        // Qara siyahı: iki təsdiq. Stage: Normal, FirstApproval (kredit şöbəsi), FinalApproval (Super Admin)
        public bool RequiresSuperAdminApproval { get; set; }
        public string Stage { get; set; } = "Normal";
        public string? FirstApprovedBy { get; set; }
        public DateTime? FirstApprovedAt { get; set; }
        public decimal? FirstApprovedAmount { get; set; }

        // Formada qalan dəyərlər (xətadan sonra yenidən göstərmək üçün)
        public string? ApprovedAmountText { get; set; }
        public string? RejectReasonText { get; set; }
        public string? Error { get; set; }
    }
}
