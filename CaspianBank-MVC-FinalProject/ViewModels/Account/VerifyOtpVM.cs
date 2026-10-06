namespace CaspianBank_MVC_FinalProject.ViewModels.Account
{
    public class VerifyOtpVM
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string MaskedEmail { get; set; } = string.Empty;
        public string? DevCode { get; set; }
    }
}
