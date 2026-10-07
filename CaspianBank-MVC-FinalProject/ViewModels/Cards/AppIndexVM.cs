namespace CaspianBank_MVC_FinalProject.ViewModels.Cards
{
    public class AppIndexVM
    {
        public List<CardUIVM> Cards { get; set; } = new();
        public bool ApiUnavailable { get; set; }
    }
}
