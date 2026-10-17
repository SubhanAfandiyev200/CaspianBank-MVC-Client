using CaspianBank_MVC_FinalProject.ViewModels.Cards;

namespace CaspianBank_MVC_FinalProject.ViewModels.History
{
    // Tarixçə səhifəsindəki filtr formu (GET: dəyərlər URL-də qalır, səhifəni paylaşmaq/yeniləmək olur)
    public class HistoryFilterVM
    {
        public int? CardId { get; set; }
        public string? Type { get; set; }           // TopUp, TransferOut, TransferIn, Commission, CardFee
        public string? Direction { get; set; }      // in / out
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string? Search { get; set; }
        public int Page { get; set; } = 1;
    }

    // API-nin api/history cavabı (ad və yazılış API-dəki HistoryPageDto ilə eynidir)
    public class HistoryPageUIVM
    {
        public List<TransactionUIVM> Items { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public decimal TotalIn { get; set; }
        public decimal TotalOut { get; set; }
    }

    // Tarixçə səhifəsinin hamısı: filtr + nəticə + kartlar (kart seçimi və çıxarış üçün)
    public class HistoryIndexVM
    {
        public HistoryFilterVM Filter { get; set; } = new();
        public HistoryPageUIVM Result { get; set; } = new();
        public List<CardUIVM> Cards { get; set; } = new();
        public bool ApiUnavailable { get; set; }
        public string? Error { get; set; }
    }

    // API-nin api/history/statement cavabı + çapda göstərilən bank məlumatı
    public class StatementUIVM
    {
        public int CardId { get; set; }
        public string CardLabel { get; set; } = string.Empty;
        public string HolderName { get; set; } = string.Empty;
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }
        public decimal TotalIn { get; set; }
        public decimal TotalOut { get; set; }
        public List<TransactionUIVM> Rows { get; set; } = new();

        // API-dən gəlmir: controller Settings-dən doldurur
        public string CompanyName { get; set; } = "Caspian Bank";
        public string Address { get; set; } = string.Empty;
    }
}
