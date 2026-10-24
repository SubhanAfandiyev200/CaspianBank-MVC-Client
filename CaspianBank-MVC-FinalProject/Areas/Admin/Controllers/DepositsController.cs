using CaspianBank_MVC_FinalProject.Controllers;
using CaspianBank_MVC_FinalProject.Helpers;
using CaspianBank_MVC_FinalProject.ViewModels.Deposits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Net;

namespace CaspianBank_MVC_FinalProject.Areas.Admin.Controllers
{
    // BANK bölməsi: müştərilərin depozitləri və variantlar (müddət, faiz, hədlər). Siyahını bütün işçilər görür,
    // faiz və hədləri yalnız SuperAdmin, Admin və Accountant dəyişir. Qaydaları API yoxlayır
    [Area("Admin")]
    [Authorize(Roles = AppRoles.Staff)]
    public class DepositsController : ApiControllerBase
    {
        private const int PageSize = 20;

        public DepositsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search, string? status, int page = 1)
        {
            var url = "api/admin/deposits?page=" + Math.Max(1, page) + "&pageSize=" + PageSize;
            if (!string.IsNullOrWhiteSpace(search))
            {
                url += "&search=" + Uri.EscapeDataString(search.Trim());
            }
            if (!string.IsNullOrWhiteSpace(status))
            {
                url += "&status=" + Uri.EscapeDataString(status);
            }

            var (data, errors, code) = await GetWithErrorsAsync<DepositPageVM>(url);
            if (code == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }

            var model = data ?? new DepositPageVM { PageSize = PageSize, Page = 1 };
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
            model.Status = status;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.Admin + "," + AppRoles.Accountant)]
        public async Task<IActionResult> UpdatePlan(int id, string? rate, string? min, string? max, bool active)
        {
            if (!TryParse(rate, out var annualRate) || !TryParse(min, out var minAmount) || !TryParse(max, out var maxAmount))
            {
                TempData["Error"] = "Enter valid numbers for the rate and the amounts.";
                return RedirectToAction(nameof(Index));
            }

            var (success, errors, status) = await PutAsync($"api/admin/deposits/plans/{id}", new
            {
                annualRate,
                minAmount,
                maxAmount,
                isActive = active
            });
            if (!success)
            {
                if (status == HttpStatusCode.Unauthorized)
                {
                    return await SessionExpiredAsync();
                }
                TempData["Error"] = string.Join(" ", errors);
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = "The deposit term was updated. Deposits that are already open keep their old rate.";
            return RedirectToAction(nameof(Index));
        }

        private static bool TryParse(string? raw, out decimal value)
        {
            var normalized = (raw ?? string.Empty).Trim().Replace(" ", string.Empty).Replace(',', '.');
            return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
        }
    }
}
