namespace CaspianBank_MVC_FinalProject.ViewModels.Staff
{
    // Admin: işçi (api/admin/staff)
    public class StaffVM
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsSuperAdmin { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? InviteLink { get; set; }
    }

    public class StaffListVM
    {
        public List<StaffVM> Items { get; set; } = new();
        public List<string> AssignableRoles { get; set; } = new();
    }

    // Yeni işçi formu (xətadan sonra daxil edilənlər qalır)
    public class AddStaffFormVM
    {
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
    }

    public class StaffPageVM
    {
        public StaffListVM List { get; set; } = new();
        public AddStaffFormVM Form { get; set; } = new();
        public string? Error { get; set; }
        public bool Unavailable { get; set; }
        public string CurrentUserId { get; set; } = string.Empty;
    }
}
