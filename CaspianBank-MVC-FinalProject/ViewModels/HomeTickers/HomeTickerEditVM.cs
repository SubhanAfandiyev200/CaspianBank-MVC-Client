using System.ComponentModel.DataAnnotations;

namespace CaspianBank_MVC_FinalProject.ViewModels.HomeTickers
{
    public class HomeTickerEditVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Enter the text.")]
        [StringLength(100, ErrorMessage = "The text can be at most 100 characters.")]
        public string Text { get; set; } = string.Empty;
    }
}
