namespace CaspianBank_MVC_FinalProject.ViewModels.Security
{
    // Admin: şübhəli fəaliyyət siqnalı (api/admin/security/alerts)
    public class SecurityAlertVM
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Kind { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal? Amount { get; set; }
        public string Reference { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool AccountRestricted { get; set; }
        public string? ResolvedByName { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? ResolutionNote { get; set; }
    }

    public class SecurityPageVM
    {
        public List<SecurityAlertVM> Items { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int OpenCount { get; set; }
        public int OpenHighCount { get; set; }
        public int ResolvedThisWeek { get; set; }

        // Süzgəc (səhifə yenilənəndə formada qalır)
        public string? Status { get; set; }
        public string? Severity { get; set; }
        public string? Kind { get; set; }
        public string? Search { get; set; }
        public bool Unavailable { get; set; }
        public string? Error { get; set; }
    }
}
