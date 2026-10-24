using CaspianBank_MVC_FinalProject.ViewModels.Notifications;
using System.Globalization;

namespace CaspianBank_MVC_FinalProject.Helpers
{
    // Bildiriş sətirlərinin ikonu, nisbi vaxtı və açılış ünvanı (zəng və səhifə eyni qaydanı işlədir; wwwroot/js/notifications.js-də eyni məntiq var)
    public static class NotificationUi
    {
        // Növə görə SVG daxili yolları (24x24 görünüş qutusu)
        public static string IconPaths(string category)
        {
            switch (category)
            {
                case "Loan":
                    return "<path d=\"M4 10l8-6 8 6\"/><path d=\"M6 10v8h12v-8\"/><path d=\"M10 18v-4h4v4\"/>";
                case "Transfer":
                    return "<path d=\"M7 17 17 7\"/><path d=\"M9 7h8v8\"/>";
                case "Card":
                    return "<rect x=\"3\" y=\"6\" width=\"18\" height=\"13\" rx=\"2.5\"/><path d=\"M3 10.5h18M7 15h3\"/>";
                case "Staff":
                    return "<path d=\"M4 6h16v10H7l-3 3z\"/>";
                case "Support":
                    return "<path d=\"M4 6h16v10H7l-3 3z\"/>";
                case "Bill":
                    return "<path d=\"M13 3 5 14h6l-1 7 8-11h-6z\"/>";
                case "Deposit":
                    return "<path d=\"M4 9h16v10H4zM8 9V6h8v3M12 12v4M10 14h4\"/>";
                default:
                    return "<path d=\"M6 17h12l-1.5-2V11a4.5 4.5 0 0 0-9 0v4z\"/><path d=\"M10 20a2 2 0 0 0 4 0\"/>";
            }
        }

        // "just now", "5 min ago", "3 h ago", sonra tarix. Bazada vaxt UTC saxlanılır
        public static string Ago(DateTime createdAtUtc, CaspianBank_MVC_FinalProject.Services.ITranslator? t = null)
        {
            var utc = createdAtUtc.Kind == DateTimeKind.Utc ? createdAtUtc : DateTime.SpecifyKind(createdAtUtc, DateTimeKind.Utc);
            var span = DateTime.UtcNow - utc;
            if (span.TotalSeconds < 45)
            {
                return t is null ? "just now" : t["just now"];
            }
            if (span.TotalMinutes < 60)
            {
                var minutes = Math.Max(1, (int)Math.Round(span.TotalMinutes));
                return t is null ? minutes + " min ago" : t["{0} min ago", minutes];
            }
            if (span.TotalHours < 24)
            {
                var hours = (int)span.TotalHours;
                return t is null ? hours + " h ago" : t["{0} h ago", hours];
            }
            return utc.ToLocalTime().ToString("dd MMM, HH:mm", CultureInfo.InvariantCulture);
        }

        // Bildirişə basanda: əvvəl oxunmuş sayılır, sonra bildirişin yoluna keçilir (yalnız saytın öz yolları)
        public static string OpenUrl(NotificationUIVM item)
        {
            var url = "/App/Notifications/Open/" + item.Id;
            if (!string.IsNullOrEmpty(item.LinkUrl) && item.LinkUrl.StartsWith("/", StringComparison.Ordinal) && !item.LinkUrl.StartsWith("//", StringComparison.Ordinal))
            {
                url += "?to=" + Uri.EscapeDataString(item.LinkUrl);
            }
            return url;
        }

        public static string CountText(int unread)
        {
            return unread > 99 ? "99+" : unread.ToString(CultureInfo.InvariantCulture);
        }
    }
}
