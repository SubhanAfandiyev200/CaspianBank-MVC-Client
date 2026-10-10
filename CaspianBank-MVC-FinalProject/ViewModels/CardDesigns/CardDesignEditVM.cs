using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace CaspianBank_MVC_FinalProject.ViewModels.CardDesigns
{
    public class CardDesignEditVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Enter a title.")]
        [StringLength(50, ErrorMessage = "The title can be at most 50 characters.")]
        public string Title { get; set; } = string.Empty;

        [Range(0, 1000, ErrorMessage = "The order must be between 0 and 1000.")]
        public int DisplayOrder { get; set; }

        public bool ShowOnHome { get; set; }

        // İstəyə bağlıdır: seçilməsə köhnə şəkil qalır
        public IFormFile? Image { get; set; }

        // Yalnız göstərmək üçündür, formadan oxunmur (müştəri yol göndərə bilməsin)
        [BindNever]
        public string CurrentImage { get; set; } = string.Empty;
    }
}
