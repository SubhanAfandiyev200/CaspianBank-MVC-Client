namespace CaspianBank_MVC_FinalProject.Helpers
{
    // Home səhifəsindəki bütün düymə və linklərin hara getdiyi burada həll olunur.
    //   Login olmayıb: qeydiyyat/giriş axınına (Welcome), dəstək üçün isə footer-ə (#contact)
    //   Login olub   : həmin işin öz səhifəsinə
    // Səhifələr MVC-yə köçürüləndə yalnız aşağıdakı ünvanlar dəyişdirilir.
    public static class NavLinks
    {
        public const string Welcome = "/Account/Welcome";
        public const string Contact = "/#contact";

        // TODO: səhifələr yazılanda ünvanları dəyiş (AddCard artıq hazırdır)
        public const string App = "/App";
        public const string AddCard = "/App/AddCard";
        public const string Transfer = "/App/Transfer";
        public const string Loan = "/App";
        public const string Bills = "/App";
        public const string Support = "/App";

        // Qeydiyyatdan keçmək / kart açmaq düymələri
        public static string OpenAccount(bool signedIn) => signedIn ? AddCard : Welcome;

        // Giriş tələb edən səhifələr: login olmayıbsa əvvəl Welcome
        public static string Page(bool signedIn, string appUrl) => signedIn ? appUrl : Welcome;

        // Dəstək: login olmayıbsa footer-dakı əlaqə hissəsi, olubsa tətbiqin dəstək səhifəsi
        public static string SupportLink(bool signedIn) => signedIn ? Support : Contact;

        // Bazadan gələn ButtonUrl (/Account/Welcome və /#contact) login vəziyyətinə görə dəyişir
        public static string FromDb(string? url, bool signedIn)
        {
            if (!signedIn || string.IsNullOrWhiteSpace(url)) return url ?? string.Empty;

            if (url.Equals(Welcome, StringComparison.OrdinalIgnoreCase)) return AddCard;
            if (url.Equals(Contact, StringComparison.OrdinalIgnoreCase)) return Support;
            return url;
        }

        // Service kartları (Number bazadakı sıra nömrəsidir: 1 Cards ... 6 24/7 Support)
        public static string ForService(int number, bool signedIn) => number switch
        {
            1 or 4 => Page(signedIn, App),        // Cards, Cashback Rewards
            2 => Page(signedIn, Transfer),        // Transfers
            3 => Page(signedIn, Loan),            // Loans
            5 => Page(signedIn, Bills),           // Bill Payments
            6 => SupportLink(signedIn),           // 24/7 Support
            _ => Page(signedIn, App)
        };
    }
}
