namespace CaspianBank_MVC_FinalProject.ViewModels.Bank
{
    // Admin: xəzinə jurnalının sətri (api/admin/treasury)
    public class TreasuryEntryVM
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Kind { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
    }

    public class TreasuryPageVM
    {
        public decimal Balance { get; set; }
        public decimal CustomerMoney { get; set; }
        public decimal ReservePercent { get; set; }
        public decimal RequiredReserve { get; set; }
        public decimal AvailableForLoans { get; set; }
        public decimal MonthIn { get; set; }
        public decimal MonthOut { get; set; }
        public string MonthLabel { get; set; } = string.Empty;
        public List<TreasuryEntryVM> Items { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }

        // Süzgəc (səhifə yenilənəndə formada qalır)
        public string? Kind { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public bool Unavailable { get; set; }
        public string? Error { get; set; }
    }
}
