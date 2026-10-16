using CaspianBank_MVC_FinalProject.Helpers;
using CaspianBank_MVC_FinalProject.ViewModels.Cards;
using CaspianBank_MVC_FinalProject.ViewModels.Loans;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Net;
using System.Text.Json;

namespace CaspianBank_MVC_FinalProject.Controllers
{
    // Müştərinin kreditləri: müraciət, təklifə cavab, ödəniş. Bütün qaydalar API-dədir, burada yalnız forma və göstəriş var.
    // Dizayn: CaspianBank-Frontend/loan-application.html. Ödəniş uğurlu olanda PRG: qəbz ayrıca səhifədə (yeniləmə ödənişi təkrar etməsin)
    [Authorize]
    [Route("App/Loans")]
    public class LoansController : ApiControllerBase
    {
        private const string ReceiptKey = "LoanReceipt";

        public LoansController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var (model, unauthorized) = await LoadIndexAsync(new LoanApplyFormVM());
            if (unauthorized)
            {
                return await SessionExpiredAsync();
            }
            return View(model);
        }

        // Yeni müraciət: API məbləği, müddəti, məqsədi və kartı yoxlayır, burada yalnız rəqəm mətndən çevrilir
        [HttpPost("Apply")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(LoanApplyFormVM form)
        {
            if (!TryParseAmount(form.Amount, out var amount))
            {
                return await ShowIndexAsync(form, "Enter a valid amount.");
            }

            var (summary, errors, status) = await PostAsync<LoanSummaryUIVM>("api/loans", new
            {
                amount,
                purpose = form.Purpose,
                purposeNote = form.PurposeNote,
                termMonths = form.TermMonths,
                targetCardId = form.TargetCardId
            });
            if (status == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }
            if (summary is null)
            {
                return await ShowIndexAsync(form, string.Join(" ", errors));
            }

            // Qara siyahıdakı vətəndaşın müraciəti dərhal rədd olunur: müştəri ümumi mətn görür, səbəb gizli qalır
            if (summary.Status == "Rejected")
            {
                TempData["LoanError"] = summary.Message ?? "We can't approve this application right now. You can apply again later.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = "Your application " + summary.ContractNumber + " was sent. The decision will appear here.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Detail(int id)
        {
            var (loan, status) = await GetAsync<LoanDetailUIVM>($"api/loans/{id}");
            if (status == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }
            if (loan is null)
            {
                TempData["LoanError"] = status == HttpStatusCode.NotFound
                    ? "This loan was not found."
                    : "We can't load this loan right now. Please try again.";
                return RedirectToAction(nameof(Index));
            }

            var (cards, cardsStatus) = await GetAsync<List<CardUIVM>>("api/cards");
            if (cardsStatus == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }

            return View(new LoanDetailPageVM
            {
                Loan = loan,
                Cards = cards is null ? new List<CardUIVM>() : Absolutize(cards)
            });
        }

        // Azaldılmış məbləğ təklifinə razılıq: kredit aktivləşir və pul seçilən karta düşür
        [HttpPost("{id:int}/Accept")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept(int id, int? targetCardId)
        {
            var (summary, errors, status) = await PostAsync<LoanSummaryUIVM>($"api/loans/{id}/accept", new
            {
                targetCardId
            });
            if (status == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }
            if (summary is null)
            {
                TempData["LoanError"] = string.Join(" ", errors);
                return RedirectToAction(nameof(Detail), new { id });
            }

            TempData["Success"] = "The loan is active and the money was sent to your card.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        [HttpPost("{id:int}/Decline")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Decline(int id)
        {
            var (summary, errors, status) = await PostAsync<LoanSummaryUIVM>($"api/loans/{id}/decline", new { });
            if (status == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }
            if (summary is null)
            {
                TempData["LoanError"] = string.Join(" ", errors);
                return RedirectToAction(nameof(Detail), new { id });
            }

            TempData["Success"] = "You declined the offer.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        // Ödəniş: aylıq, hissəvi erkən və ya tam bağlama. Eyni RequestId təkrar gələrsə API əvvəlki qəbzi qaytarır (pul bir dəfə çıxır)
        [HttpPost("{id:int}/Pay")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(int id, int cardId, string type, string? amount, string? requestId)
        {
            decimal parsedAmount = 0m;
            if (type == "PartialEarly" && !TryParseAmount(amount, out parsedAmount))
            {
                return await ShowDetailAsync(id, "Enter a valid amount.", requestId);
            }

            var (receipt, errors, status) = await PostAsync<LoanReceiptUIVM>($"api/loans/{id}/pay", new
            {
                cardId,
                type,
                amount = parsedAmount,
                requestId
            });
            if (status == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }
            if (receipt is null)
            {
                return await ShowDetailAsync(id, string.Join(" ", errors), requestId);
            }

            receipt.LoanId = id;
            TempData[ReceiptKey] = JsonSerializer.Serialize(receipt);
            return RedirectToAction(nameof(Receipt), new { id });
        }

        [HttpGet("{id:int}/Receipt")]
        public IActionResult Receipt(int id)
        {
            if (TempData[ReceiptKey] is not string json)
            {
                return RedirectToAction(nameof(Detail), new { id });
            }

            var receipt = JsonSerializer.Deserialize<LoanReceiptUIVM>(json);
            if (receipt is null)
            {
                return RedirectToAction(nameof(Detail), new { id });
            }
            return View(receipt);
        }

        // ---------- köməkçilər ----------

        private async Task<IActionResult> ShowIndexAsync(LoanApplyFormVM form, string error)
        {
            var (model, unauthorized) = await LoadIndexAsync(form);
            if (unauthorized)
            {
                return await SessionExpiredAsync();
            }
            model.Error = error;
            return View("Index", model);
        }

        private async Task<IActionResult> ShowDetailAsync(int id, string error, string? requestId)
        {
            var (loan, status) = await GetAsync<LoanDetailUIVM>($"api/loans/{id}");
            if (status == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }
            if (loan is null)
            {
                TempData["LoanError"] = error;
                return RedirectToAction(nameof(Index));
            }

            var (cards, _) = await GetAsync<List<CardUIVM>>("api/cards");
            var page = new LoanDetailPageVM
            {
                Loan = loan,
                Cards = cards is null ? new List<CardUIVM>() : Absolutize(cards),
                Error = error
            };
            // Uğursuz cəhddən sonra eyni RequestId qalır: pul çıxıbsa təkrar göndərmə ikinci dəfə çıxmır
            if (!string.IsNullOrWhiteSpace(requestId))
            {
                page.RequestId = requestId;
            }
            return View("Detail", page);
        }

        private async Task<(LoanIndexVM Model, bool Unauthorized)> LoadIndexAsync(LoanApplyFormVM form)
        {
            var model = new LoanIndexVM { Form = form };

            var (options, optionsStatus) = await GetAsync<LoanOptionsUIVM>("api/loans/options");
            if (optionsStatus == HttpStatusCode.Unauthorized)
            {
                return (model, true);
            }
            if (options is null)
            {
                model.ApiUnavailable = true;
                return (model, false);
            }
            model.Options = options;

            var (loans, _) = await GetAsync<List<LoanSummaryUIVM>>("api/loans");
            model.Loans = loans ?? new List<LoanSummaryUIVM>();

            var (cards, _) = await GetAsync<List<CardUIVM>>("api/cards");
            model.Cards = cards is null ? new List<CardUIVM>() : Absolutize(cards);

            // Kart seçilməyibsə ilk yararlı kart əvvəlcədən seçilir
            if (model.Form.TargetCardId == 0)
            {
                var first = model.Cards.FirstOrDefault(c => LoanUi.CanUseCard(c.IsBlocked, c.Tier));
                if (first is not null)
                {
                    model.Form.TargetCardId = first.Id;
                }
            }
            if (options.TermOptions.Length > 0 && !options.TermOptions.Contains(model.Form.TermMonths))
            {
                model.Form.TermMonths = options.TermOptions[options.TermOptions.Length / 2];
            }
            return (model, false);
        }

        // "5000,5" və "5000.5" ikisi də qəbul olunur (server mədəniyyətindən asılı olmasın)
        private static bool TryParseAmount(string? raw, out decimal amount)
        {
            var normalized = (raw ?? string.Empty).Trim().Replace(" ", string.Empty).Replace(',', '.');
            return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out amount);
        }
    }
}
