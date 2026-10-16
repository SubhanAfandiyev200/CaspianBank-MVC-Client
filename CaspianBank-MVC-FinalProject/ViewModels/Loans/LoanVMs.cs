using CaspianBank_MVC_FinalProject.ViewModels.Cards;

namespace CaspianBank_MVC_FinalProject.ViewModels.Loans
{
    // API-dən gələn müraciət qaydaları (api/loans/options)
    public class LoanOptionsUIVM
    {
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public decimal AnnualInterestRate { get; set; }
        public int[] TermOptions { get; set; } = Array.Empty<int>();
        public int MaxActiveLoans { get; set; }
        public int OpenLoanCount { get; set; }
        public bool CanApply { get; set; }
        public string? CannotApplyReason { get; set; }
        public decimal MinEarlyPayment { get; set; }
        public decimal EarlyRepaymentMinPercent { get; set; }
        public decimal EarlyRepaymentRateReduction { get; set; }
        public decimal MinAnnualInterestRate { get; set; }
    }

    // Siyahıdakı bir kredit və ya müraciət (api/loans)
    public class LoanSummaryUIVM
    {
        public int Id { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public decimal RequestedAmount { get; set; }
        public decimal? ApprovedAmount { get; set; }
        public int TermMonths { get; set; }
        public decimal MonthlyPayment { get; set; }
        public decimal OutstandingPrincipal { get; set; }
        public decimal AnnualInterestRate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DisbursedAt { get; set; }
        public DateTime? OfferExpiresAt { get; set; }
        public DateTime? NextDueDate { get; set; }
        public decimal NextDueAmount { get; set; }
        public bool HasOverdue { get; set; }
        public string? Message { get; set; }
    }

    public class LoanDueUIVM
    {
        public decimal Total { get; set; }
        public decimal Penalty { get; set; }
        public decimal Interest { get; set; }
        public decimal Principal { get; set; }
        public decimal OverdueTotal { get; set; }
        public int OverdueCount { get; set; }
        public int InstallmentCount { get; set; }
        public DateTime? NextDueDate { get; set; }
    }

    public class LoanInstallmentUIVM
    {
        public int Number { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Principal { get; set; }
        public decimal Interest { get; set; }
        public decimal Payment { get; set; }
        public decimal Penalty { get; set; }
        public decimal Paid { get; set; }
        public decimal Remaining { get; set; }
        public string Status { get; set; } = string.Empty;     // Paid, Prepaid, Upcoming, Overdue
        public int DaysOverdue { get; set; }
    }

    public class LoanPaymentUIVM
    {
        public DateTime PaidAt { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Penalty { get; set; }
        public decimal Interest { get; set; }
        public decimal Principal { get; set; }
        public decimal? InterestRateAfter { get; set; }
        public string CardLabel { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
    }

    // Bir kreditin səhifəsi (api/loans/{id})
    public class LoanDetailUIVM : LoanSummaryUIVM
    {
        public string? PurposeNote { get; set; }
        public decimal TotalRepayment { get; set; }
        public decimal InitialAnnualInterestRate { get; set; }
        public string TargetCardLabel { get; set; } = string.Empty;
        public int TargetCardId { get; set; }
        public DateTime? ClosedAt { get; set; }
        public bool ClosedEarly { get; set; }
        public decimal FixedPenalty { get; set; }
        public decimal DailyPenaltyPercent { get; set; }

        public List<LoanInstallmentUIVM> Installments { get; set; } = new();
        public List<LoanPaymentUIVM> Payments { get; set; } = new();

        public LoanDueUIVM Due { get; set; } = new();
        public decimal PayoffAmount { get; set; }
        public bool CanPayInstallment { get; set; }
        public bool CanPartialEarly { get; set; }
        public bool CanPayOff { get; set; }
        public string? EarlyBlockedReason { get; set; }

        public decimal MinEarlyPayment { get; set; }
        public decimal EarlyRepaymentMinPercent { get; set; }
        public decimal EarlyRepaymentRateReduction { get; set; }
        public decimal MinAnnualInterestRate { get; set; }
    }

    public class LoanReceiptUIVM
    {
        public string Reference { get; set; } = string.Empty;
        public string ContractNumber { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Penalty { get; set; }
        public decimal Interest { get; set; }
        public decimal Principal { get; set; }
        public string CardLabel { get; set; } = string.Empty;
        public decimal CardBalanceAfter { get; set; }
        public bool LoanClosed { get; set; }
        public decimal? NewInterestRate { get; set; }
        public DateTime PaidAt { get; set; }

        // Qəbz səhifəsi kreditə qayıtsın deyə (API qaytarmır, controller doldurur)
        public int LoanId { get; set; }
    }

    // Müraciət formu (xətadan sonra daxil edilənlər saxlanır)
    public class LoanApplyFormVM
    {
        public string? Amount { get; set; }
        public string Purpose { get; set; } = "Consumer";
        public string? PurposeNote { get; set; }
        public int TermMonths { get; set; } = 24;
        public int TargetCardId { get; set; }
    }

    // Kreditlər səhifəsi: müraciət formu + mənim kreditlərim
    public class LoanIndexVM
    {
        public LoanOptionsUIVM Options { get; set; } = new();
        public List<LoanSummaryUIVM> Loans { get; set; } = new();
        public List<CardUIVM> Cards { get; set; } = new();
        public LoanApplyFormVM Form { get; set; } = new();
        public string? Error { get; set; }
        public bool ApiUnavailable { get; set; }
    }

    // Bir kreditin səhifəsi: kredit + ödəniş üçün kartlar
    public class LoanDetailPageVM
    {
        public LoanDetailUIVM Loan { get; set; } = new();
        public List<CardUIVM> Cards { get; set; } = new();
        public string RequestId { get; set; } = Guid.NewGuid().ToString();
        public string? Error { get; set; }
    }
}
