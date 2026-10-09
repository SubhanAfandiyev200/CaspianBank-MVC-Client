using CaspianBank_MVC_FinalProject.ViewModels.BenefitItems;

namespace CaspianBank_MVC_FinalProject.ViewModels.BenefitSections
{
    // Admin panelində Detail səhifəsi üçün: bölmənin özü və ona aid kartlar (siyahı üçün BenefitSectionVM)
    public class BenefitSectionDetailVM
    {
        public int Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // API cavabında gəlmir, controller bu bölməyə aid kartlarla doldurur
        public List<BenefitItemVM> Items { get; set; } = new List<BenefitItemVM>();
    }
}
