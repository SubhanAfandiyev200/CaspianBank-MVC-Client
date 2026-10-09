using System.ComponentModel.DataAnnotations;

namespace CaspianBank_MVC_FinalProject.ViewModels.CardDesigns
{
    public class CardDesignCreateVM
    {
        [Required(ErrorMessage = "Enter a title.")]
        [StringLength(50, ErrorMessage = "The title can be at most 50 characters.")]
        public string Title { get; set; } = string.Empty;

        [Range(0, 1000, ErrorMessage = "The order must be between 0 and 1000.")]
        public int DisplayOrder { get; set; }

        public bool ShowOnHome { get; set; } = true;

        [Required(ErrorMessage = "Choose a card image.")]
        public IFormFile? Image { get; set; }
    }
}
