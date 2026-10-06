namespace CaspianBank_MVC_FinalProject.ViewModels.Account
{
    // API-nin check-email cavabı
    public class CheckEmailResponseVM
    {
        public bool IsSuccess { get; set; }
        public string[] Errors { get; set; } = Array.Empty<string>();
        public bool Exists { get; set; }
    }
}
