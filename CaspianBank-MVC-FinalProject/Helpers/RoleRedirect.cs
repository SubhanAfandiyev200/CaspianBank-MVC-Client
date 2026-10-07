namespace CaspianBank_MVC_FinalProject.Helpers
{
    // Login-dən sonra hara getməli: işçi admin panelinə, müştəri öz kartlarına
    public static class RoleRedirect
    {
        public const string AdminHome = "/Admin";
        public const string CustomerHome = "/App";

        public static string HomeUrl(IEnumerable<string> roles)
        {
            if (AppRoles.IsStaff(roles))
            {
                return AdminHome;
            }

            return CustomerHome;
        }
    }
}
