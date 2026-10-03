namespace CaspianBank_MVC_FinalProject.ViewModels.Account
{
    // API-nin send-otp cavabı
    public class SendOtpResponseVM
    {
        public bool IsSuccess { get; set; }
        public string[] Errors { get; set; } = Array.Empty<string>();
        public int RetryAfterSeconds { get; set; }
        public string? DevCode { get; set; }
    }

    // API-nin verify-otp cavabı
    public class VerifyOtpResponseVM
    {
        public bool IsSuccess { get; set; }
        public string[] Errors { get; set; } = Array.Empty<string>();
        public string? VerificationToken { get; set; }
    }
}
