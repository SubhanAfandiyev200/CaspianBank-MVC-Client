using CaspianBank_MVC_FinalProject.ViewModels.AboutPillars;
using CaspianBank_MVC_FinalProject.ViewModels.Abouts;
using CaspianBank_MVC_FinalProject.ViewModels.AboutVideo;
using Microsoft.AspNetCore.Mvc;

namespace CaspianBank_MVC_FinalProject.ViewComponents
{
    public class AboutVideoViewComponent : ViewComponent
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public AboutVideoViewComponent(IHttpClientFactory httpClientFactory,
                                       IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            AboutUIVM? about = null;
            var pillars = new List<AboutPillarUIVM>();

            try
            {
                var client = _httpClientFactory.CreateClient("CaspianApi");
                about = await client.GetFromJsonAsync<AboutUIVM>("api/home/about");
                pillars = await client.GetFromJsonAsync<List<AboutPillarUIVM>>("api/home/pillars")
                          ?? new List<AboutPillarUIVM>();
            }
            catch (HttpRequestException)
            {
                // API işləmirsə və ya About sətri yoxdursa (404) bölmə göstərilmir
            }

            if (about is null)
                return Content(string.Empty);

            // Bazada yalnız yol var (/images/..., /videos/...), fayllar API-dən verilir
            var baseUrl = (_configuration["ApiSettings:BaseUrl"] ?? string.Empty).TrimEnd('/');
            about.VideoPath = ToAbsoluteUrl(baseUrl, about.VideoPath);
            foreach (var pillar in pillars)
                pillar.Image = ToAbsoluteUrl(baseUrl, pillar.Image);

            return View(new AboutVideoVM
            {
                About = about,
                AboutPillars = pillars
            });
        }

        private static string ToAbsoluteUrl(string baseUrl, string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return string.Empty;
            if (path.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return path;
            return baseUrl + "/" + path.TrimStart('/');
        }
    }
}
