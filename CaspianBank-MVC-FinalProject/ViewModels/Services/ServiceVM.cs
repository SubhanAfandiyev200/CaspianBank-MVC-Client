using CaspianBank_MVC_FinalProject.ViewModels.ServiceItems;
using CaspianBank_MVC_FinalProject.ViewModels.ServiceSections;

namespace CaspianBank_MVC_FinalProject.ViewModels.Services
{
    public class ServiceVM
    {
        // ViewComponent üçün
        public ServiceSectionUIVM ServiceSection { get; set; } = new ServiceSectionUIVM();
        public IEnumerable<ServiceItemUIVM> ServiceItems { get; set; } = new List<ServiceItemUIVM>();
    }
}
