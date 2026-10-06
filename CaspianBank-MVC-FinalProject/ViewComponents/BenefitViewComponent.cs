using CaspianBank_MVC_FinalProject.ViewModels.BenefitItems;
using CaspianBank_MVC_FinalProject.ViewModels.BenefitSections;
using CaspianBank_MVC_FinalProject.ViewModels.Benefits;
using Microsoft.AspNetCore.Mvc;

namespace CaspianBank_MVC_FinalProject.ViewComponents
{
    public class BenefitViewComponent : ViewComponent
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public BenefitViewComponent(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            BenefitSectionUIVM? section = null;
            var items = new List<BenefitItemUIVM>();
            var client = _httpClientFactory.CreateClient("CaspianApi");

            try
            {
                section = await client.GetFromJsonAsync<BenefitSectionUIVM>("api/home/benefitSections");
                items = await client.GetFromJsonAsync<List<BenefitItemUIVM>>("api/home/benefitItems")
                        ?? new List<BenefitItemUIVM>();
            }
            catch (HttpRequestException)
            {
                // API işləmirsə və ya section sətri yoxdursa (404) bölmə göstərilmir
            }

            if (section is null)
                return Content(string.Empty);

            return View(new BenefitVM
            {
                BenefitSection = section,
                BenefitItems = items
            });
        }
    }
}
