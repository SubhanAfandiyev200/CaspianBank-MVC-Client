using CaspianBank_MVC_FinalProject.Controllers;
using CaspianBank_MVC_FinalProject.Helpers;
using CaspianBank_MVC_FinalProject.ViewModels.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CaspianBank_MVC_FinalProject.Areas.Admin.Controllers
{
    // BANK bölməsi: müştərilərin bildirdiyi şübhəli əməliyyatlar. Siyahını bütün işçilər görür, qərarı (baxıldı, tədbir, əsassız)
    // yalnız SuperAdmin, Admin və Security verir. Qaydaları API yoxlayır
    [Area("Admin")]
    [Authorize(Roles = AppRoles.Staff)]
    public class ReportsController : ApiControllerBase
    {
        private const int PageSize = 20;

        public ReportsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        // Süzgəc verilməyibsə baxılmamış bildirişlər göstərilir
        [HttpGet]
        public async Task<IActionResult> Index(string? status, string? search, int page = 1)
        {
            var effective = status ?? "Pending";
            if (effective == "All")
            {
                effective = string.Empty;
            }

            var url = "api/admin/reports?page=" + Math.Max(1, page) + "&pageSize=" + PageSize;
            if (!string.IsNullOrEmpty(effective))
            {
                url += "&status=" + Uri.EscapeDataString(effective);
            }
            if (!string.IsNullOrWhiteSpace(search))
            {
                url += "&search=" + Uri.EscapeDataString(search.Trim());
            }

            var (data, errors, code) = await GetWithErrorsAsync<AdminReportsPageVM>(url);
            if (code == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }

            var model = data ?? new AdminReportsPageVM { PageSize = PageSize, Page = 1 };
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
            model.Status = status ?? "Pending";
            model.Search = search;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.Admin + "," + AppRoles.Security)]
        public async Task<IActionResult> Resolve(int id, string outcome, string? note, string? returnStatus)
        {
            var (success, errors, code) = await PostAsync($"api/admin/reports/{id}/resolve", new { outcome, note });
            if (code == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }

            if (success)
            {
                TempData["Success"] = outcome == "Action" ? "Action was taken and the customer was told."
                    : outcome == "Dismiss" ? "The report was dismissed."
                    : "The report was marked as reviewed.";
            }
            else
            {
                TempData["Error"] = code == HttpStatusCode.NotFound ? "This report no longer exists." : string.Join(" ", errors);
            }
            return RedirectToAction(nameof(Index), new { status = returnStatus });
        }
    }
}
