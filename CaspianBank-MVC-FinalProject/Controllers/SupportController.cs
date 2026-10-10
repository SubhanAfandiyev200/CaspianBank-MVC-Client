using CaspianBank_MVC_FinalProject.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CaspianBank_MVC_FinalProject.Controllers
{
    // Müştərinin "Contact us" səhifəsi (dizayn: CaspianBank-Frontend/contact-us.html).
    // Hələlik yalnız statik görünüş: mesaj göndərmə və mesajlar siyahısı sonra yazılacaq (ola bilər onlayn çatla əvəz olunsun).
    // Şirkətin adı, ünvanı və telefonu footer ilə eyni yerdən (api/settings) gəlir
    [Authorize]
    [Route("App/Support")]
    public class SupportController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public SupportController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var settings = await SiteSettingsLoader.LoadAsync(HttpContext, _httpClientFactory, _configuration);
            return View(settings);
        }
    }
}
