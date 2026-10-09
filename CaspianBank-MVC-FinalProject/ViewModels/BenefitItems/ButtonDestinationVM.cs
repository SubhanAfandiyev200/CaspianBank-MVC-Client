namespace CaspianBank_MVC_FinalProject.ViewModels.BenefitItems
{
    // "Düymə hara aparsın" siyahısının bir sətri (API-dən gəlir: api/admin/benefit-items/destinations)
    public class ButtonDestinationVM
    {
        public string Path { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }
}
