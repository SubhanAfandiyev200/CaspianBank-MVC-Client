namespace CaspianBank_MVC_FinalProject.ViewModels.CardHeroes
{
    // Admin panelində hero mətninin Detail səhifəsi üçün (Home üçün CardHeroUIVM)
    public class CardHeroDetailVM
    {
        public int Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
