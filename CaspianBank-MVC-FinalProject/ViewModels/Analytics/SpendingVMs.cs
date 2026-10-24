namespace CaspianBank_MVC_FinalProject.ViewModels.Analytics
{
    // API-dən gələn xərc analitikası (api/analytics/spending)
    public class SpendingCategoryUIVM
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class SpendingMonthUIVM
    {
        public string Key { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public List<SpendingCategoryUIVM> Categories { get; set; } = new();
    }

    public class SpendingUIVM
    {
        public List<SpendingMonthUIVM> Months { get; set; } = new();
        public decimal CurrentTotal { get; set; }
        public decimal PreviousTotal { get; set; }
        public decimal? ChangePercent { get; set; }
        public string? TopCategory { get; set; }
        public decimal AverageMonthly { get; set; }
    }

    public class SpendingPageVM
    {
        public SpendingUIVM Data { get; set; } = new();
        public int Months { get; set; } = 6;
        public bool Unavailable { get; set; }
    }
}
