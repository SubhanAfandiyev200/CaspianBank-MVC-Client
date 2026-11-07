using CaspianBank_MVC_FinalProject.Controllers;
using CaspianBank_MVC_FinalProject.Helpers;
using CaspianBank_MVC_FinalProject.ViewModels.Cards;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CaspianBank_MVC_FinalProject.Areas.Admin.Controllers
{
    // BANK bölməsi: kartların (hesabların) reyestri. Bütün işçilər görür, yalnız oxuma. Sahibinin səhifəsinə keçib hesabı dondurmaq olur.
    // Bütün qaydalar API-dədir (api/admin/cards)
    [Area("Admin")]
    [Authorize(Roles = AppRoles.Staff)]
    public class CardsController : ApiControllerBase
    {
        private const int PageSize = 20;

        public CardsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search, string? tier, string? status, int page = 1)
        {
            var url = "api/admin/cards?page=" + Math.Max(1, page) + "&pageSize=" + PageSize;
            if (!string.IsNullOrWhiteSpace(search))
            {
                url += "&search=" + Uri.EscapeDataString(search.Trim());
            }
            if (!string.IsNullOrWhiteSpace(tier))
            {
                url += "&tier=" + Uri.EscapeDataString(tier);
            }
            if (!string.IsNullOrWhiteSpace(status))
            {
                url += "&status=" + Uri.EscapeDataString(status);
            }

            var (data, httpStatus) = await GetAsync<AdminCardPageVM>(url);
            if (httpStatus == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }

            var model = data ?? new AdminCardPageVM { Unavailable = true, PageSize = PageSize, Page = 1 };
            model.Search = search;
            model.Tier = tier;
            model.Status = status;
            return View(model);
        }
    }
}
