namespace CaspianBank_MVC_FinalProject.ViewModels.Deposits
{
    // Admin: depozit siyahısının sətri (api/admin/deposits)
    public class DepositVM
    {
        public string Reference { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int TermMonths { get; set; }
        public decimal AnnualRate { get; set; }
        public decimal Principal { get; set; }
        public decimal ExpectedInterest { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime MaturesAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? PaidAt { get; set; }
    }

    public class DepositPageVM
    {
        public List<DepositVM> Items { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int ActiveCount { get; set; }
        public decimal ActivePrincipal { get; set; }
        public decimal ExpectedInterest { get; set; }
        public decimal PaidInterest { get; set; }
        public List<DepositPlanUIVM> Plans { get; set; } = new();

        // Süzgəc (səhifə yenilənəndə formada qalır)
        public string? Search { get; set; }
        public string? Status { get; set; }
        public bool Unavailable { get; set; }
        public string? Error { get; set; }
    }
}
