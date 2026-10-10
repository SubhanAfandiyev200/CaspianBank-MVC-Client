namespace CaspianBank_MVC_FinalProject.ViewModels.CardDesigns
{
    // Admin panelində siyahı üçün: gizli olanlar da daxil (Home üçün CardDesignUIVM)
    public class CardDesignVM
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public bool ShowOnHome { get; set; }
        public int DisplayOrder { get; set; }
    }
}
