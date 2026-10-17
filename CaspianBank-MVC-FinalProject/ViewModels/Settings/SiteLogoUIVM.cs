namespace CaspianBank_MVC_FinalProject.ViewModels.Settings
{
    // Giriş, qeydiyyat, müştəri və admin panelinin yuxarısındakı loqo + şirkət adı: ikisi də bazadan (Settings) gəlir
    public class SiteLogoUIVM
    {
        public string Logo { get; set; } = string.Empty;           // tam ünvan; boşdursa köhnə SVG işarə göstərilir
        public string CompanyName { get; set; } = "Caspian Bank";

        // Loqoya basanda hara getsin (müştəri tərəfində Home, admin panelində Dashboard)
        public string Area { get; set; } = string.Empty;
        public string Controller { get; set; } = "Home";
        public string Action { get; set; } = "Index";
    }
}
