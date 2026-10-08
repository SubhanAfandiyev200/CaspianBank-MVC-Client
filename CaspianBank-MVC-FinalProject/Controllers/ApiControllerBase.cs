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

        // PUT (JSON): fayl olmayan formaların yenilənməsi üçün. Uğurlu olarsa cavabın gövdəsi Data-da qayıdır
        protected async Task<(T? Data, string[] Errors, HttpStatusCode? Status)> PutAsync<T>(string url, object body) where T : class
        {
            try
            {
                var response = await Api.PutAsJsonAsync(url, body);
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

        // PUT (JSON), cavab gövdəsi yoxdur: API yeniləmədən sonra boş cavab (204) qaytarır
        protected async Task<(bool Success, string[] Errors, HttpStatusCode? Status)> PutAsync(string url, object body)
        {
            try
            {
                var response = await Api.PutAsJsonAsync(url, body);
                if (response.IsSuccessStatusCode) return (true, Array.Empty<string>(), response.StatusCode);
                return (false, await ReadErrorsAsync(response), response.StatusCode);
            }
            catch (HttpRequestException)
            {
                return (false, new[] { UnavailableMessage }, null);
            }
        }

        // POST (multipart/form-data), cavab gövdəsi yoxdur: fayl yükləyən formalar üçün
        protected async Task<(bool Success, string[] Errors, HttpStatusCode? Status)> PostFormAsync(string url, MultipartFormDataContent content)
        {
            try
            {
                var response = await Api.PostAsync(url, content);
                if (response.IsSuccessStatusCode) return (true, Array.Empty<string>(), response.StatusCode);
                return (false, await ReadErrorsAsync(response), response.StatusCode);
            }
            catch (HttpRequestException)
            {
                return (false, new[] { UnavailableMessage }, null);
            }
        }

        // PUT (multipart/form-data), cavab gövdəsi yoxdur
        protected async Task<(bool Success, string[] Errors, HttpStatusCode? Status)> PutFormAsync(string url, MultipartFormDataContent content)
        {
            try
            {
                var response = await Api.PutAsync(url, content);
                if (response.IsSuccessStatusCode) return (true, Array.Empty<string>(), response.StatusCode);
                return (false, await ReadErrorsAsync(response), response.StatusCode);
            }
            catch (HttpRequestException)
            {
                return (false, new[] { UnavailableMessage }, null);
            }
        }

        // DELETE: uğurlu olarsa Success = true, olmazsa istifadəçiyə göstəriləcək xətalar (404-də API gövdə qaytarmaya bilər)
        protected async Task<(bool Success, string[] Errors, HttpStatusCode? Status)> DeleteAsync(string url)
        {
            try
            {
                var response = await Api.DeleteAsync(url);
                if (response.IsSuccessStatusCode) return (true, Array.Empty<string>(), response.StatusCode);
                return (false, await ReadErrorsAsync(response), response.StatusCode);
            }
            catch (HttpRequestException)
            {
                return (false, new[] { UnavailableMessage }, null);
            }
        }

        // POST (multipart/form-data): fayl yükləyən formalar üçün. Uğurlu olarsa cavabın gövdəsi Data-da qayıdır,
        // olmazsa istifadəçiyə göstəriləcək xətalar (API-nin { isSuccess, errors } cavabından)
        protected async Task<(T? Data, string[] Errors, HttpStatusCode? Status)> PostFormAsync<T>(string url, MultipartFormDataContent content) where T : class
        {
            try
            {
                var response = await Api.PostAsync(url, content);
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

        // PUT (multipart/form-data): PostFormAsync ilə eyni, yeniləmə üçün
        protected async Task<(T? Data, string[] Errors, HttpStatusCode? Status)> PutFormAsync<T>(string url, MultipartFormDataContent content) where T : class
        {
            try
            {
                var response = await Api.PutAsync(url, content);
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

        private async Task<string[]> ReadErrorsAsync(HttpResponseMessage response)
        {
            // 429: API-nin sürət limiti
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
                return new[] { "Too many attempts. Wait a minute and try again." };

            var body = string.Empty;
            try
            {
                body = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(body);
                var root = document.RootElement;

                if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("errors", out var errors))
                {
                    var messages = new List<string>();

                    // Bizim API: { isSuccess: false, errors: ["..."] }
                    if (errors.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in errors.EnumerateArray())
                        {
                            if (item.ValueKind == JsonValueKind.String) messages.Add(item.GetString()!);
                        }
                    }
                    // ASP.NET-in öz doğrulama cavabı: { errors: { "Field": ["..."] } }
                    else if (errors.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var field in errors.EnumerateObject())
                        {
                            foreach (var item in field.Value.EnumerateArray())
                            {
                                if (item.ValueKind == JsonValueKind.String) messages.Add(item.GetString()!);
                            }
                        }
                    }

                    if (messages.Count > 0) return messages.ToArray();
                }
            }
            catch (JsonException)
            {
            }

            // Gözlənilməz cavab (məs. 415, 403, boş gövdə): istifadəçiyə ümumi mesaj, səbəbi tapmaq üçün isə log-a status və gövdə yazılır
            var logger = HttpContext?.RequestServices.GetService<ILoggerFactory>()?.CreateLogger("ApiCall");
            logger?.LogWarning("API returned {Status} for {Method} {Url}. Body: {Body}",
                (int)response.StatusCode,
                response.RequestMessage?.Method,
                response.RequestMessage?.RequestUri,
                body.Length > 500 ? body.Substring(0, 500) : body);

            return new[] { "Something went wrong. Please try again." };
        }

        // JWT bitibsə (API 401 qaytarır) cookie-ni də bağlayıb əvvəldən giriş istəyirik
        protected async Task<IActionResult> SessionExpiredAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Welcome", "Account");
        }
    }
}
