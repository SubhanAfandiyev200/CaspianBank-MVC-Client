using CaspianBank_MVC_FinalProject.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net.Http.Json;
using System.Text.Json;

namespace CaspianBank_MVC_FinalProject.Helpers
{
    // Admin yaratma/redaktə formasında EN | AZ | RU seçimi işlədilibsə (wwwroot/js/lang-edit.js) forma gizli "__translations" sahəsini də göndərir:
    // { "az": { "Title": "...", "Description": "..." }, "ru": { ... } }. Sahənin adı formadakı ingilis mətnin sahəsidir, açar da həmin ingilis mətndir.
    // Əməliyyat uğurlu bitibsə (xəta yoxdur və səhifə yönləndirilir) tərcümələr API-yə yazılır. Əməliyyat uğursuzdursa heç nə yazılmır
    public class SaveTranslationsFilter : IAsyncActionFilter
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITranslationStore _store;
        private readonly ILogger<SaveTranslationsFilter> _logger;

        public SaveTranslationsFilter(IHttpClientFactory httpClientFactory, ITranslationStore store, ILogger<SaveTranslationsFilter> logger)
        {
            _httpClientFactory = httpClientFactory;
            _store = store;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var executed = await next();

            var request = context.HttpContext.Request;
            if (!HttpMethods.IsPost(request.Method) || !request.HasFormContentType || !request.Form.TryGetValue("__translations", out var json) || string.IsNullOrWhiteSpace(json))
            {
                return;
            }
            if (executed.Exception is not null || !context.ModelState.IsValid || !(executed.Result is RedirectToActionResult || executed.Result is RedirectResult || executed.Result is LocalRedirectResult || executed.Result is RedirectToRouteResult))
            {
                return;
            }

            Dictionary<string, Dictionary<string, string>>? data;
            try
            {
                data = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(json.ToString());
            }
            catch (JsonException)
            {
                return;
            }
            if (data is null)
            {
                return;
            }

            var client = _httpClientFactory.CreateClient("CaspianApi");
            foreach (var language in new[] { "az", "ru" })
            {
                if (!data.TryGetValue(language, out var texts))
                {
                    continue;
                }

                var items = new List<object>();
                foreach (var pair in texts)
                {
                    var key = request.Form[pair.Key].ToString().Trim();
                    if (key.Length > 0)
                    {
                        items.Add(new { key, value = pair.Value ?? string.Empty });
                    }
                }
                if (items.Count == 0)
                {
                    continue;
                }

                try
                {
                    var response = await client.PutAsJsonAsync("api/admin/translations/content", new { language, items });
                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Tərcümələr yazılmadı: {Language} {Status}", language, (int)response.StatusCode);
                    }
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogWarning(ex, "Tərcümələr yazılmadı: {Language}", language);
                }
            }

            _store.Invalidate();
        }
    }
}
