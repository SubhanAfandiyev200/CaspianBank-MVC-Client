namespace CaspianBank_MVC_FinalProject.ViewModels.Common
{
    // Nömrəli səhifələmə (1 2 3 ... 9) üçün ortaq model: _Pager.cshtml bunu göstərir
    public class PagerVM
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }

        // Verilən səhifə nömrəsi üçün ünvan (süzgəclər qorunur)
        public Func<int, string> Url { get; set; } = _ => "#";
    }
}
