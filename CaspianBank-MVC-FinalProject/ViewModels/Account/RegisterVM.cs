namespace CaspianBank_MVC_FinalProject.ViewModels.Account
{
    // Qeydiyyatın son addımı: telefon və şifrə. Kimlik məlumatı (FİN, vəsiqə, yazılan ad və soyad) gizli sahələrlə daşınır,
    // API onları təkrar yoxlayır. Hesaba yazılan ad, soyad və doğum tarixi reyestrdən gəlir
    public class RegisterVM
    {
        public string Email { get; set; } = string.Empty;
        public string Fin { get; set; } = string.Empty;
        public string IdCardNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string VerificationToken { get; set; } = string.Empty;

        // Yalnız göstərmək üçün ("Aysel M."), API-yə göndərilmir
        public string HolderName { get; set; } = string.Empty;
    }
}
