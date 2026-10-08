namespace CaspianBank_MVC_FinalProject.ViewModels.ServiceItems
{
    public class ServiceItemVM
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public string Icon { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
