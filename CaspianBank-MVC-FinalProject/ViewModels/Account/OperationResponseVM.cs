namespace CaspianBank_MVC_FinalProject.ViewModels.Account
{
    // API-nin sadə "uğurlu/uğursuz + xətalar" cavabı (forgot-password, reset-password)
    public class OperationResponseVM
    {
        public bool IsSuccess { get; set; }
        public string[] Errors { get; set; } = Array.Empty<string>();
        public string? DevLink { get; set; }
    }
}
