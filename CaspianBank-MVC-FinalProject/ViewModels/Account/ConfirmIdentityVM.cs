namespace CaspianBank_MVC_FinalProject.ViewModels.Account
{
    // Qeydiyyatın 2-ci addımı: "Bu sizsiniz?" kartı
    public class ConfirmIdentityVM
    {
        public string Fin { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string MaskedSurname { get; set; } = string.Empty;
        public int BirthYear { get; set; }
        public string? PhotoUrl { get; set; }
    }
}
