using CaspianBank_MVC_FinalProject.Helpers;
using CaspianBank_MVC_FinalProject.ViewModels.Support;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CaspianBank_MVC_FinalProject.Controllers
{
    // Müştərinin "Contact us" səhifəsi (dizayn: CaspianBank-Frontend/contact-us.html). Üstdə köməkçi (AI) söhbət paneli (hələ önizləmə),
    // altda müştərinin dəstəyə yazdığı mesajlar: yeni mesaj, cavabı oxumaq, davam etmək, bağlamaq. Bütün qaydalar API-dədir (api/support/tickets).
    // Şirkətin adı, ünvanı və telefonu footer ilə eyni yerdən (api/settings) gəlir
    [Authorize]
    [Route("App/Support")]
    public class SupportController : ApiControllerBase
    {
        public SupportController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var (model, unauthorized) = await LoadPageAsync(new TicketFormVM());
            if (unauthorized)
            {
                return await SessionExpiredAsync();
            }
            return View(model);
        }

        // Yeni mesaj. Uğurlu olanda mesajın səhifəsinə keçir (PRG: yeniləmə təkrar göndərməsin)
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TicketFormVM form)
        {
            var (ticket, errors, status) = await PostAsync<TicketDetailVM>("api/support/tickets", new
            {
                subject = form.Subject,
                message = form.Message
            });
            if (status == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }

            if (ticket is null)
            {
                var (model, unauthorized) = await LoadPageAsync(form);
                if (unauthorized)
                {
                    return await SessionExpiredAsync();
                }
                model.Error = string.Join(" ", errors);
                return View("Index", model);
            }

            TempData["Success"] = "Your message was sent. We will answer here.";
            return RedirectToAction(nameof(Ticket), new { id = ticket.Id });
        }

        [HttpGet("Ticket/{id:int}")]
        public async Task<IActionResult> Ticket(int id)
        {
            var (ticket, status) = await GetAsync<TicketDetailVM>($"api/support/tickets/{id}");
            if (status == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }
            if (ticket is null)
            {
                TempData["Error"] = status == HttpStatusCode.NotFound ? "This message was not found." : "We can't load this message right now. Please try again.";
                return RedirectToAction(nameof(Index));
            }
            return View(ticket);
        }

        [HttpPost("Ticket/{id:int}/Reply")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(int id, string? message)
        {
            var (ticket, errors, status) = await PostAsync<TicketDetailVM>($"api/support/tickets/{id}/messages", new { message });
            if (status == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }

            if (ticket is null)
            {
                var (current, _) = await GetAsync<TicketDetailVM>($"api/support/tickets/{id}");
                if (current is null)
                {
                    TempData["Error"] = string.Join(" ", errors);
                    return RedirectToAction(nameof(Index));
                }
                current.Error = string.Join(" ", errors);
                current.Draft = message;
                return View("Ticket", current);
            }

            TempData["Success"] = "Your message was sent.";
            return RedirectToAction(nameof(Ticket), new { id });
        }

        [HttpPost("Ticket/{id:int}/Close")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Close(int id)
        {
            var (ticket, errors, status) = await PostAsync<TicketDetailVM>($"api/support/tickets/{id}/close", new { });
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
            return RedirectToAction(nameof(Ticket), new { id });
        }

        // ---------- köməkçilər ----------

        private async Task<(SupportPageVM Model, bool Unauthorized)> LoadPageAsync(TicketFormVM form)
        {
            var model = new SupportPageVM { Form = form };
            model.Settings = await SiteSettingsLoader.LoadAsync(HttpContext, HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>(), Configuration);

            var (tickets, status) = await GetAsync<List<TicketVM>>("api/support/tickets");
            if (status == HttpStatusCode.Unauthorized)
            {
                return (model, true);
            }
            model.Tickets = tickets ?? new List<TicketVM>();
            return (model, false);
        }
    }
}
