using CaspianBank_MVC_FinalProject.ViewModels.AboutPillars;

namespace CaspianBank_MVC_FinalProject.ViewModels.Abouts
{
    // Admin panelində Detail səhifəsi üçün (mətn + video yolu + altındakı sütunlar)
    public class AboutDetailVM
    {
        public int Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string VideoPath { get; set; } = string.Empty;

        // API-dən ayrıca alınır (controller doldurur)
        public List<AboutPillarVM> Pillars { get; set; } = new List<AboutPillarVM>();
    }
}
