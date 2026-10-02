using CaspianBank_MVC_FinalProject.ViewModels.Brands;
using Microsoft.AspNetCore.Mvc;

namespace CaspianBank_MVC_FinalProject.ViewComponents
{
    public class BrandViewComponent : ViewComponent
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public BrandViewComponent(IHttpClientFactory httpClientFactory,
                                  IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var brands = new List<BrandUIVM>();

            try
            {
                var client = _httpClientFactory.CreateClient("CaspianApi");
                brands = await client.GetFromJsonAsync<List<BrandUIVM>>("api/home/brands")
                         ?? new List<BrandUIVM>();
            }
            catch (HttpRequestException)
            {
                // API işləmirsə brend zolağı boş qalır, səhifənin qalanı yenə açılsın
            }

            // Bazada yalnız yol var (/images/claude.png), şəkil API-dən yüklənir
            var baseUrl = (_configuration["ApiSettings:BaseUrl"] ?? string.Empty).TrimEnd('/');
            foreach (var brand in brands)
            {
                if (!brand.Image.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    brand.Image = baseUrl + "/" + brand.Image.TrimStart('/');
            }

            return View(brands);
        }
    }
}
