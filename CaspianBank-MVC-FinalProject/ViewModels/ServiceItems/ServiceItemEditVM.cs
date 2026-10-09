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

        // Yalnız göstərmək üçündür, formadan oxunmur: nömrə kartın hansı səhifəyə getdiyini, ikon isə görünüşünü müəyyən edir
        [BindNever]
        public int Number { get; set; }

        [BindNever]
        public string Icon { get; set; } = string.Empty;
    }
}
