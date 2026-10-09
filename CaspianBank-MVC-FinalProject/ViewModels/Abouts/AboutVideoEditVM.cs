using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CaspianBank_MVC_FinalProject.ViewModels.Abouts
{
    public class AboutVideoEditVM
    {
        public int Id { get; set; }

        // Boşluq və ölçü yoxlaması controller/API-dədir
        public IFormFile? Video { get; set; }

        // Yalnız göstərmək üçündür, formadan oxunmur: hazırkı videonun yolu API-dən alınır
        [BindNever]
        public string CurrentVideo { get; set; } = string.Empty;
    }
}
