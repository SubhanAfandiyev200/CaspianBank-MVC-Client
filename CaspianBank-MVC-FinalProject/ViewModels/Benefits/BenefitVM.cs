using CaspianBank_MVC_FinalProject.ViewModels.BenefitItems;
using CaspianBank_MVC_FinalProject.ViewModels.BenefitSections;

namespace CaspianBank_MVC_FinalProject.ViewModels.Benefits
{
    public class BenefitVM
    {
        // ViewComponent üçün
        public BenefitSectionUIVM BenefitSection { get; set; } = new BenefitSectionUIVM();
        public IEnumerable<BenefitItemUIVM> BenefitItems { get; set; } = new List<BenefitItemUIVM>();
    }
}
