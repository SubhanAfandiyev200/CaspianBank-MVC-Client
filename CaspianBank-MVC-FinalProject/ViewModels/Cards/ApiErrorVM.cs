namespace CaspianBank_MVC_FinalProject.ViewModels.Cards
{
    // API-nin xəta cavabı: { isSuccess: false, errors: [...] }
    public class ApiErrorVM
    {
        public bool IsSuccess { get; set; }
        public string[] Errors { get; set; } = Array.Empty<string>();
    }
}
