namespace CaspianBank_MVC_FinalProject.ViewModels.Profile
{
    // API-dən gələn profil (api/account/profile)
    public class ProfileUIVM
    {
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string? Fin { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? PhotoUrl { get; set; }       // controller tam ünvana çevirir
    }

    public class ProfilePageVM
    {
        public ProfileUIVM? Profile { get; set; }
        public bool Unavailable { get; set; }
    }
}
