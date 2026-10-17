namespace CaspianBank_MVC_FinalProject.ViewModels.Users
{
    // Admin: müştərilərin siyahısının sətri (api/admin/users)
    public class UserVM
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Fin { get; set; }
        public DateTime RegisteredAt { get; set; }
        public bool IsRestricted { get; set; }
        public int CardCount { get; set; }
        public decimal TotalBalance { get; set; }
    }

    public class UserPageVM
    {
        public List<UserVM> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int RestrictedCount { get; set; }

        // Süzgəc (səhifə yenilənəndə formada qalır)
        public string? Search { get; set; }
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

    public class UserCardVM
    {
        public int Id { get; set; }
        public string Tier { get; set; } = string.Empty;
        public string Last4 { get; set; } = string.Empty;
        public string Expiry { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public bool IsBlocked { get; set; }
    }

    public class UserLoanVM
    {
        public int Id { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal OutstandingPrincipal { get; set; }
    }

    public class UserActivityVM
    {
        public DateTime CreatedAt { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string CardLabel { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public bool IsIncome { get; set; }
    }

    public class UserDetailVM
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Fin { get; set; }
        public DateTime BirthDate { get; set; }
        public DateTime RegisteredAt { get; set; }
        public bool IsRestricted { get; set; }
        public string? RestrictionReason { get; set; }
        public DateTime? RestrictedAt { get; set; }
        public string? RestrictedBy { get; set; }
        public decimal TotalSent { get; set; }
        public int BillPaymentCount { get; set; }
        public decimal BillPaymentTotal { get; set; }
        public List<UserCardVM> Cards { get; set; } = new();
        public List<UserLoanVM> Loans { get; set; } = new();
        public List<UserActivityVM> RecentActivity { get; set; } = new();

        // Dondurma formasında xəta olanda yazılan səbəb saxlanır (API qaytarmır, controller doldurur)
        public string? Error { get; set; }
        public string? ReasonDraft { get; set; }

        public string FullName
        {
            get
            {
                return Name + " " + Surname;
            }
        }
    }
}
