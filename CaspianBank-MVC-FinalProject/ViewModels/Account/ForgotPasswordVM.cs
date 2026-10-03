namespace CaspianBank_MVC_FinalProject.ViewModels.Account
{
    public class ForgotPasswordVM
    {
        public string Email { get; set; } = string.Empty;

        // true: link göndərildi, "Check your email" vəziyyəti göstərilir
        public bool Sent { get; set; }
        public string MaskedEmail { get; set; } = string.Empty;

        // Yalnız SMTP qurulmayıb və API Development rejimindədir: reset linki ekranda göstərilir
        public string? DevLink { get; set; }
    }
}
