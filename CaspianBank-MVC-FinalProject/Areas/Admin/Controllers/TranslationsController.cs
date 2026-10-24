using CaspianBank_MVC_FinalProject.Controllers;
using CaspianBank_MVC_FinalProject.Helpers;
using CaspianBank_MVC_FinalProject.Services;
using CaspianBank_MVC_FinalProject.ViewModels.Translations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CaspianBank_MVC_FinalProject.Areas.Admin.Controllers
{
    // UI bölməsi: saytın tərcümələri (Azərbaycan və rus). Açar ingilis mətnin özüdür; boş saxlanan tərcümə ingilis mətnə qayıdır.
    // Yalnız SuperAdmin və Admin. Saxladıqdan sonra yaddaşdakı lüğət təzələnir ki, dəyişiklik dərhal görünsün
    [Area("Admin")]
    [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.Admin)]
    public class TranslationsController : ApiControllerBase
    {
        private const int PageSize = 30;

        private readonly ITranslationStore _store;

        public TranslationsController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ITranslationStore store)
            : base(httpClientFactory, configuration)
        {
            _store = store;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search, bool onlyMissing, int page = 1)
        {
            var url = "api/admin/translations?page=" + Math.Max(1, page) + "&pageSize=" + PageSize + "&onlyMissing=" + (onlyMissing ? "true" : "false");
            if (!string.IsNullOrWhiteSpace(search))
            {
                url += "&search=" + Uri.EscapeDataString(search.Trim());
            }

            var (data, errors, code) = await GetWithErrorsAsync<TranslationsPageVM>(url);
            if (code == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }

            var model = data ?? new TranslationsPageVM { PageSize = PageSize, Page = 1 };
            if (data is null)
            {
                if (code == HttpStatusCode.BadRequest)
                {
                    model.Error = string.Join(" ", errors);
                }
                else
                {
                    model.Unavailable = true;
                }
            }
            model.Search = search;
            model.OnlyMissing = onlyMissing;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(string key, string? az, string? ru, string? search, bool onlyMissing, int page = 1)
        {
            var (success, errors, code) = await PutAsync("api/admin/translations", new { key, az, ru });
            if (code == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }

            if (success)
            {
                _store.Invalidate();
                TempData["Success"] = "The translation was saved.";
            }
            else
            {
                TempData["Error"] = string.Join(" ", errors);
            }
            return RedirectToAction(nameof(Index), new { search, onlyMissing, page });
        }
    }
}
