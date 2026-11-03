namespace CaspianBank_MVC_FinalProject.ViewModels.Cards
{
    // Admin: kartların (hesabların) reyestri, api/admin/cards. Yalnız oxuma
    public class AdminCardVM
    {
        public int Id { get; set; }
        public string OwnerId { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string OwnerEmail { get; set; } = string.Empty;
        public string Tier { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public string Last4 { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsExpired { get; set; }
        public string Expiry { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class AdminCardPageVM
    {
        public List<AdminCardVM> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public decimal FilteredBalance { get; set; }
        public int AllCount { get; set; }
        public int BlockedCount { get; set; }
        public int ExpiredCount { get; set; }

        // Süzgəc (səhifə yenilənəndə formada qalır)
        public string? Search { get; set; }
        public string? Tier { get; set; }
        public string? Status { get; set; }
        public bool Unavailable { get; set; }

        public int TotalPages
        {
            get
            {
                return PageSize <= 0 ? 1 : Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
            }
        }
    }
}
