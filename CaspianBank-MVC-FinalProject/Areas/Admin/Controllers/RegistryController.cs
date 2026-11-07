using CaspianBank_MVC_FinalProject.Controllers;
using CaspianBank_MVC_FinalProject.Helpers;
using CaspianBank_MVC_FinalProject.ViewModels.Registry;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CaspianBank_MVC_FinalProject.Areas.Admin.Controllers
{
    // BANK bölməsi: dövlət reyestri (vətəndaşlar, qara siyahı, vəfat edənlər, hesabı olanlar). Bütün işçilər görür, yalnız oxuma.
    // Bütün qaydalar API-dədir (api/admin/registry)
    [Area("Admin")]
    [Authorize(Roles = AppRoles.Staff)]
    public class RegistryController : ApiControllerBase
    {
        private const int PageSize = 20;

        public RegistryController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search, string? status, int page = 1)
        {
            var url = "api/admin/registry?page=" + Math.Max(1, page) + "&pageSize=" + PageSize;
            if (!string.IsNullOrWhiteSpace(search))
            {
                url += "&search=" + Uri.EscapeDataString(search.Trim());
            }
            if (!string.IsNullOrWhiteSpace(status))
            {
                url += "&status=" + Uri.EscapeDataString(status);
            }

            var (data, errors, httpStatus) = await GetWithErrorsAsync<RegistryPageVM>(url);
            if (httpStatus == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }

            var model = data ?? new RegistryPageVM { PageSize = PageSize, Page = 1 };
            if (data is null)
            {
                if (httpStatus == HttpStatusCode.BadRequest)
                {
                    model.Error = string.Join(" ", TranslateErrors(errors));
                }
                else
                {
                    model.Unavailable = true;
                }
            }

            var baseUrl = Configuration["ApiSettings:BaseUrl"];
            foreach (var item in model.Items)
            {
                item.PhotoUrl = string.IsNullOrEmpty(item.PhotoUrl) ? null : ApiUrl.ToAbsolute(baseUrl, item.PhotoUrl);
            }
            model.Search = search;
            model.Status = status;
            return View(model);
        }
    }
}
