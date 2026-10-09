using System.ComponentModel.DataAnnotations;

namespace CaspianBank_MVC_FinalProject.ViewModels.AboutPillars
{
    public class AboutPillarCreateVM
    {
        [Required(ErrorMessage = "Enter the title.")]
        [StringLength(100, ErrorMessage = "The title can be at most 100 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the description.")]
        [StringLength(300, ErrorMessage = "The description can be at most 300 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Choose an image.")]
        public IFormFile? Image { get; set; }
    }
}
