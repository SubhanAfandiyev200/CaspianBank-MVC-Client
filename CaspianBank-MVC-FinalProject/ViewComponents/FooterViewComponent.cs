using CaspianBank_MVC_FinalProject.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace CaspianBank_MVC_FinalProject.ViewComponents
{
    public class FooterViewComponent : ViewComponent
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public FooterViewComponent(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View(await SiteSettingsLoader.LoadAsync(HttpContext, _httpClientFactory, _configuration));
        }
    }
}
