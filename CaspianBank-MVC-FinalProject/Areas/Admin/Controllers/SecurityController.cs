using CaspianBank_MVC_FinalProject.Controllers;
using CaspianBank_MVC_FinalProject.Helpers;
using CaspianBank_MVC_FinalProject.ViewModels.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CaspianBank_MVC_FinalProject.Areas.Admin.Controllers
{
    // BANK bölməsi: şübhəli fəaliyyət. Siqnalları sistem yaradır, burada Security (və Admin, SuperAdmin) baxır:
    // "təhlükəsizdir" və ya "şübhəlidir, hesabı dondur". Qaydalar və yoxlamalar API-dədir
    [Area("Admin")]
    [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.Admin + "," + AppRoles.Security)]
    public class SecurityController : ApiControllerBase
    {
        private const int PageSize = 20;

        public SecurityController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        // Süzgəc verilməyibsə açıq siqnallar göstərilir
        [HttpGet]
        public async Task<IActionResult> Index(string? status, string? severity, string? kind, string? search, int page = 1)
        {
            var effectiveStatus = status ?? "Open";
            if (effectiveStatus == "All")
            {
                effectiveStatus = string.Empty;
            }

            var url = "api/admin/security/alerts?page=" + Math.Max(1, page) + "&pageSize=" + PageSize;
            if (!string.IsNullOrEmpty(effectiveStatus))
            {
                url += "&status=" + Uri.EscapeDataString(effectiveStatus);
            }
            if (!string.IsNullOrWhiteSpace(severity))
            {
                url += "&severity=" + Uri.EscapeDataString(severity);
            }
            if (!string.IsNullOrWhiteSpace(kind))
            {
                url += "&kind=" + Uri.EscapeDataString(kind);
            }
            if (!string.IsNullOrWhiteSpace(search))
            {
                url += "&search=" + Uri.EscapeDataString(search.Trim());
            }

            var (data, errors, code) = await GetWithErrorsAsync<SecurityPageVM>(url);
            if (code == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }

            var model = data ?? new SecurityPageVM { PageSize = PageSize, Page = 1 };
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
            model.Status = status ?? "Open";
            model.Severity = severity;
            model.Kind = kind;
            model.Search = search;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Resolve(int id, string outcome, string? note, string? returnStatus)
        {
            var (success, errors, status) = await PostAsync($"api/admin/security/alerts/{id}/resolve", new { outcome, note });
            if (status == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }

            if (success)
            {
                TempData["Success"] = outcome == "Freeze"
                    ? "The account was frozen and the alert is closed."
                    : "The alert was marked as safe.";
            }
            else
            {
                TempData["Error"] = status == HttpStatusCode.NotFound ? "This alert no longer exists." : string.Join(" ", errors);
            }
            return RedirectToAction(nameof(Index), new { status = returnStatus });
        }
    }
}
