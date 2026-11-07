using CaspianBank_MVC_FinalProject.Helpers;
using CaspianBank_MVC_FinalProject.ViewModels.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CaspianBank_MVC_FinalProject.Controllers
{
    // Müştərinin profili: şəkil, ad, soyad, FİN, telefon, email (header-dəki istifadəçi adına basanda açılır)
    [Authorize(Roles = AppRoles.Customer)]
    [Route("App/Profile")]
    public class ProfileController : ApiControllerBase
    {
        public ProfileController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        // Header-dəki kiçik şəkil: müştərinin şəklinə yönləndirir (cookie-də saxlanmır, ona görə köhnə sessiyada da işləyir).
        // Brauzer cavabı 1 dəqiqə yadda saxlayır, hər səhifədə API-yə getmir. Şəkil faylı dəyişəndə köhnəsi qalmasın deyə ünvana dəqiqə nömrəsi əlavə olunur. Şəkli yoxdursa 404: header ikonu qalır
        [HttpGet("Avatar")]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
        public async Task<IActionResult> Avatar()
        {
            var (profile, _) = await GetAsync<ProfileUIVM>("api/account/profile");
            if (profile is null || string.IsNullOrEmpty(profile.PhotoUrl))
                return NotFound();

            return Redirect(ApiUrl.ToAbsolute(Configuration["ApiSettings:BaseUrl"], profile.PhotoUrl) + "?v=" + DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 60);
        }

        // Balans xəbərdarlığı həddi: kartın qalığı bu məbləğdən aşağı düşəndə bildiriş gedir (0 = söndürülür). Qaydalar API-dədir
        [HttpPost("AlertSettings")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlertSettings(string? threshold)
        {
            var text = (threshold ?? string.Empty).Trim().Replace(',', '.');
            if (!decimal.TryParse(text, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var value))
            {
                return await ShowAsync(threshold, "Enter the amount as a number, for example 25 or 25.50. Use 0 to turn the alert off.");
            }

            var (success, errors, status) = await PutAsync("api/account/alert-settings", new { lowBalanceThreshold = value });
            if (status == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }
            if (!success)
            {
                return await ShowAsync(threshold, errors.Length > 0 ? string.Join(" ", TranslateErrors(errors)) : UnavailableMessage);
            }

            TempData["Success"] = value <= 0m ? "The low balance alert is turned off." : "We will notify you when a card balance falls below the amount you set.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<IActionResult> ShowAsync(string? draft, string error)
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
            return View("Index", new ProfilePageVM { Profile = profile, Unavailable = profile is null, Error = error, ThresholdDraft = draft });
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
