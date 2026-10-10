using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace CaspianBank_MVC_FinalProject.ViewModels.Settings
{
    // Mətn ayarları Value ilə, loqo isə Image ilə dəyişir. Hansı olduğunu açar (Key) müəyyən edir
    public class SettingEditVM
    {
        public int Id { get; set; }

        // Boşluq yoxlaması və uzunluq controller/API-dədir: loqo üçün bu sahə istifadə olunmur
        [StringLength(500, ErrorMessage = "The value can be at most 500 characters.")]
        public string? Value { get; set; }

        // Yalnız loqo üçün
        public IFormFile? Image { get; set; }

        // Yalnız göstərmək üçündür, formadan oxunmur: açarı müştəri dəyişə bilməz
        [BindNever]
        public string Key { get; set; } = string.Empty;

        // Loqonun hazırkı yolu (API-dən alınır)
        [BindNever]
        public string CurrentImage { get; set; } = string.Empty;
    }
}
