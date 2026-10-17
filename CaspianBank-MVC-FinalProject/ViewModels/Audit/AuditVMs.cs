namespace CaspianBank_MVC_FinalProject.ViewModels.Audit
{
    // Admin: audit log sətri (api/admin/audit)
    public class AuditVM
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ActorName { get; set; } = string.Empty;
        public string ActorRole { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string TargetType { get; set; } = string.Empty;
        public string TargetLabel { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }

    public class AuditPageVM
    {
        public List<AuditVM> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }

        // Süzgəc (səhifə yenilənəndə formada qalır)
        public string? Search { get; set; }
        public string? Action { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public bool Unavailable { get; set; }
        public string? Error { get; set; }

        public int TotalPages
        {
            get
            {
                return PageSize <= 0 ? 1 : Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
            }
        }
    }
}
