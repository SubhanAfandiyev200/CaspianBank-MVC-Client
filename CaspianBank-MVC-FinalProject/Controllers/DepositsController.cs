using CaspianBank_MVC_FinalProject.ViewModels.Cards;
using CaspianBank_MVC_FinalProject.ViewModels.Deposits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Net;

namespace CaspianBank_MVC_FinalProject.Controllers
{
    // Müddətli depozitlər: variant seç, məbləğ yaz, kart seç. Faiz müddət bitəndə ödənir, müddətdən əvvəl çıxarmaq olmur.
    // Bütün qaydalar (hədlər, kartlar, faiz) API-dədir; burada yalnız forma və siyahı var. Açma adi form + PRG: yeniləmə depoziti təkrar açmasın
    [Authorize]
    [Route("App/Deposits")]
    public class DepositsController : ApiControllerBase
    {
        public DepositsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var (model, unauthorized) = await LoadAsync(new DepositIndexVM());
            return unauthorized ? await SessionExpiredAsync() : View(model);
        }

        // Eyni RequestId təkrar gələrsə API əvvəlki depoziti qaytarır (pul bir dəfə çıxır)
        [HttpPost("Open")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Open(DepositFormVM form)
        {
            var normalized = (form.Amount ?? string.Empty).Trim().Replace(" ", string.Empty).Replace(',', '.');
            if (!decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
            {
                return await ShowAsync(form, "Enter a valid amount.");
            }

            var (deposit, errors, status) = await PostAsync<DepositUIVM>("api/deposits", new
            {
                planId = form.PlanId,
                amount,
                sourceCardId = form.SourceCardId,
                payoutCardId = form.PayoutCardId,
                requestId = form.RequestId
            });
            if (status == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }
            if (deposit is null)
            {
                return await ShowAsync(form, string.Join(" ", errors));
            }

            TempData["Success"] = "Your deposit is open. You will get "
                + deposit.TotalAtMaturity.ToString("N2", CultureInfo.InvariantCulture) + " AZN on "
                + deposit.MaturesAt.ToLocalTime().ToString("dd MMM yyyy", CultureInfo.InvariantCulture) + ".";
            return RedirectToAction(nameof(Index));
        }

        private async Task<IActionResult> ShowAsync(DepositFormVM form, string error)
        {
            var (model, unauthorized) = await LoadAsync(new DepositIndexVM { Form = form, Error = error });
            return unauthorized ? await SessionExpiredAsync() : View("Index", model);
        }

        private async Task<(DepositIndexVM Model, bool Unauthorized)> LoadAsync(DepositIndexVM model)
        {
            var (overview, status) = await GetAsync<DepositsOverviewUIVM>("api/deposits");
            if (status == HttpStatusCode.Unauthorized)
            {
                return (model, true);
            }

            var (cards, _) = await GetAsync<List<CardUIVM>>("api/cards");
            model.ApiUnavailable = overview is null || cards is null;
            model.Overview = overview ?? new DepositsOverviewUIVM();
            model.Cards = cards is null ? new List<CardUIVM>() : Absolutize(cards);
            return (model, false);
        }
    }
}
