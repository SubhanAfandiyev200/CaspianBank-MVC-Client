namespace CaspianBank_MVC_FinalProject.ViewModels.Account
{
    // API-nin register cavabı: { "isSuccess": ..., "errors": [...] }
    public class RegisterResponseVM
    {
        public bool IsSuccess { get; set; }
        public string[] Errors { get; set; } = Array.Empty<string>();
    }
}
