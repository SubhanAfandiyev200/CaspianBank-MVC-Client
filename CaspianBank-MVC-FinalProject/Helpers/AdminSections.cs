namespace CaspianBank_MVC_FinalProject.Helpers
{
    // Admin panelinin bölmələri və hansı rolların görə biləcəyi.
    // Yeni admin səhifəsi yazanda ona uyğun sətir bura əlavə edilir; Admin/Index səhifəsi bu siyahıdan "kim nəyi görür" cədvəlini çəkir.
    public class AdminSection
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string[] Roles { get; set; } = Array.Empty<string>();
        public string? Url { get; set; }        // səhifə yazılıbsa ünvanı; yoxdursa "tezliklə"
    }

    public static class AdminSections
    {
        public static readonly AdminSection[] All =
        {
            new AdminSection
            {
                Title = "Home content",
                Description = "Ticker, brands, about, services, benefits, hero, site settings",
                Roles = new[] { AppRoles.Admin, AppRoles.SuperAdmin }
            },
            new AdminSection
            {
                Title = "Card designs",
                Description = "Card face images shown on Home and on customer cards",
                Roles = new[] { AppRoles.Admin, AppRoles.SuperAdmin }
            },
            new AdminSection
            {
                Title = "Users",
                Description = "Customer list and details",
                Roles = new[] { AppRoles.CustomerSupport, AppRoles.Security, AppRoles.Admin, AppRoles.SuperAdmin }
            },
            new AdminSection
            {
                Title = "Freeze accounts",
                Description = "Restrict or restore a customer account",
                Roles = new[] { AppRoles.Security, AppRoles.Admin, AppRoles.SuperAdmin }
            },
            new AdminSection
            {
                Title = "Transactions and reports",
                Description = "All transfers, top ups, reported transactions",
                Roles = new[] { AppRoles.Accountant, AppRoles.Admin, AppRoles.SuperAdmin }
            },
            new AdminSection
            {
                Title = "Loans",
                Description = "Review loan applications: approve or decline",
                Roles = new[] { AppRoles.Accountant, AppRoles.Admin, AppRoles.SuperAdmin }
            },
            new AdminSection
            {
                Title = "Audit log",
                Description = "Who did what and when",
                Roles = new[] { AppRoles.Security, AppRoles.Admin, AppRoles.SuperAdmin }
            },
            new AdminSection
            {
                Title = "Card tier rules",
                Description = "Fees, limits and commission for each card type",
                Roles = new[] { AppRoles.Admin, AppRoles.SuperAdmin }
            },
            new AdminSection
            {
                Title = "Staff and roles",
                Description = "Create staff accounts and assign roles",
                Roles = new[] { AppRoles.SuperAdmin }
            }
        };
    }
}
