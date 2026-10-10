namespace CaspianBank_MVC_FinalProject.ViewModels.Settings
{
    // Admin panelində siyahı üçün (header və footer üçün SiteSettingsVM)
    public class SettingVM
    {
        public int Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
