using CaspianBank_MVC_FinalProject.Controllers;
using CaspianBank_MVC_FinalProject.Helpers;
using CaspianBank_MVC_FinalProject.ViewModels.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CaspianBank_MVC_FinalProject.Areas.Admin.Controllers
{
    // BANK bölməsi: müştərilər. Siyahı və profil bütün işçilərə açıqdır, hesabı dondurmaq və açmaq yalnız Admin, SuperAdmin və Security-yə.
    // Bütün qaydalar API-dədir (api/admin/users): burada yalnız səhifə və forma var
    [Area("Admin")]
    [Authorize(Roles = AppRoles.Staff)]
    public class UsersController : ApiControllerBase
    {
        private const int PageSize = 20;
        private const string RestrictRoles = AppRoles.SuperAdmin + "," + AppRoles.Admin + "," + AppRoles.Security;

        public UsersController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search, string? status, int page = 1)
        {
            var url = "api/admin/users?page=" + Math.Max(1, page) + "&pageSize=" + PageSize;
            if (!string.IsNullOrWhiteSpace(search))
            {
                url += "&search=" + Uri.EscapeDataString(search.Trim());
            }
            if (!string.IsNullOrWhiteSpace(status))
            {
                url += "&status=" + Uri.EscapeDataString(status);
            }

            var (data, httpStatus) = await GetAsync<UserPageVM>(url);
            if (httpStatus == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }

            var model = data ?? new UserPageVM { Unavailable = true, PageSize = PageSize, Page = 1 };
            model.Search = search;
            model.Status = status;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(string id)
        {
            var (user, status) = await GetAsync<UserDetailVM>("api/admin/users/" + Uri.EscapeDataString(id));
            if (status == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }
            if (user is null)
            {
                if (status == HttpStatusCode.NotFound)
                {
                    return NotFound();
                }
                TempData["Error"] = "We can't load this customer right now. Please try again.";
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RestrictRoles)]
        public async Task<IActionResult> Restrict(string id, string? reason)
        {
            var (user, errors, status) = await PostAsync<UserDetailVM>("api/admin/users/" + Uri.EscapeDataString(id) + "/restrict", new { reason });
            if (status == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }

            if (user is null)
            {
                // Səbəb çatmırsa və ya hesab artıq dondurulubsa səhifə yenidən açılır, yazılan səbəb qalır
                var (current, _) = await GetAsync<UserDetailVM>("api/admin/users/" + Uri.EscapeDataString(id));
                if (current is null)
                {
                    TempData["Error"] = string.Join(" ", errors);
                    return RedirectToAction(nameof(Index));
                }
                current.Error = string.Join(" ", errors);
                current.ReasonDraft = reason;
                return View("Detail", current);
            }

            TempData["Success"] = user.FullName + "'s account was restricted.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RestrictRoles)]
        public async Task<IActionResult> Unrestrict(string id)
        {
            var (user, errors, status) = await PostAsync<UserDetailVM>("api/admin/users/" + Uri.EscapeDataString(id) + "/unrestrict", new { });
            if (status == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }

            if (user is null)
            {
                TempData["Error"] = string.Join(" ", errors);
                return RedirectToAction(nameof(Detail), new { id });
            }

            TempData["Success"] = user.FullName + "'s account is active again.";
            return RedirectToAction(nameof(Detail), new { id });
        }
    }
}
