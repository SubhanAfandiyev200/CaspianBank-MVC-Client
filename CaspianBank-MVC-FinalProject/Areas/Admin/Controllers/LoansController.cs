using CaspianBank_MVC_FinalProject.Controllers;
using CaspianBank_MVC_FinalProject.Helpers;
using CaspianBank_MVC_FinalProject.ViewModels.LoanReviews;
using CaspianBank_MVC_FinalProject.ViewModels.Loans;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Net;

namespace CaspianBank_MVC_FinalProject.Areas.Admin.Controllers
{
    // BANK bölməsi: kredit müraciətlərinə baxış. İşçi müraciətçinin reyestr məlumatını, iş yerlərini, başqa banklardakı kreditlərini
    // və qaydalara görə hesablanmış rəqəmləri görür, sonra təsdiq (tam və ya azaldılmış məbləğ) və ya rədd edir.
    // Pul qərarlarını Accountant, Admin və SuperAdmin verir. API: api/admin/loans
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin,Accountant")]
    public class LoansController : ApiControllerBase
    {
        private const int PageSize = 20;

        public LoansController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? status, string? search, int page = 1)
        {
            var url = "api/admin/loans?page=" + Math.Max(1, page) + "&pageSize=" + PageSize;
            if (!string.IsNullOrWhiteSpace(status))
            {
                url += "&status=" + Uri.EscapeDataString(status);
            }
            if (!string.IsNullOrWhiteSpace(search))
            {
                url += "&search=" + Uri.EscapeDataString(search.Trim());
            }

            var (data, httpStatus) = await GetAsync<LoanReviewPageVM>(url);
            if (httpStatus == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }

            var model = data ?? new LoanReviewPageVM { Unavailable = true, PageSize = PageSize, Page = 1 };
            model.Status = status;
            model.Search = search;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var (model, unauthorized, notFound) = await LoadDetailAsync(id);
            if (unauthorized)
            {
                return await SessionExpiredAsync();
            }
            if (notFound || model is null)
            {
                return NotFound();
            }
            return View(model);
        }

        // Təsdiq: məbləğ istənilənə bərabərdirsə kredit dərhal verilir, azdırsa müştəriyə təklif gedir (API qərar verir)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, string? approvedAmount)
        {
            var normalized = (approvedAmount ?? string.Empty).Trim().Replace(" ", string.Empty).Replace(',', '.');
            if (!decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
            {
                return await ShowDetailAsync(id, approvedAmount, null, "Enter a valid amount.");
            }

            var (summary, errors, status) = await PostAsync<LoanSummaryUIVM>($"api/admin/loans/{id}/approve", new
            {
                approvedAmount = amount
            });
            if (status == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }
            if (summary is null)
            {
                return await ShowDetailAsync(id, approvedAmount, null, string.Join(" ", errors));
            }

            TempData["Success"] = summary.Status == "Offered"
                ? "A smaller amount was offered. The customer must accept it before the money is sent."
                : "The loan was approved and the money was sent to the customer's card.";
            return RedirectToAction(nameof(Detail), "Loans", new { area = "Admin", id });
        }

        // Rədd səbəbi daxilidir: müştəri ümumi mətn görür
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string? reason)
        {
            var (summary, errors, status) = await PostAsync<LoanSummaryUIVM>($"api/admin/loans/{id}/reject", new
            {
                reason = reason ?? string.Empty
            });
            if (status == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }
            if (summary is null)
            {
                return await ShowDetailAsync(id, null, reason, string.Join(" ", errors));
            }

            TempData["Success"] = "The application was rejected.";
            return RedirectToAction(nameof(Detail), "Loans", new { area = "Admin", id });
        }

        private async Task<IActionResult> ShowDetailAsync(int id, string? approvedText, string? rejectText, string error)
        {
            var (model, unauthorized, notFound) = await LoadDetailAsync(id);
            if (unauthorized)
            {
                return await SessionExpiredAsync();
            }
            if (notFound || model is null)
            {
                TempData["Error"] = error;
                return RedirectToAction(nameof(Index), "Loans", new { area = "Admin" });
            }

            model.ApprovedAmountText = approvedText;
            model.RejectReasonText = rejectText;
            model.Error = error;
            return View("Detail", model);
        }

        private async Task<(LoanReviewDetailVM? Model, bool Unauthorized, bool NotFound)> LoadDetailAsync(int id)
        {
            var (model, status) = await GetAsync<LoanReviewDetailVM>($"api/admin/loans/{id}");
            if (status == HttpStatusCode.Unauthorized)
            {
                return (null, true, false);
            }
            if (model is null)
            {
                return (null, false, true);
            }

            // Şəkil API-dən verilir, ona görə tam ünvan lazımdır
            model.Applicant.PhotoUrl = ApiUrl.ToAbsolute(Configuration["ApiSettings:BaseUrl"], model.Applicant.PhotoUrl);
            return (model, false, false);
        }
    }
}
