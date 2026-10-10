using CaspianBank_MVC_FinalProject.Helpers;
using CaspianBank_MVC_FinalProject.ViewModels.Cards;
using CaspianBank_MVC_FinalProject.ViewModels.Transfers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Net;
using System.Text.Json;

namespace CaspianBank_MVC_FinalProject.Controllers
{
    // Köçürmə: forma → yoxla (pul hələ köçmür) → təsdiq et → qəbz
    [Authorize]
    [Route("App/Transfer")]
    public class TransferController : ApiControllerBase
    {
        private const string ReceiptKey = "TransferReceipt";

        public TransferController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        // from: kart səhifəsindəki "Transfer" düyməsi göndərən kartı əvvəlcədən seçir
        [HttpGet("")]
        public async Task<IActionResult> Index(int? from)
        {
            var (model, unauthorized) = await LoadFormAsync(new TransferFormVM
            {
                FromCardId = from ?? 0
            });
            return unauthorized ? await SessionExpiredAsync() : View(model);
        }

        // 1-ci addım: API yoxlayır (qaydalar, komissiya, alıcının gizlədilmiş adı), pul köçürülmür
        [HttpPost("Review")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Review(TransferFormVM form)
        {
            var (data, errors, status) = await PostAsync<TransferPreviewUIVM>("api/transfers/preview", ToRequest(form, out var amountError));
            if (status == HttpStatusCode.Unauthorized) return await SessionExpiredAsync();

            if (data is null)
                return await ShowFormAsync(form, amountError ?? string.Join(" ", errors));

            // Yoxlama səhifəsi kartları göstərmir: forma sahələri gizli saxlanılır
            return View("Review", new TransferReviewVM { Preview = data, Form = form });
        }

        // "Geri": yoxlama səhifəsindən formaya qayıdır, daxil edilənlər qalır
        [HttpPost("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TransferFormVM form)
            => await ShowFormAsync(form, null);

        // 2-ci addım: pul köçürülür. Eyni RequestId təkrar gələrsə API əvvəlki qəbzi qaytarır (pul bir dəfə köçür)
        [HttpPost("Confirm")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(TransferFormVM form)
        {
            var (data, errors, status) = await PostAsync<TransferReceiptUIVM>("api/transfers", ToRequest(form, out var amountError));
            if (status == HttpStatusCode.Unauthorized) return await SessionExpiredAsync();

            if (data is null)
                return await ShowFormAsync(form, amountError ?? string.Join(" ", errors));

            // PRG: səhifəni yeniləmək köçürməni təkrar göndərməsin
            TempData[ReceiptKey] = JsonSerializer.Serialize(data);
            return RedirectToAction(nameof(Done));
        }

        [HttpGet("Done")]
        public IActionResult Done()
        {
            if (TempData[ReceiptKey] is not string json) return RedirectToAction(nameof(Index));

            var receipt = JsonSerializer.Deserialize<TransferReceiptUIVM>(json);
            return receipt is null ? RedirectToAction(nameof(Index)) : View(receipt);
        }

        // ---------- köməkçilər ----------

        private async Task<IActionResult> ShowFormAsync(TransferFormVM form, string? error)
        {
            form.Error = error;
            var (model, unauthorized) = await LoadFormAsync(form);
            return unauthorized ? await SessionExpiredAsync() : View("Index", model);
        }

        // Kartlar və növ qaydaları (canlı komissiya göstəricisi üçün) formaya doldurulur
        private async Task<(TransferFormVM Model, bool Unauthorized)> LoadFormAsync(TransferFormVM form)
        {
            var (cards, status) = await GetAsync<List<CardUIVM>>("api/cards");
            if (status == HttpStatusCode.Unauthorized) return (form, true);

            form.ApiUnavailable = cards is null;
            form.Cards = cards is null ? new List<CardUIVM>() : Absolutize(cards);

            var (tiers, _) = await GetAsync<List<CardTierUIVM>>("api/card-tiers");
            form.Tiers = tiers ?? new List<CardTierUIVM>();
            return (form, false);
        }

        // "50,5" və "50.5" ikisi də qəbul olunur (server mədəniyyətindən asılı olmasın)
        private static object ToRequest(TransferFormVM form, out string? amountError)
        {
            amountError = null;
            var normalized = (form.Amount ?? string.Empty).Trim().Replace(',', '.');
            if (!decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
            {
                amountError = "Enter a valid amount.";
                amount = 0;
            }

            var own = form.Mode != "other";
            return new
            {
                fromCardId = form.FromCardId,
                toCardId = own ? form.ToCardId : null,
                toCardNumber = own ? null : form.ToCardNumber,
                amount,
                note = form.Note,
                requestId = form.RequestId
            };
        }
    }
}
