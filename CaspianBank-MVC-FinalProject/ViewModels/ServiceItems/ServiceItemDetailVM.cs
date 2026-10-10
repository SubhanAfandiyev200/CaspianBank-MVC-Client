namespace CaspianBank_MVC_FinalProject.ViewModels.ServiceItems
{
    // Admin panelində Detail səhifəsi üçün (siyahı üçün ServiceItemVM)
    public class ServiceItemDetailVM
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public string Icon { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
