using CaspianBank_MVC_FinalProject.Helpers;
using CaspianBank_MVC_FinalProject.ViewModels.Settings;
using Microsoft.AspNetCore.Mvc;

namespace CaspianBank_MVC_FinalProject.ViewComponents
{
    // Saytın hər yerindəki loqo + şirkət adı (giriş/qeydiyyat səhifələri, müştəri və admin paneli, xəta səhifəsi).
    // Loqo və ad Settings cədvəlindəndir, ona görə admin Settings-də dəyişəndə hamısı birlikdə dəyişir.
    // Home-dakı header və footer öz komponentlərində eyni məlumatı istifadə edir.
    public class SiteLogoViewComponent : ViewComponent
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public SiteLogoViewComponent(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<IViewComponentResult> InvokeAsync(string area = "", string controller = "Home", string action = "Index")
        {
            // Eyni sorğuda başqa komponent də Settings istəyibsə API-yə təkrar getmir (HttpContext.Items-də saxlanılır)
            var settings = await SiteSettingsLoader.LoadAsync(HttpContext, _httpClientFactory, _configuration);

            return View(new SiteLogoUIVM
            {
                Logo = settings.Logo,
                CompanyName = settings.CompanyName,
                Area = area,
                Controller = controller,
                Action = action
            });
        }
    }
}
