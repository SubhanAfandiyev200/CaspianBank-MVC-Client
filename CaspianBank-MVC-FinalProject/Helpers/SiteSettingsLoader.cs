using CaspianBank_MVC_FinalProject.ViewModels.Settings;

namespace CaspianBank_MVC_FinalProject.Helpers
{
    public static class SiteSettingsLoader
    {
        private const string CacheKey = "SiteSettings";

        // Header və footer eyni səhifədə iki ayrı ViewComponent-dir; API-yə bir dəfə müraciət olunsun deyə
        // nəticə yalnız cari sorğu üçün HttpContext.Items-də saxlanılır.
        public static async Task<SiteSettingsVM> LoadAsync(HttpContext httpContext,
                                                           IHttpClientFactory httpClientFactory,
                                                           IConfiguration configuration)
        {
            if (httpContext.Items[CacheKey] is SiteSettingsVM cached) return cached;

            var model = new SiteSettingsVM();

            try
            {
                var client = httpClientFactory.CreateClient("CaspianApi");
                var all = await client.GetFromJsonAsync<Dictionary<string, string>>("api/settings");

                if (all != null)
                {
                    model.Logo = ApiUrl.ToAbsolute(configuration["ApiSettings:BaseUrl"], Get(all, "Logo", ""));
                    model.CompanyName = Get(all, "CompanyName", model.CompanyName);
                    model.Address = Get(all, "Address", model.Address);
                    model.Email = Get(all, "Email", model.Email);
                    model.PhoneNumber = Get(all, "PhoneNumber", model.PhoneNumber);
                    model.FooterTitle = Get(all, "FooterTitle", model.FooterTitle);
                    model.FooterDescription = Get(all, "FooterDescription", model.FooterDescription);
                    model.InstagramUrl = Get(all, "InstagramUrl", model.InstagramUrl);
                    model.FacebookUrl = Get(all, "FacebookUrl", model.FacebookUrl);
                    model.TwitterUrl = Get(all, "TwitterUrl", model.TwitterUrl);
                    model.LinkedInUrl = Get(all, "LinkedInUrl", model.LinkedInUrl);
                    model.MapLatitude = Get(all, "MapLatitude", model.MapLatitude);
                    model.MapLongitude = Get(all, "MapLongitude", model.MapLongitude);
                }
            }
            catch (HttpRequestException)
            {
                // API işləmir: statik ehtiyat mətnlər qalır
            }

            httpContext.Items[CacheKey] = model;
            return model;
        }

        private static string Get(Dictionary<string, string> all, string key, string fallback)
            => all.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value.Trim() : fallback;
    }
}
