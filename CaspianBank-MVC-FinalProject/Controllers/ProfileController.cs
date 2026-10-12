using CaspianBank_MVC_FinalProject.Helpers;
using CaspianBank_MVC_FinalProject.ViewModels.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CaspianBank_MVC_FinalProject.Controllers
{
    // Müştərinin profili: şəkil, ad, soyad, FİN, telefon, email (header-dəki istifadəçi adına basanda açılır)
    [Authorize]
    [Route("App/Profile")]
    public class ProfileController : ApiControllerBase
    {
        public ProfileController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        // Header-dəki kiçik şəkil: müştərinin şəklinə yönləndirir (cookie-də saxlanmır, ona görə köhnə sessiyada da işləyir).
        // Brauzer cavabı 1 saat yadda saxlayır, hər səhifədə API-yə getmir. Şəkli yoxdursa 404: header ikonu qalır
        [HttpGet("Avatar")]
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Client)]
        public async Task<IActionResult> Avatar()
        {
            var (profile, _) = await GetAsync<ProfileUIVM>("api/account/profile");
            if (profile is null || string.IsNullOrEmpty(profile.PhotoUrl))
                return NotFound();

            return Redirect(ApiUrl.ToAbsolute(Configuration["ApiSettings:BaseUrl"], profile.PhotoUrl));
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var (profile, status) = await GetAsync<ProfileUIVM>("api/account/profile");
            if (status == HttpStatusCode.Unauthorized) return await SessionExpiredAsync();

            if (profile is not null)
            {
                // Şəkil API-dən verilir, ona görə tam ünvan lazımdır
                profile.PhotoUrl = ApiUrl.ToAbsolute(Configuration["ApiSettings:BaseUrl"], profile.PhotoUrl);
            }

            return View(new ProfilePageVM
            {
                Profile = profile,
                Unavailable = profile is null
            });
        }
    }
}
