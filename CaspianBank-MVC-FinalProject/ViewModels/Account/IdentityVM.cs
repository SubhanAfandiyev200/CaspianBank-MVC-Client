namespace CaspianBank_MVC_FinalProject.ViewModels.Account
{
    // Qeydiyyatın 1-ci addımı: vətəndaşı reyestrdə tapmaq üçün FİN, vəsiqə nömrəsi, ad və soyad (hamısı eyni vətəndaşa uyğun olmalıdır)
    public class IdentityVM
    {
        public string Fin { get; set; } = string.Empty;
        public string IdCardNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
    }
}
