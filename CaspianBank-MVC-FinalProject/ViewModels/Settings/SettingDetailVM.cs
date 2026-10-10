namespace CaspianBank_MVC_FinalProject.ViewModels.Settings
{
    // Admin panelində Detail səhifəsi üçün (siyahı üçün SettingVM)
    public class SettingDetailVM
    {
        public int Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
