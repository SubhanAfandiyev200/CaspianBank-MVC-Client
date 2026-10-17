using CaspianBank_MVC_FinalProject.Controllers;
using CaspianBank_MVC_FinalProject.Helpers;
using CaspianBank_MVC_FinalProject.ViewModels.Audit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Net;

namespace CaspianBank_MVC_FinalProject.Areas.Admin.Controllers
{
    // BANK bölməsi: audit log (kim nə etdi). Həssasdır: yalnız SuperAdmin, Admin və Security. Qeydləri API yazır, burada yalnız oxunur
    [Area("Admin")]
    [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.Admin + "," + AppRoles.Security)]
    public class AuditController : ApiControllerBase
    {
        private const int PageSize = 25;

        public AuditController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search, string? kind, DateTime? from, DateTime? to, int page = 1)
        {
            var url = "api/admin/audit?page=" + Math.Max(1, page) + "&pageSize=" + PageSize;
            if (!string.IsNullOrWhiteSpace(search))
            {
                url += "&search=" + Uri.EscapeDataString(search.Trim());
            }
            if (!string.IsNullOrWhiteSpace(kind))
            {
                // "action" MVC-nin marşrut açarıdır, ona görə formada və ünvanda "kind" adlanır; API-yə action kimi gedir
                url += "&action=" + Uri.EscapeDataString(kind);
            }
            if (from.HasValue)
            {
                url += "&from=" + from.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
            if (to.HasValue)
            {
                url += "&to=" + to.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }

            var (data, errors, status) = await GetWithErrorsAsync<AuditPageVM>(url);
            if (status == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }

            var model = data ?? new AuditPageVM { PageSize = PageSize, Page = 1 };
            if (data is null)
            {
                // 400 (məs. başlanğıc tarix sondan böyükdür) istifadəçiyə deyilir, qalanı "yüklənmədi"dir
                if (status == HttpStatusCode.BadRequest)
                {
                    model.Error = string.Join(" ", errors);
                }
                else
                {
                    model.Unavailable = true;
                }
            }
            model.Search = search;
            model.Action = kind;
            model.From = from;
            model.To = to;
            return View(model);
        }
    }
}
