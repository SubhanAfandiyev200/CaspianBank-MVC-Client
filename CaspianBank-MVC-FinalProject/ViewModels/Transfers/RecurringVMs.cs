using CaspianBank_MVC_FinalProject.ViewModels.Cards;

namespace CaspianBank_MVC_FinalProject.ViewModels.Transfers
{
    // API-dən gələn aylıq köçürmə (api/recurring-transfers)
    public class RecurringTransferUIVM
    {
        public int Id { get; set; }
        public string FromLabel { get; set; } = string.Empty;
        public string ToLabel { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int DayOfMonth { get; set; }
        public string? Note { get; set; }
        public bool IsActive { get; set; }
        public DateTime NextRunDate { get; set; }
        public string? LastError { get; set; }
    }

    // Yeni aylıq köçürmə formu (xətadan sonra daxil edilənlər qalır)
    public class RecurringFormVM
    {
        public int FromCardId { get; set; }
        public string Mode { get; set; } = "other";         // own: öz kartıma, other: başqa karta
        public int? ToCardId { get; set; }
        public string? ToCardNumber { get; set; }
        public string? Amount { get; set; }
        public int DayOfMonth { get; set; } = 5;
        public string? Note { get; set; }
    }

    public class RecurringPageVM
    {
        public List<RecurringTransferUIVM> Items { get; set; } = new();
        public List<CardUIVM> Cards { get; set; } = new();
        public RecurringFormVM Form { get; set; } = new();
        public string? Error { get; set; }
        public bool ApiUnavailable { get; set; }
    }
}
