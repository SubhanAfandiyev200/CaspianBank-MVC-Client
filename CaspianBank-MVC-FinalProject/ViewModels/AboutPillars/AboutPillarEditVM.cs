using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace CaspianBank_MVC_FinalProject.ViewModels.AboutPillars
{
    public class AboutPillarEditVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Enter the title.")]
        [StringLength(100, ErrorMessage = "The title can be at most 100 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the description.")]
        [StringLength(300, ErrorMessage = "The description can be at most 300 characters.")]
        public string Description { get; set; } = string.Empty;

        // İstəyə bağlıdır: seçilməsə köhnə şəkil qalır
        public IFormFile? Image { get; set; }

        // Yalnız göstərmək üçündür, formadan oxunmur (müştəri yol göndərə bilməsin)
        [BindNever]
        public string CurrentImage { get; set; } = string.Empty;
    }
}
