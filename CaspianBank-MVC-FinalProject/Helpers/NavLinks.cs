namespace CaspianBank_MVC_FinalProject.Helpers
{
    // Home səhifəsindəki bütün düymə və linklərin hara getdiyi burada həll olunur.
    //   Login olmayıb: giriş səhifəsinə (Login) və ya qeydiyyata (Register), dəstək üçün isə footer-ə (#contact)
    //   Login olub   : həmin işin öz səhifəsinə
    // Səhifələr MVC-yə köçürüləndə yalnız aşağıdakı ünvanlar dəyişdirilir.
    public static class NavLinks
    {
        public const string Login = "/Account/Login";

        // Köhnə ayrıca "Welcome" səhifəsi silinib. Bazadakı düymələrdə bu ünvan hələ qala bilər ("hesab aç" mənasında), FromDb onu tanıyır
        public const string LegacyWelcome = "/Account/Welcome";
        public const string Register = "/Account/Register";     // "Open account": birbaşa kimlik formu (FİN, vəsiqə, ad, soyad)
        public const string Contact = "/#contact";

        // TODO: səhifələr yazılanda ünvanları dəyiş (AddCard artıq hazırdır)
        public const string App = "/App";
        public const string AddCard = "/App/AddCard";
        public const string Transfer = "/App/Transfer";
        public const string Loan = "/App/Loans";
        public const string Bills = "/App/Bills";
        public const string Support = "/App/Support";

        // "Mənim hesabım" düyməsi: müştəri üçün kartlar, işçi üçün admin paneli
        public static string AppHome(bool isStaff)
        {
            return isStaff ? RoleRedirect.AdminHome : App;
        }

        public const string Profile = "/App/Profile";

        // Header-dəki şəkil və ad: müştəri üçün profil səhifəsi, işçi üçün admin paneli
        public static string UserLink(bool isStaff)
        {
            return isStaff ? RoleRedirect.AdminHome : Profile;
        }

        // Qeydiyyatdan keçmək / kart açmaq düymələri
        // Login olmayanlar üçün: birbaşa kimlik formu. Login olanın artıq hesabı var, ona kart əlavə etmək təklif olunur
        public static string OpenAccount(bool signedIn, bool isStaff = false)
        {
            if (isStaff)
            {
                return RoleRedirect.AdminHome;
            }
            return signedIn ? AddCard : Register;
        }

        // Giriş tələb edən səhifələr: login olmayıbsa giriş səhifəsi
        public static string Page(bool signedIn, string appUrl, bool isStaff = false)
        {
            if (isStaff)
            {
                return RoleRedirect.AdminHome;      // işçinin müştəri tətbiqi yoxdur: bütün "mənim səhifəm" düymələri admin panelinə aparır
            }
            return signedIn ? appUrl : Login;
        }

        // Dəstək: login olmayıbsa footer-dakı əlaqə hissəsi, olubsa tətbiqin dəstək səhifəsi
        public static string SupportLink(bool signedIn, bool isStaff = false)
        {
            if (isStaff)
            {
                return RoleRedirect.AdminHome + "/Tickets";      // işçi üçün müştəri mesajları
            }
            return signedIn ? Support : Contact;
        }

        // Bazadan gələn ButtonUrl (/Account/Welcome (köhnə ad) və /#contact) login vəziyyətinə görə dəyişir
        public static string FromDb(string? url, bool signedIn, bool isStaff = false)
        {
            if (string.IsNullOrWhiteSpace(url)) return url ?? string.Empty;

            // İşçi üçün müştəri ünvanları (/App..., əlaqə, hesab aç) admin panelinə çevrilir
            if (isStaff && (url.StartsWith("/App", StringComparison.OrdinalIgnoreCase) || url.Equals(Contact, StringComparison.OrdinalIgnoreCase) || url.Equals(LegacyWelcome, StringComparison.OrdinalIgnoreCase) || url.Equals(Register, StringComparison.OrdinalIgnoreCase)))
            {
                return RoleRedirect.AdminHome;
            }

            // Bazadakı "/Account/Welcome" düymələri "hesab aç" mənasındadır: login olmayan birbaşa kimlik formuna düşür
            if (url.Equals(LegacyWelcome, StringComparison.OrdinalIgnoreCase)) return OpenAccount(signedIn);
            if (signedIn && url.Equals(Contact, StringComparison.OrdinalIgnoreCase)) return Support;
            return url;
        }

        // Service kartları (Number bazadakı sıra nömrəsidir: 1 Cards ... 6 24/7 Support)
        public static string ForService(int number, bool signedIn, bool isStaff = false)
        {
            switch (number)
            {
                case 2: return Page(signedIn, Transfer, isStaff);        // Transfers
                case 3: return Page(signedIn, Loan, isStaff);            // Loans
                case 5: return Page(signedIn, Bills, isStaff);           // Bill Payments
                case 6: return SupportLink(signedIn, isStaff);           // 24/7 Support
                default: return Page(signedIn, App, isStaff);            // 1 Cards, 4 Cashback Rewards
            }
        }
    }
}
