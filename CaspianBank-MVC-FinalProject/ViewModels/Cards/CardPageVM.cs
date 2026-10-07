namespace CaspianBank_MVC_FinalProject.ViewModels.Cards
{
    // Kart səhifəsi: kartın özü + son əməliyyatları
    public class CardPageVM
    {
        public CardDetailUIVM? Card { get; set; }
        public List<TransactionUIVM> Transactions { get; set; } = new();
        public bool Unavailable { get; set; }
    }

    public class TransactionUIVM
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;        // TopUp, TransferOut, TransferIn, Commission, CardFee
        public bool IsIncome { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Note { get; set; }
        public string Reference { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
