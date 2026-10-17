using CaspianBank_MVC_FinalProject.ViewModels.Notifications;
using Microsoft.AspNetCore.Mvc;

namespace CaspianBank_MVC_FinalProject.ViewComponents
{
    // Header-dəki bildiriş zəngi: oxunmamışların sayı və son 6 bildiriş. Sonrası canlı gəlir (wwwroot/js/notifications.js, SignalR)
    public class NotificationBellViewComponent : ViewComponent
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public NotificationBellViewComponent(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // area: "" müştəri, "Admin" işçi ("View all" hansı səhifəyə aparsın)
        public async Task<IViewComponentResult> InvokeAsync(string area = "")
        {
            // Giriş etməyənin zəngi olmur
            if (User.Identity?.IsAuthenticated != true)
            {
                return Content(string.Empty);
            }

            var model = new NotificationBellVM { Area = area };
            try
            {
                var client = _httpClientFactory.CreateClient("CaspianApi");
                var page = await client.GetFromJsonAsync<NotificationPageUIVM>("api/notifications?page=1&pageSize=6");
                if (page is not null)
                {
                    model.UnreadCount = page.UnreadCount;
                    model.Items = page.Items;
                }
            }
            catch (HttpRequestException)
            {
                // API işləmirsə zəng boş açılır; səhifənin qalanı pozulmur
            }
            catch (System.Text.Json.JsonException)
            {
            }

            return View(model);
        }
    }
}
