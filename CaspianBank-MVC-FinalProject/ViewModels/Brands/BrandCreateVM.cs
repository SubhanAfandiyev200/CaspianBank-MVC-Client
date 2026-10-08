using System.ComponentModel.DataAnnotations;

namespace CaspianBank_MVC_FinalProject.ViewModels.Brands
{
    public class BrandCreateVM
    {
        [Required(ErrorMessage = "Enter a name.")]
        [StringLength(100, ErrorMessage = "The name can be at most 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Choose an image.")]
        public IFormFile? Image { get; set; }
    }
}
