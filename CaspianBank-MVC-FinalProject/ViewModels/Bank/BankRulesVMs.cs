namespace CaspianBank_MVC_FinalProject.ViewModels.Bank
{
    // Bankın qaydaları (api/admin/bank-rules): hər rəqəm forma üçün mətn kimi saxlanılır (onluq ayırıcı "," və "." qəbul olunur)
    public class BankRulesVM
    {
        public string ReservePercent { get; set; } = string.Empty;
        public string ProfitTaxPercent { get; set; } = string.Empty;
        public string OperatingCostMonthly { get; set; } = string.Empty;
        public string MaxInterbankAmount { get; set; } = string.Empty;
        public string MinLoanAmount { get; set; } = string.Empty;
        public string MaxLoanAmount { get; set; } = string.Empty;
        public string AnnualInterestRate { get; set; } = string.Empty;
        public string FixedPenalty { get; set; } = string.Empty;
        public string DailyPenaltyPercent { get; set; } = string.Empty;
        public string MaxActiveLoans { get; set; } = string.Empty;
        public string OfferValidDays { get; set; } = string.Empty;

        public string? Error { get; set; }
        public bool Unavailable { get; set; }
    }

    // API-dən gələn qaydalar (rəqəm kimi)
    public class BankRulesApiVM
    {
        public decimal ReservePercent { get; set; }
        public decimal ProfitTaxPercent { get; set; }
        public decimal OperatingCostMonthly { get; set; }
        public decimal MaxInterbankAmount { get; set; }
        public decimal MinLoanAmount { get; set; }
        public decimal MaxLoanAmount { get; set; }
        public decimal AnnualInterestRate { get; set; }
        public decimal FixedPenalty { get; set; }
        public decimal DailyPenaltyPercent { get; set; }
        public int MaxActiveLoans { get; set; }
        public int OfferValidDays { get; set; }
    }
}
