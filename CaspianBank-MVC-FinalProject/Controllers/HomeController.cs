using CaspianBank_MVC_FinalProject.ViewModels.HomeTickers;
using Microsoft.AspNetCore.Mvc;

namespace CaspianBank_MVC_FinalProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var tickers = new List<HomeTickerUIVM>();

            try
            {
                var client = _httpClientFactory.CreateClient("CaspianApi");
                tickers = await client.GetFromJsonAsync<List<HomeTickerUIVM>>("api/home/tickers")
                          ?? new List<HomeTickerUIVM>();
            }
            catch (HttpRequestException)
            {
                // API işləmirsə ticker boş qalır, səhifənin qalanı yenə açılsın
            }

            return View(tickers);
        }
    }
}
