namespace CaspianBank_MVC_FinalProject.ViewModels.Translations
{
    // api/admin/translations: bir açarın (ingilis mətnin) az və ru tərcüməsi
    public class TranslationRowVM
    {
        public string Key { get; set; } = string.Empty;
        public string Az { get; set; } = string.Empty;
        public string Ru { get; set; } = string.Empty;
    }

    public class TranslationsPageVM
    {
        public List<TranslationRowVM> Items { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }

        public string? Search { get; set; }
        public bool OnlyMissing { get; set; }
        public bool Unavailable { get; set; }
        public string? Error { get; set; }
    }
}
