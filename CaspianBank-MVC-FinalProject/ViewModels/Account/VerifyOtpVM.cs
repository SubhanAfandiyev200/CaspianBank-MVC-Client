namespace CaspianBank_MVC_FinalProject.ViewModels.Account
{
    public class VerifyOtpVM
    {
        public string? Email { get; set; }
        public string? Code { get; set; }

        public string MaskedEmail
        {
            get
            {
                var email = Email?.Trim();
                if (string.IsNullOrEmpty(email) || !email.Contains('@'))
                    return "your email";

                var at = email.IndexOf('@');
                var local = email[..at];
                var domain = email[(at + 1)..];
                var shown = local.Length == 0 ? "*" : local[0] + "***";
                return shown + "@" + domain;
            }
        }
    }
}
