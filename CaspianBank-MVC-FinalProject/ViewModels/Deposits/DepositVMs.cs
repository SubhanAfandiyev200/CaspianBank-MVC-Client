using CaspianBank_MVC_FinalProject.ViewModels.Cards;

namespace CaspianBank_MVC_FinalProject.ViewModels.Deposits
{
    // API-dən gələn depozit variantı (api/deposits)
    public class DepositPlanUIVM
    {
        public int Id { get; set; }
        public int TermMonths { get; set; }
        public decimal AnnualRate { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public bool IsActive { get; set; }
    }

    public class DepositUIVM
    {
        public string Reference { get; set; } = string.Empty;
        public int TermMonths { get; set; }
        public decimal AnnualRate { get; set; }
        public decimal Principal { get; set; }
        public decimal ExpectedInterest { get; set; }
        public decimal AccruedInterest { get; set; }
        public List<DepositPostingUIVM> Postings { get; set; } = new();
        public decimal TotalAtMaturity { get; set; }
        public int ProgressPercent { get; set; }
        public int DaysLeft { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime MaturesAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? PaidAt { get; set; }
        public string SourceCardLabel { get; set; } = string.Empty;
        public string PayoutCardLabel { get; set; } = string.Empty;
    }

    public class DepositPostingUIVM
    {
        public int Number { get; set; }
        public decimal Amount { get; set; }
        public DateTime PostedAt { get; set; }
    }

    public class DepositsOverviewUIVM
    {
        public List<DepositPlanUIVM> Plans { get; set; } = new();
        public List<DepositUIVM> Deposits { get; set; } = new();
        public decimal ActivePrincipal { get; set; }
        public decimal ActiveExpectedInterest { get; set; }
        public decimal InterestEarned { get; set; }
    }

    // Depozit açma formu (xətadan sonra daxil edilənlər saxlanılır)
    public class DepositFormVM
    {
        public int PlanId { get; set; }
        public string? Amount { get; set; }
        public int SourceCardId { get; set; }
        public int PayoutCardId { get; set; }
        public string RequestId { get; set; } = Guid.NewGuid().ToString();
    }

    public class DepositIndexVM
    {
        public DepositsOverviewUIVM Overview { get; set; } = new();
        public List<CardUIVM> Cards { get; set; } = new();
        public DepositFormVM Form { get; set; } = new();
        public string? Error { get; set; }
        public bool ApiUnavailable { get; set; }
    }
}
