using System.Security.Claims;

namespace CaspianBank_MVC_FinalProject.Helpers
{
    // Rol adları (API-dəki Domain/Constants/Roles.cs ilə eyni olmalıdır: MVC layihəsi Domain-i görmür, ona görə təkrar yazılıb)
    public static class AppRoles
    {
        public const string Customer = "Customer";
        public const string SuperAdmin = "SuperAdmin";
        public const string Admin = "Admin";
        public const string Accountant = "Accountant";
        public const string WebDesigner = "WebDesigner";
        public const string CustomerSupport = "CustomerSupport";
        public const string Security = "Security";

        // [Authorize(Roles = AppRoles.Staff)]: Customer-dən başqa hər rol (işçilər)
        public const string Staff = SuperAdmin + "," + Admin + "," + Accountant + "," + WebDesigner + "," + CustomerSupport + "," + Security;

        public static readonly string[] StaffRoles =
        {
            SuperAdmin, Admin, Accountant, WebDesigner, CustomerSupport, Security
        };

        public static bool IsStaff(IEnumerable<string> roles)
        {
            return roles.Any(role => StaffRoles.Contains(role));
        }

        public static bool IsStaff(ClaimsPrincipal user)
        {
            return IsStaff(user.FindAll(ClaimTypes.Role).Select(claim => claim.Value));
        }
    }
}
