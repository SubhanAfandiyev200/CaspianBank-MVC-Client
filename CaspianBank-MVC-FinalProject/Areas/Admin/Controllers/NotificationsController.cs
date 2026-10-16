using CaspianBank_MVC_FinalProject.Controllers;
using CaspianBank_MVC_FinalProject.Helpers;
using CaspianBank_MVC_FinalProject.ViewModels.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CaspianBank_MVC_FinalProject.Areas.Admin.Controllers
{
    // İşçinin bildirişləri (yeni kredit müraciəti və s.). Siyahı admin görünüşündədir; basanda keçid (Open) və canlı bilet müştəri controller-indədir
    [Area("Admin")]
    [Authorize(Roles = AppRoles.Staff)]
    public class NotificationsController : ApiControllerBase
    {
        private const int PageSize = 20;

        public NotificationsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index(bool unread = false, int page = 1)
        {
            var url = "api/notifications?page=" + Math.Max(1, page) + "&pageSize=" + PageSize + (unread ? "&unreadOnly=true" : string.Empty);
            var (data, status) = await GetAsync<NotificationPageUIVM>(url);
            if (status == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }

            var model = data ?? new NotificationPageUIVM { Unavailable = true };
            model.UnreadOnly = unread;
            model.Area = "Admin";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAll()
        {
            var (_, _, status) = await PostAsync("api/notifications/read-all", new { });
            if (status == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }

            return RedirectToAction(nameof(Index), "Notifications", new { area = "Admin" });
        }
    }
}
