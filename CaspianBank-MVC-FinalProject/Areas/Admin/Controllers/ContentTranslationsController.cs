using CaspianBank_MVC_FinalProject.Controllers;
using CaspianBank_MVC_FinalProject.Helpers;
using CaspianBank_MVC_FinalProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CaspianBank_MVC_FinalProject.Areas.Admin.Controllers
{
    // Admin redaktə səhifələrindəki dil seçimi (EN | AZ | RU) bu iki əməliyyatı çağırır (wwwroot/js/lang-edit.js).
    // Tərcümə ingilis mətnə bağlıdır: açar səhifədə saxlanmış ingilis mətnin özüdür. Yalnız SuperAdmin və Admin
    [Area("Admin")]
    [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.Admin)]
    public class ContentTranslationsController : ApiControllerBase
    {
        private readonly ITranslationStore _store;

        public ContentTranslationsController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ITranslationStore store)
            : base(httpClientFactory, configuration)
        {
            _store = store;
        }

        // Seçilən dilin tərcümələri: { "ingilis mətn": "tərcümə" }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Lookup(string language, string[] keys)
        {
            var (data, errors, status) = await PostAsync<Dictionary<string, string>>("api/admin/translations/lookup", new { language, keys });
            if (status == HttpStatusCode.Unauthorized)
            {
                return Unauthorized();
            }
            if (data is null)
            {
                return BadRequest(new { errors = Translate(errors) });
            }
            return Json(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(string language, string[] keys, string[] values)
        {
            var items = keys.Select((key, index) => new { key, value = index < values.Length ? values[index] : string.Empty }).ToList();
            var (success, errors, status) = await PutAsync("api/admin/translations/content", new { language, items });
            if (status == HttpStatusCode.Unauthorized)
            {
                return Unauthorized();
            }
            if (!success)
            {
                return BadRequest(new { errors = Translate(errors) });
            }

            // Saxlanandan sonra saytın yaddaşındakı lüğət təzələnir: dəyişiklik dərhal görünür
            _store.Invalidate();
            return Json(new { message = HttpContext.RequestServices.GetRequiredService<ITranslator>()["The translation was saved."] });
        }

        private string[] Translate(string[] errors)
        {
            var translator = HttpContext.RequestServices.GetRequiredService<ITranslator>();
            return errors.Select(error => translator[error]).ToArray();
        }
    }
}
