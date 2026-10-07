namespace CaspianBank_MVC_FinalProject.ViewModels.Cards
{
    // add-card səhifəsi: kartı yoxdursa FirstTime (FİN ilə ilk kartlar), varsa növ seçimi
    public class AddCardPageVM
    {
        public bool FirstTime { get; set; }
        public bool ApiUnavailable { get; set; }
        public List<CardTierUIVM> Tiers { get; set; } = new();
        public List<CardUIVM> Cards { get; set; } = new();
        public string HolderName { get; set; } = string.Empty;
        public string StandardDesign { get; set; } = string.Empty;
        public string CashbackDesign { get; set; } = string.Empty;
    }
}
