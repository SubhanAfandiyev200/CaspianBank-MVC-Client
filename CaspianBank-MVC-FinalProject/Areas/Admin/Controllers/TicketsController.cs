using CaspianBank_MVC_FinalProject.Helpers;
using CaspianBank_MVC_FinalProject.Controllers;
using CaspianBank_MVC_FinalProject.ViewModels.Support;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CaspianBank_MVC_FinalProject.Areas.Admin.Controllers
{
    // BANK bölməsi: müştərilərin və ziyarətçilərin mesajları (Help). Hələlik bütün işçi rolları görür. Qaydalar API-dədir (api/admin/tickets)
    [Area("Admin")]
    [Authorize(Roles = AppRoles.Staff)]
    public class TicketsController : ApiControllerBase
    {
        private const int PageSize = 20;

        public TicketsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? status, string? search, int page = 1)
        {
            var url = "api/admin/tickets?page=" + Math.Max(1, page) + "&pageSize=" + PageSize;
            if (!string.IsNullOrWhiteSpace(status))
            {
                url += "&status=" + Uri.EscapeDataString(status);
            }
            if (!string.IsNullOrWhiteSpace(search))
            {
                url += "&search=" + Uri.EscapeDataString(search.Trim());
            }

            var (data, httpStatus) = await GetAsync<TicketPageVM>(url);
            if (httpStatus == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }

            var model = data ?? new TicketPageVM { Unavailable = true, PageSize = PageSize, Page = 1 };
            model.Status = status;
            model.Search = search;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var (ticket, status) = await GetAsync<TicketDetailVM>($"api/admin/tickets/{id}");
            if (status == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }
            if (ticket is null)
            {
                if (status == HttpStatusCode.NotFound)
                {
                    return NotFound();
                }
                TempData["Error"] = "We can't load this message right now. Please try again.";
                return RedirectToAction(nameof(Index));
            }
            return View(WithImages(ticket));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(int id, string? message)
        {
            var (ticket, errors, status) = await PostAsync<TicketDetailVM>($"api/admin/tickets/{id}/reply", new { message });
            if (status == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }

            if (ticket is null)
            {
                var (current, _) = await GetAsync<TicketDetailVM>($"api/admin/tickets/{id}");
                if (current is null)
                {
                    TempData["Error"] = string.Join(" ", errors);
                    return RedirectToAction(nameof(Index));
                }
                current.Error = string.Join(" ", errors);
                current.Draft = message;
                return View("Detail", WithImages(current));
            }

            TempData["Success"] = "Your reply was sent to " + ticket.CustomerName + ".";
            return RedirectToAction(nameof(Detail), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Close(int id)
        {
            var (ticket, errors, status) = await PostAsync<TicketDetailVM>($"api/admin/tickets/{id}/close", new { });
            if (status == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }

            if (ticket is null)
            {
                TempData["Error"] = string.Join(" ", errors);
            }
            else
            {
                TempData["Success"] = "The conversation was closed.";
            }
            return RedirectToAction(nameof(Detail), new { id });
        }
    }
}
