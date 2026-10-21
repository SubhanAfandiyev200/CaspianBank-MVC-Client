namespace CaspianBank_MVC_FinalProject.ViewModels.Settings
{
    // API-dən gələn açar-dəyər lüğəti (api/settings) header və footer üçün hazır xassələrə çevrilir.
    // Açar bazada yoxdursa (və ya API işləmirsə) əvvəlki statik mətn göstərilir.
    public class SiteSettingsVM
    {
        public string Logo { get; set; } = string.Empty;          // tam ünvan; boşdursa köhnə SVG loqo göstərilir
        public string CompanyName { get; set; } = "Caspian Bank";
        public string Address { get; set; } = "28 Nizami Street, Baku";
        public string Email { get; set; } = "support@caspianbank.az";
        public string PhoneNumber { get; set; } = "+994 12 310 00 00";
        public string FooterTitle { get; set; } = "Ready to open your Caspian account?";
        public string FooterDescription { get; set; } =
            "Cards, transfers, and a loan request from the same login. The desk in Baku stays on the account from the first day.";

        // Sosial şəbəkə linkləri (Settings). Admin dəyişənə qədər bu ünvanlar göstərilir; boş qoyularsa ikon görünmür
        public string InstagramUrl { get; set; } = "https://www.instagram.com/efendiyev.006/";
        public string FacebookUrl { get; set; } = "https://www.facebook.com/code.edu.az/";
        public string TwitterUrl { get; set; } = "https://code.edu.az/";
        public string LinkedInUrl { get; set; } = "https://www.linkedin.com/school/code.edu.az/";

        // tel: ünvanında boşluq olmasın
        public string PhoneHref => new string(PhoneNumber.Where(c => char.IsDigit(c) || c == '+').ToArray());
    }
}
