using CaspianBank_MVC_FinalProject.Controllers;
using CaspianBank_MVC_FinalProject.Helpers;
using CaspianBank_MVC_FinalProject.ViewModels.Bank;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CaspianBank_MVC_FinalProject.Areas.Admin.Controllers
{
    // BANK bölməsi: bankın ümumi maliyyə vəziyyəti (gəlir, xərc, vergi, ehtiyat, istifadəçilər). Rəqəmlər API-dədir (api/admin/dashboard)
    [Area("Admin")]
    [Authorize(Roles = AppRoles.Staff)]
    public class BankDashboardController : ApiControllerBase
    {
        public BankDashboardController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? month)
        {
            var url = "api/admin/dashboard";
            if (!string.IsNullOrWhiteSpace(month))
            {
                url += "?month=" + Uri.EscapeDataString(month.Trim());
            }

            var (data, errors, status) = await GetWithErrorsAsync<BankDashboardVM>(url);
            if (status == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }

            if (data is null)
            {
                // Yanlış ay (400) istifadəçiyə deyilir; cari ay yenidən yüklənir. Qalan hallarda "yüklənmədi"
                if (status == HttpStatusCode.BadRequest && !string.IsNullOrWhiteSpace(month))
                {
                    var (fallback, _, _) = await GetWithErrorsAsync<BankDashboardVM>("api/admin/dashboard");
                    if (fallback is not null)
                    {
                        fallback.Error = string.Join(" ", errors);
                        return View(fallback);
                    }
                }
                return View(new BankDashboardVM { Unavailable = true });
            }
            return View(data);
        }
    }
}
