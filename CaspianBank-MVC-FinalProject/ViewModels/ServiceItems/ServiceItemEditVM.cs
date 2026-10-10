using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace CaspianBank_MVC_FinalProject.ViewModels.ServiceItems
{
    public class ServiceItemEditVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Enter the title.")]
        [StringLength(100, ErrorMessage = "The title can be at most 100 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the description.")]
        [StringLength(300, ErrorMessage = "The description can be at most 300 characters.")]
        public string Description { get; set; } = string.Empty;

        // İstəyə bağlıdır: seçilməsə köhnə ikon qalır
        public IFormFile? Icon { get; set; }

        // Yalnız göstərmək üçündür, formadan oxunmur: nömrə kartın hansı səhifəyə getdiyini müəyyən edir, ikonun yolu isə API-dən alınır
        [BindNever]
        public int Number { get; set; }

        [BindNever]
        public string CurrentIcon { get; set; } = string.Empty;
    }
}
