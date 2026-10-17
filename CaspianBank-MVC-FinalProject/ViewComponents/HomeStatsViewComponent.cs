using CaspianBank_MVC_FinalProject.ViewModels.HomeStats;
using Microsoft.AspNetCore.Mvc;

namespace CaspianBank_MVC_FinalProject.ViewComponents
{
    public class HomeStatsViewComponent : ViewComponent
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeStatsViewComponent(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("CaspianApi");
                var stats = await client.GetFromJsonAsync<HomeStatsUIVM>("api/home/stats");
                if (stats is not null)
                {
                    return View(stats);
                }
            }
            catch (HttpRequestException)
            {
                // API işləmirsə rəqəmləri uydurmuruq: bölmə gizlənir, səhifənin qalanı yenə açılır
            }

            return Content(string.Empty);
        }
    }
}
