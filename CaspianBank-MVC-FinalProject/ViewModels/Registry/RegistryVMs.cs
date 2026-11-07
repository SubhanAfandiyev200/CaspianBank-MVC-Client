namespace CaspianBank_MVC_FinalProject.ViewModels.Registry
{
    // Admin: dövlət reyestri, api/admin/registry. Yalnız oxuma
    public class RegistryCitizenVM
    {
        public string Fin { get; set; } = string.Empty;
        public string IdCardNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string FatherName { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
        public int Age { get; set; }
        public string ResidenceAddress { get; set; } = string.Empty;
        public bool IsBlacklisted { get; set; }
        public string? BlacklistReason { get; set; }
        public bool IsDeceased { get; set; }
        public DateTime? DeathDate { get; set; }
        public string? AccountUserId { get; set; }
        public string? PhotoUrl { get; set; }
    }

    public class RegistryPageVM
    {
        public List<RegistryCitizenVM> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int AllCount { get; set; }
        public int BlacklistedCount { get; set; }
        public int DeceasedCount { get; set; }

        // Süzgəc (səhifə yenilənəndə formada qalır)
        public string? Search { get; set; }
        public string? Status { get; set; }
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
