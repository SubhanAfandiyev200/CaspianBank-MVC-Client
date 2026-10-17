using CaspianBank_MVC_FinalProject.ViewModels.Settings;

namespace CaspianBank_MVC_FinalProject.ViewModels.Support
{
    // api/support/tickets və api/admin/tickets: siyahı sətri (həm müştəri, həm işçi)
    public class TicketVM
    {
        public int Id { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;          // Pending, Answered, Closed
        public DateTime CreatedAt { get; set; }
        public DateTime LastActivityAt { get; set; }
        public int MessageCount { get; set; }
        public string LastMessagePreview { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
    }

    public class TicketPageVM
    {
        public List<TicketVM> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int PendingCount { get; set; }

        // Süzgəc (səhifə yenilənəndə formada qalır)
        public string? Status { get; set; }
        public string? Search { get; set; }
        public bool Unavailable { get; set; }

        public int TotalPages
        {
            get
            {
                return PageSize <= 0 ? 1 : Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
            }
        }
    }

    public class TicketMessageVM
    {
        public int Id { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public bool IsStaff { get; set; }
        public string Body { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class TicketDetailVM
    {
        public int Id { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public List<TicketMessageVM> Messages { get; set; } = new();

        // Cavab formasında xəta olanda yazılan mətn saxlanır (API qaytarmır, controller doldurur)
        public string? Error { get; set; }
        public string? Draft { get; set; }
    }

    // "Write to us" formasının daxil edilənləri
    public class TicketFormVM
    {
        public string? Subject { get; set; }
        public string? Message { get; set; }
    }

    // /App/Support səhifəsi: footer ilə eyni ayarlar (ünvan, telefon), müştərinin mesajları və yeni mesaj forması
    public class SupportPageVM
    {
        public SiteSettingsVM Settings { get; set; } = new();
        public List<TicketVM> Tickets { get; set; } = new();
        public TicketFormVM Form { get; set; } = new();
        public string? Error { get; set; }
    }
}
