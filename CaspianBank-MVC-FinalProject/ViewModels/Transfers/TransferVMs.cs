using CaspianBank_MVC_FinalProject.ViewModels.Cards;

namespace CaspianBank_MVC_FinalProject.ViewModels.Transfers
{
    // Köçürmə formu (həm ilk açılış, həm xətadan sonra daxil edilənləri saxlayaraq yenidən göstərmək üçün)
    public class TransferFormVM
    {
        public int FromCardId { get; set; }
        public string Mode { get; set; } = "own";               // own: öz kartıma, other: başqa müştəriyə
        public int? ToCardId { get; set; }
        public string? ToCardNumber { get; set; }
        public string? Amount { get; set; }
        public string? Note { get; set; }
        public string RequestId { get; set; } = Guid.NewGuid().ToString();

        public List<CardUIVM> Cards { get; set; } = new();
        public List<CardTierUIVM> Tiers { get; set; } = new();  // canlı komissiya hesabı üçün
        public string? Error { get; set; }
        public bool ApiUnavailable { get; set; }
    }

    // API-dən gələn "yoxla" nəticəsi (pul hələ köçürülməyib)
    public class TransferPreviewUIVM
    {
        public string FromLabel { get; set; } = string.Empty;
        public decimal FromBalance { get; set; }
        public string ToLabel { get; set; } = string.Empty;
        public string? ToHolder { get; set; }
        public bool IsOwn { get; set; }
        public decimal Amount { get; set; }
        public decimal Commission { get; set; }
        public decimal Total { get; set; }
        public string? Note { get; set; }
    }

    // Yoxlama səhifəsi: nəticə + təsdiqlənəndə yenidən göndəriləcək sahələr
    public class TransferReviewVM
    {
        public TransferPreviewUIVM Preview { get; set; } = new();
        public TransferFormVM Form { get; set; } = new();
    }

    public class TransferReceiptUIVM
    {
        public string Reference { get; set; } = string.Empty;
        public string FromLabel { get; set; } = string.Empty;
        public string ToLabel { get; set; } = string.Empty;
        public string? ToHolder { get; set; }
        public decimal Amount { get; set; }
        public decimal Commission { get; set; }
        public decimal Total { get; set; }
        public string? Note { get; set; }
        public decimal FromBalanceAfter { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
