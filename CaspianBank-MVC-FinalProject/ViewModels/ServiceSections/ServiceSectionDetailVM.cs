using CaspianBank_MVC_FinalProject.ViewModels.ServiceItems;

namespace CaspianBank_MVC_FinalProject.ViewModels.ServiceSections
{
    // Admin panelində Detail səhifəsi üçün (başlıq mətni + altında göstərilən xidmət kartları)
    public class ServiceSectionDetailVM
    {
        public int Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // API-dən ayrıca alınır (controller doldurur)
        public List<ServiceItemVM> Items { get; set; } = new List<ServiceItemVM>();
    }
}
