using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace CaspianBank_MVC_FinalProject.ViewModels.BenefitItems
{
    public class BenefitItemEditVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Enter the label.")]
        [StringLength(100, ErrorMessage = "The label can be at most 100 characters.")]
        public string Label { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the title.")]
        [StringLength(200, ErrorMessage = "The title can be at most 200 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the description.")]
        [StringLength(300, ErrorMessage = "The description can be at most 300 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the button text.")]
        [StringLength(100, ErrorMessage = "The button text can be at most 100 characters.")]
        public string ButtonText { get; set; } = string.Empty;

        // Düymənin yeri hazır siyahıdan seçilir (link əl ilə yazılmır)
        [Required(ErrorMessage = "Choose where the button goes.")]
        public string ButtonUrl { get; set; } = string.Empty;

        // Yalnız göstərmək üçündür, formadan oxunmur: seçim siyahısı API-dən alınır
        [BindNever]
        public List<ButtonDestinationVM> Destinations { get; set; } = new List<ButtonDestinationVM>();

        [Required(ErrorMessage = "Enter the first line.")]
        [StringLength(200, ErrorMessage = "The first line can be at most 200 characters.")]
        public string Text1 { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the second line.")]
        [StringLength(200, ErrorMessage = "The second line can be at most 200 characters.")]
        public string Text2 { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the third line.")]
        [StringLength(200, ErrorMessage = "The third line can be at most 200 characters.")]
        public string Text3 { get; set; } = string.Empty;
    }
}
