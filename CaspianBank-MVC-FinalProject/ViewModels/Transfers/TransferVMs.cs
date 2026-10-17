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
        public List<CardLimitUIVM> Limits { get; set; } = new(); // bu ay hər kartdan artıq göndərilən məbləğ
        public string? Error { get; set; }
        public bool ApiUnavailable { get; set; }
    }

    // Kartın aylıq komissiyasız limiti və bu ay göndərilən məbləğ
    public class CardLimitUIVM
    {
        public int CardId { get; set; }
        public string Tier { get; set; } = string.Empty;
        public decimal MonthlyLimit { get; set; }
        public decimal CommissionPercent { get; set; }
        public decimal UsedThisMonth { get; set; }
    }

    // Kart nömrəsinin ilk 8 rəqəminə görə tapılan bank (formada nömrənin üstündə göstərilir)
    public class BankLookupUIVM
    {
        public string Name { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public bool IsCaspian { get; set; }
    }

    // API-dən gələn "yoxla" nəticəsi (pul hələ köçürülməyib)
    public class TransferPreviewUIVM
    {
        public string FromLabel { get; set; } = string.Empty;
        public decimal FromBalance { get; set; }
        public string ToLabel { get; set; } = string.Empty;
        public string? ToHolder { get; set; }
        public bool IsOwn { get; set; }
        public string? ToBankName { get; set; }
        public bool IsExternal { get; set; }
        public decimal MonthlyLimit { get; set; }
        public decimal UsedThisMonth { get; set; }
        public decimal ChargedAmount { get; set; }   // komissiya tutulan hissə (limitdən artıq olan)
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
        public string? ToBankName { get; set; }
        public bool IsExternal { get; set; }
        public decimal Amount { get; set; }
        public decimal Commission { get; set; }
        public decimal Total { get; set; }
        public string? Note { get; set; }
        public decimal FromBalanceAfter { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
