using CaspianBank_MVC_FinalProject.ViewModels.Cards;
using CaspianBank_MVC_FinalProject.ViewModels.Transfers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Net;

namespace CaspianBank_MVC_FinalProject.Controllers
{
    // Aylıq təkrarlanan köçürmələr: hər ayın seçilmiş günü kartdan eyni məbləğ köçürülür. Qaydaları (alıcı, kart, limit, komissiya) API yoxlayır
    [Authorize]
    [Route("App/Recurring")]
    public class RecurringController : ApiControllerBase
    {
        public RecurringController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var (model, unauthorized) = await LoadAsync(new RecurringPageVM());
            return unauthorized ? await SessionExpiredAsync() : View(model);
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RecurringFormVM form)
        {
            var normalized = (form.Amount ?? string.Empty).Trim().Replace(" ", string.Empty).Replace(',', '.');
            if (!decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
            {
                return await ShowAsync(form, "Enter a valid amount.");
            }

            var own = form.Mode == "own";
            var (item, errors, status) = await PostAsync<RecurringTransferUIVM>("api/recurring-transfers", new
            {
                fromCardId = form.FromCardId,
                toCardId = own ? form.ToCardId : null,
                toCardNumber = own ? null : form.ToCardNumber,
                amount,
                dayOfMonth = form.DayOfMonth,
                note = form.Note
            });
            if (status == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }
            if (item is null)
            {
                return await ShowAsync(form, string.Join(" ", errors));
            }

            TempData["Success"] = "Done. " + item.Amount.ToString("N2", CultureInfo.InvariantCulture) + " AZN will go to " + item.ToLabel
                + " on day " + item.DayOfMonth + " of every month. The next transfer is on " + item.NextRunDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) + ".";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("{id:int}/Pause")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pause(int id)
        {
            return await ChangeAsync($"api/recurring-transfers/{id}/pause", "The monthly transfer is paused.");
        }

        [HttpPost("{id:int}/Resume")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Resume(int id)
        {
            return await ChangeAsync($"api/recurring-transfers/{id}/resume", "The monthly transfer is on again. The next one is on the chosen day.");
        }

        [HttpPost("{id:int}/Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, errors, status) = await DeleteAsync($"api/recurring-transfers/{id}");
            if (status == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }
            TempData[success ? "Success" : "CardError"] = success ? "The monthly transfer was deleted." : string.Join(" ", errors);
            return RedirectToAction(nameof(Index));
        }

        private async Task<IActionResult> ChangeAsync(string url, string message)
        {
            var (success, errors, status) = await PostAsync(url, new { });
            if (status == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }
            TempData[success ? "Success" : "CardError"] = success ? message : string.Join(" ", errors);
            return RedirectToAction(nameof(Index));
        }

        private async Task<IActionResult> ShowAsync(RecurringFormVM form, string error)
        {
            var (model, unauthorized) = await LoadAsync(new RecurringPageVM { Form = form, Error = error });
            return unauthorized ? await SessionExpiredAsync() : View("Index", model);
        }

        private async Task<(RecurringPageVM Model, bool Unauthorized)> LoadAsync(RecurringPageVM model)
        {
            var (items, status) = await GetAsync<List<RecurringTransferUIVM>>("api/recurring-transfers");
            if (status == HttpStatusCode.Unauthorized)
            {
                return (model, true);
            }
            var (cards, _) = await GetAsync<List<CardUIVM>>("api/cards");
            model.ApiUnavailable = items is null || cards is null;
            model.Items = items ?? new List<RecurringTransferUIVM>();
            model.Cards = cards is null ? new List<CardUIVM>() : Absolutize(cards);
            return (model, false);
        }
    }
}
