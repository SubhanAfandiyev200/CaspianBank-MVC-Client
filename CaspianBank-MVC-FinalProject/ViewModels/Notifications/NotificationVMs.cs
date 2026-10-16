namespace CaspianBank_MVC_FinalProject.ViewModels.Notifications
{
    // API-dən gələn bir bildiriş (api/notifications)
    public class NotificationUIVM
    {
        public int Id { get; set; }
        public string Category { get; set; } = string.Empty;     // Loan, Transfer, Card, Staff
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? LinkUrl { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class NotificationPageUIVM
    {
        public List<NotificationUIVM> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int UnreadCount { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        // Səhifənin öz vəziyyəti (API qaytarmır)
        public bool UnreadOnly { get; set; }
        public string Area { get; set; } = string.Empty;         // "" müştəri, "Admin" işçi (linklər üçün)
        public bool Unavailable { get; set; }

        public int TotalPages
        {
            get
            {
                return PageSize <= 0 ? 1 : Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
            }
        }
    }

    // Header-dəki zəng: oxunmamışların sayı və son bildirişlər
    public class NotificationBellVM
    {
        public string Area { get; set; } = string.Empty;
        public int UnreadCount { get; set; }
        public List<NotificationUIVM> Items { get; set; } = new();
    }

    // Canlı kanala qoşulmaq üçün qısa ömürlü bilet (api/notifications/hub-token)
    public class HubTicketUIVM
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
