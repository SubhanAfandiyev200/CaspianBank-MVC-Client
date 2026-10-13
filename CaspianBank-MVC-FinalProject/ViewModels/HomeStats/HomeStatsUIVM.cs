namespace CaspianBank_MVC_FinalProject.ViewModels.HomeStats
{
    // API-dən gələn rəqəmlər (api/home/stats): Home-dakı "Trust indicators" bölməsi
    public class HomeStatsUIVM
    {
        public int CustomerCount { get; set; }
        public int TransferCount { get; set; }
        public decimal TransferVolume { get; set; }
        public int CardCount { get; set; }
    }
}
