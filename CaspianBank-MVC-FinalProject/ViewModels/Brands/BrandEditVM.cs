using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace CaspianBank_MVC_FinalProject.ViewModels.Brands
{
    public class BrandEditVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Enter a name.")]
        [StringLength(100, ErrorMessage = "The name can be at most 100 characters.")]
        public string Name { get; set; } = string.Empty;

        // İstəyə bağlıdır: seçilməsə köhnə logo qalır
        public IFormFile? Image { get; set; }

        // Yalnız göstərmək üçündür, formadan oxunmur (müştəri yol göndərə bilməsin)
        [BindNever]
        public string CurrentImage { get; set; } = string.Empty;
    }
}
