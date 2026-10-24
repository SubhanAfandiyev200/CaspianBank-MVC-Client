namespace CaspianBank_MVC_FinalProject.ViewModels.Reports
{
    // Müştəri: bildirilən əməliyyatın məlumatı (linkdən gəlir) və yazılan səbəb
    public class ReportFormVM
    {
        public int TransactionId { get; set; }
        public string Reference { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal? Amount { get; set; }
        public DateTime? Date { get; set; }
        public string? Reason { get; set; }
        public string? Error { get; set; }
    }

    // Müştərinin öz bildirişləri (api/reports)
    public class MyReportUIVM
    {
        public int Id { get; set; }
        public string TransactionReference { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime TransactionAt { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }

    public class MyReportsPageVM
    {
        public List<MyReportUIVM> Items { get; set; } = new();
        public bool Unavailable { get; set; }
    }

    // İşçi: bildiriş siyahısı (api/admin/reports)
    public class AdminReportVM
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ReporterId { get; set; } = string.Empty;
        public string ReporterName { get; set; } = string.Empty;
        public string ReporterEmail { get; set; } = string.Empty;
        public string TransactionReference { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime TransactionAt { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? ProposedAction { get; set; }
        public string? ActionTaken { get; set; }
        public string? ResolvedByName { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? ResolutionNote { get; set; }
    }

    public class AdminReportsPageVM
    {
        public List<AdminReportVM> Items { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int PendingCount { get; set; }
        public int ResolvedThisWeek { get; set; }

        public string? Status { get; set; }
        public string? Search { get; set; }
        public bool Unavailable { get; set; }
        public string? Error { get; set; }
    }
}
