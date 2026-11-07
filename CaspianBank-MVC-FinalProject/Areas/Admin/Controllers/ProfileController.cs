using CaspianBank_MVC_FinalProject.Controllers;
using CaspianBank_MVC_FinalProject.Helpers;
using CaspianBank_MVC_FinalProject.ViewModels.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CaspianBank_MVC_FinalProject.Areas.Admin.Controllers
{
    // İşçinin öz profili: şəkil, ad, email və rolu (admin başlığındakı ada basanda açılır). Məlumat API-dən gəlir (api/account/profile)
    [Area("Admin")]
    [Authorize(Roles = AppRoles.Staff)]
    public class ProfileController : ApiControllerBase
    {
        public ProfileController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        // Başlıqdakı kiçik şəkil: işçinin şəklinə yönləndirir. Şəkli yoxdursa 404 və baş hərflər qalır
        [HttpGet]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
        public async Task<IActionResult> Avatar()
        {
            var (profile, _) = await GetAsync<ProfileUIVM>("api/account/profile");
            if (profile is null || string.IsNullOrEmpty(profile.PhotoUrl))
            {
                return NotFound();
            }
            return Redirect(ApiUrl.ToAbsolute(Configuration["ApiSettings:BaseUrl"], profile.PhotoUrl) + "?v=" + DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 60);
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var (profile, status) = await GetAsync<ProfileUIVM>("api/account/profile");
            if (status == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }

            if (profile is not null)
            {
                profile.PhotoUrl = ApiUrl.ToAbsolute(Configuration["ApiSettings:BaseUrl"], profile.PhotoUrl);
            }
            return View(new ProfilePageVM { Profile = profile, Unavailable = profile is null });
        }
    }
}
