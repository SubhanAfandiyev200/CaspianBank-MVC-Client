using CaspianBank_MVC_FinalProject.ViewModels.AboutPillars;
using CaspianBank_MVC_FinalProject.ViewModels.Abouts;

namespace CaspianBank_MVC_FinalProject.ViewModels.AboutVideo
{
    public class AboutVideoVM
    {
        //viewComponente goredi bura
        public IEnumerable<AboutPillarUIVM> AboutPillars { get; set; } = new List<AboutPillarUIVM>();
        public AboutUIVM About { get; set; } = new AboutUIVM();
    }
}
