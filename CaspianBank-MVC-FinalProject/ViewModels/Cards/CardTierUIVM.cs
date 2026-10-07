namespace CaspianBank_MVC_FinalProject.ViewModels.Cards
{
    public class CardTierUIVM
    {
        public string Tier { get; set; } = string.Empty;
        public decimal IssueFee { get; set; }
        public decimal CashbackPercent { get; set; }
        public decimal TransferLimit { get; set; }
        public decimal CommissionPercent { get; set; }
        public string DesignImage { get; set; } = string.Empty;
    }
}
