namespace CaspianBank_MVC_FinalProject.ViewModels.CardDesigns
{
    // Admin panelində Detail səhifəsi üçün (siyahı üçün CardDesignVM)
    public class CardDesignDetailVM
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public bool ShowOnHome { get; set; }
        public int DisplayOrder { get; set; }
    }
}
