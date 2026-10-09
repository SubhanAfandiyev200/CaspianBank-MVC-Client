using System.ComponentModel.DataAnnotations;

namespace CaspianBank_MVC_FinalProject.ViewModels.BenefitSections
{
    public class BenefitSectionCreateVM
    {
        [Required(ErrorMessage = "Enter the label.")]
        [StringLength(100, ErrorMessage = "The label can be at most 100 characters.")]
        public string Label { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the title.")]
        [StringLength(200, ErrorMessage = "The title can be at most 200 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the description.")]
        [StringLength(500, ErrorMessage = "The description can be at most 500 characters.")]
        public string Description { get; set; } = string.Empty;
    }
}
