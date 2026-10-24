using CaspianBank_MVC_FinalProject.ViewModels.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CaspianBank_MVC_FinalProject.Controllers
{
    // Bildirişlər (müştəri səhifəsi). Siyahı və "oxundu" API-dədir; canlı hissə brauzerin SignalR bağlantısıdır (wwwroot/js/notifications.js).
    // Open və Ticket həm müştərinin, həm işçinin zəngi üçün işləyir (giriş etmiş hər kəs)
    [Authorize]
    [Route("App/Notifications")]
    public class NotificationsController : ApiControllerBase
    {
        private const int PageSize = 20;

        public NotificationsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        [HttpGet("")]
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
            model.Area = string.Empty;
            return View(model);
        }

        // Bildirişə basanda: oxundu işarələnir, sonra bildirişin yoluna keçilir. "to" yalnız saytın öz yolu ola bilər (açıq yönləndirmə olmasın)
        [HttpGet("Open/{id:int}")]
        public async Task<IActionResult> Open(int id, string? to)
        {
            var (_, _, status) = await PostAsync($"api/notifications/{id}/read", new { });
            if (status == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }

            if (!string.IsNullOrEmpty(to) && Url.IsLocalUrl(to))
            {
                return LocalRedirect(to);
            }
            return RedirectToAction(nameof(Index));
        }

        // Canlı gələn bildiriş API-dən ingiliscə gəlir: brauzer onu göstərməzdən əvvəl seçilmiş dilə tərcümə etdirir.
        // Mətnlər URL-ə yox, sorğunun gövdəsinə yazılır (məbləğ və adlar ünvanda qalmasın)
        [HttpPost("Translate")]
        [IgnoreAntiforgeryToken]
        public IActionResult Translate([FromBody] NoteTextVM model)
        {
            var translator = HttpContext.RequestServices.GetRequiredService<CaspianBank_MVC_FinalProject.Services.ITranslator>();
            return Json(new
            {
                title = translator[model.Title ?? string.Empty],
                message = translator[model.Message ?? string.Empty]
            });
        }

        public class NoteTextVM
        {
            public string? Title { get; set; }
            public string? Message { get; set; }
        }

        [HttpPost("MarkAll")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAll()
        {
            var (_, _, status) = await PostAsync("api/notifications/read-all", new { });
            if (status == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // Brauzerin canlı kanala qoşulması üçün: əsas JWT cookie-də gizli qalır, JS yalnız 2 dəqiqəlik hub biletini alır.
        // Bilet yalnız bildiriş hub-ında keçir, API endpoint-lərində keçmir
        [HttpGet("Ticket")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Ticket()
        {
            var (ticket, _, status) = await PostAsync<HubTicketUIVM>("api/notifications/hub-token", new { });
            if (status == HttpStatusCode.Unauthorized)
            {
                return Unauthorized();
            }
            if (ticket is null)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable);
            }

            var baseUrl = (Configuration["ApiSettings:BaseUrl"] ?? string.Empty).TrimEnd('/');
            return Json(new
            {
                hubUrl = baseUrl + "/hubs/notifications",
                token = ticket.Token
            });
        }
    }
}
