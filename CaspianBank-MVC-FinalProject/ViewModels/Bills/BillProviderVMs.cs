using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace CaspianBank_MVC_FinalProject.ViewModels.Bills
{
    // Admin siyahısı (api/admin/bill-providers): söndürülmüşlər də görünür
    public class BillProviderVM
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
    }

    // Adı, aktivliyi və loqonu dəyişir; kod, kateqoriya və qaydalar dəyişmir
    public class BillProviderEditVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Enter a name.")]
        [StringLength(50, ErrorMessage = "The name can be at most 50 characters.")]
        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        // İstəyə bağlıdır: seçilməsə köhnə loqo qalır
        public IFormFile? Logo { get; set; }

        // Yalnız göstərmək üçündür, formadan oxunmur
        [BindNever]
        public string Code { get; set; } = string.Empty;

        [BindNever]
        public string Category { get; set; } = string.Empty;

        [BindNever]
        public string? CurrentLogo { get; set; }
    }
}
