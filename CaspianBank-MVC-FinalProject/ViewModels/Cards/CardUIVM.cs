namespace CaspianBank_MVC_FinalProject.ViewModels.Cards
{
    // API-dən gələn kart (siyahı üçün: tam nömrə yoxdur)
    public class CardUIVM
    {
        public int Id { get; set; }
        public string Tier { get; set; } = string.Empty;        // Cashback / Standard / Silver / Gold
        public string Last4 { get; set; } = string.Empty;
        public string Expiry { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public bool IsBlocked { get; set; }
        public string HolderName { get; set; } = string.Empty;
        public string DesignImage { get; set; } = string.Empty; // controller tam ünvana çevirir
    }
}
