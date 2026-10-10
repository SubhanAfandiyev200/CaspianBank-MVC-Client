namespace CaspianBank_MVC_FinalProject.ViewModels.CardTiers
{
    // Admin panelində siyahı üçün (ictimai add-card səhifəsi üçün CardTierUIVM)
    public class CardTierVM
    {
        public int Id { get; set; }
        public string Tier { get; set; } = string.Empty;
        public decimal IssueFee { get; set; }
        public decimal CashbackPercent { get; set; }
        public decimal TransferLimit { get; set; }
        public decimal CommissionPercent { get; set; }
        public int CardDesignId { get; set; }
        public string DesignTitle { get; set; } = string.Empty;
        public string DesignImage { get; set; } = string.Empty;
    }
}
