using CaspianBank_MVC_FinalProject.Controllers;
using CaspianBank_MVC_FinalProject.Helpers;
using CaspianBank_MVC_FinalProject.ViewModels.Bank;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Net;

namespace CaspianBank_MVC_FinalProject.Areas.Admin.Controllers
{
    // BANK bölməsi: bankın xəzinəsi və jurnalı (hər giriş və çıxış). Maliyyə məlumatıdır: SuperAdmin, Admin və Accountant.
    // Sətirləri API əməliyyatlarla birlikdə yazır, burada yalnız oxunur
    [Area("Admin")]
    [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.Admin + "," + AppRoles.Accountant)]
    public class TreasuryController : ApiControllerBase
    {
        private const int PageSize = 25;

        public TreasuryController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? kind, DateTime? from, DateTime? to, int page = 1)
        {
            var url = "api/admin/treasury?page=" + Math.Max(1, page) + "&pageSize=" + PageSize;
            if (!string.IsNullOrWhiteSpace(kind))
            {
                url += "&kind=" + Uri.EscapeDataString(kind);
            }
            if (from.HasValue)
            {
                url += "&from=" + from.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
            if (to.HasValue)
            {
                url += "&to=" + to.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }

            var (data, errors, status) = await GetWithErrorsAsync<TreasuryPageVM>(url);
            if (status == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }

            var model = data ?? new TreasuryPageVM { PageSize = PageSize, Page = 1 };
            if (data is null)
            {
                if (status == HttpStatusCode.BadRequest)
                {
                    model.Error = string.Join(" ", errors);
                }
                else
                {
                    model.Unavailable = true;
                }
            }
            model.Kind = kind;
            model.From = from;
            model.To = to;
            return View(model);
        }
    }
}
