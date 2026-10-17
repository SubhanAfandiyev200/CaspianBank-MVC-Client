namespace CaspianBank_MVC_FinalProject.ViewModels.Account
{
    // API-nin check-identity cavabı
    public class IdentityResponseVM
    {
        public bool IsSuccess { get; set; }
        public string[] Errors { get; set; } = Array.Empty<string>();
        public string Name { get; set; } = string.Empty;
        public string MaskedSurname { get; set; } = string.Empty;
        public int BirthYear { get; set; }
        public string? PhotoUrl { get; set; }
    }
}
