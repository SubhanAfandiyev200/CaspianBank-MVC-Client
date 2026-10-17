namespace CaspianBank_MVC_FinalProject.ViewModels.Bank
{
    public class DashboardMonthVM
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    // Admin: bank dashboard (api/admin/dashboard). Rəqəmlər real əməliyyatlardandır
    public class BankDashboardVM
    {
        public string Period { get; set; } = string.Empty;
        public string PeriodLabel { get; set; } = string.Empty;
        public List<DashboardMonthVM> Months { get; set; } = new();

        public int TotalUsers { get; set; }
        public int NewUsers { get; set; }
        public decimal CustomerDeposits { get; set; }
        public decimal LoansOutstanding { get; set; }

        public decimal TransferCommissions { get; set; }
        public decimal CardOpeningFees { get; set; }
        public decimal LoanInterest { get; set; }
        public decimal LatePenalties { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal? IncomeChangePercent { get; set; }

        public decimal CashbackPaid { get; set; }
        public decimal DepositInterestPaid { get; set; }
        public decimal TermDeposits { get; set; }
        public decimal TermDepositInterestOwed { get; set; }
        public decimal OperatingCosts { get; set; }
        public decimal ProfitTaxPercent { get; set; }
        public decimal ProfitTax { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal? ExpenseChangePercent { get; set; }
        public decimal ProfitBeforeTax { get; set; }
        public decimal NetProfit { get; set; }

        public decimal QuarterTaxAccrued { get; set; }
        public decimal ReservePercent { get; set; }
        public decimal RequiredReserve { get; set; }
        public decimal OwnFunds { get; set; }
        public decimal ReserveAfterTax { get; set; }
        public decimal? CoverageRatio { get; set; }
        public bool ReserveHealthy { get; set; }

        public int PendingLoans { get; set; }
        public int PendingTickets { get; set; }
        public int RestrictedAccounts { get; set; }

        public bool Unavailable { get; set; }
        public string? Error { get; set; }
    }
}
