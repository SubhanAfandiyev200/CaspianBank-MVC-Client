using CaspianBank_MVC_FinalProject.ViewModels.ServiceItems;
using CaspianBank_MVC_FinalProject.ViewModels.ServiceSections;
using CaspianBank_MVC_FinalProject.ViewModels.Services;
using Microsoft.AspNetCore.Mvc;

namespace CaspianBank_MVC_FinalProject.ViewComponents
{
    public class ServiceViewComponent : ViewComponent
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public ServiceViewComponent(IHttpClientFactory httpClientFactory,
                                    IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            ServiceSectionUIVM? section = null;
            var items = new List<ServiceItemUIVM>();
            var client = _httpClientFactory.CreateClient("CaspianApi");

            try
            {
                section = await client.GetFromJsonAsync<ServiceSectionUIVM>("api/home/serviceSections");
                items = await client.GetFromJsonAsync<List<ServiceItemUIVM>>("api/home/serviceItems")
                        ?? new List<ServiceItemUIVM>();
            }
            catch (HttpRequestException)
            {
                // API işləmirsə və ya section sətri yoxdursa (404) bölmə göstərilmir
            }

            if (section is null)
                return Content(string.Empty);

            var baseUrl = (_configuration["ApiSettings:BaseUrl"] ?? string.Empty).TrimEnd('/');
            foreach (var item in items)
            {
                item.Icon = ToAbsoluteUrl(baseUrl, item.Icon);
                item.IconSvg = await LoadIconSvgAsync(client, item.Icon);
            }

            return View(new ServiceVM
            {
                ServiceSection = section,
                ServiceItems = items
            });
        }

        private static string ToAbsoluteUrl(string baseUrl, string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return string.Empty;
            if (path.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return path;
            return baseUrl + "/" + path.TrimStart('/');
        }

        private static string ExtractSvg(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return string.Empty;

            var start = content.IndexOf("<svg", StringComparison.OrdinalIgnoreCase);
            var end = content.LastIndexOf("</svg>", StringComparison.OrdinalIgnoreCase);
            if (start < 0 || end < 0 || end <= start) return string.Empty;

            return content[start..(end + 6)];
        }

        private static async Task<string> LoadIconSvgAsync(HttpClient client, string iconUrl)
        {
            if (string.IsNullOrWhiteSpace(iconUrl)) return string.Empty;

            try
            {
                return ExtractSvg(await client.GetStringAsync(iconUrl));
            }
            catch (HttpRequestException)
            {
                return string.Empty;
            }
        }
    }
}
