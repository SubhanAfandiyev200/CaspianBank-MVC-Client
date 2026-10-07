using CaspianBank_MVC_FinalProject.Helpers;
using CaspianBank_MVC_FinalProject.ViewModels.Cards;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace CaspianBank_MVC_FinalProject.Controllers
{
    // API-yə müraciət edən giriş tələb edən səhifələrin ortaq köməkçiləri
    public abstract class ApiControllerBase : Controller
    {
        protected const string UnavailableMessage = "The service is unavailable right now. Please try again later.";

        private readonly IHttpClientFactory _httpClientFactory;
        protected readonly IConfiguration Configuration;

        protected ApiControllerBase(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            Configuration = configuration;
        }

        protected HttpClient Api => _httpClientFactory.CreateClient("CaspianApi");

        // Bazada yalnız yol var (/images/cards/x.png): fayllar API-dən verildiyi üçün tam ünvan lazımdır
        protected List<CardUIVM> Absolutize(List<CardUIVM> cards)
        {
            var baseUrl = Configuration["ApiSettings:BaseUrl"];
            foreach (var card in cards)
                card.DesignImage = ApiUrl.ToAbsolute(baseUrl, card.DesignImage);
            return cards;
        }

        // GET: nəticə yoxdursa (API işləmir və ya xəta) Data = null qayıdır
        protected async Task<(T? Data, HttpStatusCode? Status)> GetAsync<T>(string url) where T : class
        {
            try
            {
                var response = await Api.GetAsync(url);
                if (!response.IsSuccessStatusCode) return (null, response.StatusCode);
                return (await response.Content.ReadFromJsonAsync<T>(), response.StatusCode);
            }
            catch (HttpRequestException)
            {
                return (null, null);
            }
            catch (JsonException)
            {
                return (null, null);
            }
        }

        // POST: uğurlu olarsa cavabın gövdəsi Data-da qayıdır, olmazsa istifadəçiyə göstəriləcək xətalar
        protected async Task<(T? Data, string[] Errors, HttpStatusCode? Status)> PostAsync<T>(string url, object body) where T : class
        {
            try
            {
                var response = await Api.PostAsJsonAsync(url, body);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<T>();
                    return (data, Array.Empty<string>(), response.StatusCode);
                }

                return (null, await ReadErrorsAsync(response), response.StatusCode);
            }
            catch (HttpRequestException)
            {
                return (null, new[] { UnavailableMessage }, null);
            }
            catch (JsonException)
            {
                return (null, new[] { "Something went wrong. Please try again." }, null);
            }
        }

        protected async Task<(bool Success, string[] Errors, HttpStatusCode? Status)> PostAsync(string url, object body)
        {
            try
            {
                var response = await Api.PostAsJsonAsync(url, body);
                if (response.IsSuccessStatusCode) return (true, Array.Empty<string>(), response.StatusCode);
                return (false, await ReadErrorsAsync(response), response.StatusCode);
            }
            catch (HttpRequestException)
            {
                return (false, new[] { UnavailableMessage }, null);
            }
        }

        private static async Task<string[]> ReadErrorsAsync(HttpResponseMessage response)
        {
            // 429: API-nin sürət limiti
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
                return new[] { "Too many attempts. Wait a minute and try again." };

            try
            {
                var error = await response.Content.ReadFromJsonAsync<ApiErrorVM>();
                return error?.Errors is { Length: > 0 } ? error.Errors : new[] { "Something went wrong. Please try again." };
            }
            catch (JsonException)
            {
                return new[] { "Something went wrong. Please try again." };
            }
        }

        // JWT bitibsə (API 401 qaytarır) cookie-ni də bağlayıb əvvəldən giriş istəyirik
        protected async Task<IActionResult> SessionExpiredAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Welcome", "Account");
        }
    }
}
