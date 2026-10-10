namespace CaspianBank_MVC_FinalProject.ViewModels.CardDesigns
{
    // Admin panelində kartın necə görünəcəyini göstərən önizləmə üçün (_CardPreview)
    public class CardDesignPreviewVM
    {
        public string Title { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;

        // true: böyük kart (tam nömrə və sahibin adı ilə), false: kiçik kart
        public bool Large { get; set; }
    }
}
