using CaspianBank_MVC_FinalProject.Helpers;
using CaspianBank_MVC_FinalProject.ViewModels.Bills;
using CaspianBank_MVC_FinalProject.ViewModels.Cards;
using CaspianBank_MVC_FinalProject.ViewModels.Loans;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Net;
using System.Text.Json;

namespace CaspianBank_MVC_FinalProject.Controllers
{
    // Ödənişlər: mobil, işıq, qaz, su, internet və Caspian krediti. Dizayn: CaspianBank-Frontend/bill-payment.html.
    // Bütün qaydalar (operator, abunəçi, məbləğ, cashback) API-dədir; burada yalnız forma, axtarış cavabı və qəbz var.
    // Axtarış JS ilə (POST App/Bills/Lookup, JSON), ödəniş isə adi form + PRG: yeniləmə ödənişi təkrar etməsin
    [Authorize]
    [Route("App/Bills")]
    public class BillsController : ApiControllerBase
    {
        private const string ReceiptKey = "BillReceipt";

        // API-də kredit təchizatçı deyil (api/loans/lookup və pay-by-code), ona görə siyahıya -1 nömrəli xüsusi seçim kimi əlavə olunur
        // (0 formada "heç nə seçilməyib" deməkdir)
        private const int LoanProviderId = -1;

        public BillsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var (model, unauthorized) = await LoadPageAsync(new BillPayFormVM());
            if (unauthorized)
            {
                return await SessionExpiredAsync();
            }
            return View(model);
        }

        // Nömrə və ya kod yazılandan sonra "Check": kimin hesabıdır və nə qədər borcu var. Pul hərəkət etmir
        [HttpPost("Lookup")]
        [ValidateAntiForgeryToken]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Lookup(int providerId, string? identifier)
        {
            var (lookup, errors, status) = await FindAsync(providerId, identifier);
            if (status == HttpStatusCode.Unauthorized)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return Unauthorized();
            }
            if (lookup is null)
            {
                return Json(new { ok = false, error = string.Join(" ", errors) });
            }
            return Json(new { ok = true, lookup });
        }

        // Ödəniş: eyni RequestId təkrar gələrsə API əvvəlki qəbzi qaytarır (pul bir dəfə çıxır)
        [HttpPost("Pay")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(BillPayFormVM form)
        {
            BillReceiptUIVM? receipt;
            string[] errors;
            HttpStatusCode? status;
            var isLoan = form.ProviderId == LoanProviderId;
            var recurringAmount = 0m;

            if (isLoan)
            {
                // Kredit kodu ilə: cari aylıq ödəniş. Məbləğ boşdursa hamısı ödənilir, yazılıbsa hissə-hissə (API yoxlayır)
                decimal loanAmount = 0m;
                if (!string.IsNullOrWhiteSpace(form.Amount) && !TryParseAmount(form.Amount, out loanAmount))
                {
                    return await ShowPageAsync(form, "Enter a valid amount.");
                }
                var (loanReceipt, loanErrors, loanStatus) = await PostAsync<LoanReceiptUIVM>("api/loans/pay-by-code", new
                {
                    contractNumber = (form.Identifier ?? string.Empty).Trim().ToUpperInvariant(),
                    cardId = form.CardId,
                    amount = loanAmount,
                    requestId = form.RequestId
                });
                receipt = loanReceipt is null ? null : FromLoanReceipt(loanReceipt);
                if (receipt is not null)
                {
                    receipt.LogoUrl = (await SiteSettingsLoader.LoadAsync(HttpContext, HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>(), Configuration)).Logo;
                }
                errors = loanErrors;
                status = loanStatus;
            }
            else
            {
                if (!TryParseAmount(form.Amount, out var amount))
                {
                    return await ShowPageAsync(form, "Enter a valid amount.");
                }
                recurringAmount = amount;

                var (billReceipt, billErrors, billStatus) = await PostAsync<BillReceiptUIVM>("api/bills/pay", new
                {
                    providerId = form.ProviderId,
                    identifier = form.Identifier,
                    amount,
                    cardId = form.CardId,
                    requestId = form.RequestId
                });
                receipt = billReceipt is null ? null : AbsolutizeReceipt(billReceipt);
                errors = billErrors;
                status = billStatus;
            }

            if (status == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }
            if (receipt is null)
            {
                return await ShowPageAsync(form, string.Join(" ", errors));
            }

            // "Hər ay təkrarla": ödəniş artıq keçib, təkrar qeydi alınmasa da pul geri qayıtmır, qəbzdə səbəb yazılır
            if (form.Recurring)
            {
                await SaveRecurringAsync(form, isLoan, recurringAmount, receipt);
            }

            TempData[ReceiptKey] = JsonSerializer.Serialize(receipt);
            return RedirectToAction(nameof(Receipt));
        }

        // Aylıq ödənişi dayandırır (pul çıxmır, qeyd qalır)
        [HttpPost("Recurring/{id:int}/Pause")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PauseRecurring(int id)
        {
            return await ChangeRecurringAsync(id, "pause", "The monthly payment was paused.");
        }

        [HttpPost("Recurring/{id:int}/Resume")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResumeRecurring(int id)
        {
            return await ChangeRecurringAsync(id, "resume", "The monthly payment is on again.");
        }

        [HttpPost("Recurring/{id:int}/Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRecurring(int id)
        {
            var (success, errors, status) = await DeleteAsync($"api/bills/recurring/{id}");
            if (status == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }

            if (success)
            {
                TempData["Success"] = "The monthly payment was removed.";
            }
            else
            {
                TempData["BillError"] = status == HttpStatusCode.NotFound ? "This monthly payment no longer exists." : string.Join(" ", errors);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("Receipt")]
        public IActionResult Receipt()
        {
            if (TempData[ReceiptKey] is not string json)
            {
                return RedirectToAction(nameof(Index));
            }

            var receipt = JsonSerializer.Deserialize<BillReceiptUIVM>(json);
            if (receipt is null)
            {
                return RedirectToAction(nameof(Index));
            }
            return View(receipt);
        }

        // ---------- köməkçilər ----------

        private async Task<IActionResult> ChangeRecurringAsync(int id, string action, string successMessage)
        {
            var (saved, errors, status) = await PostAsync<RecurringPaymentUIVM>($"api/bills/recurring/{id}/{action}", new { });
            if (status == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }

            if (saved is not null)
            {
                TempData["Success"] = successMessage;
            }
            else
            {
                TempData["BillError"] = status == HttpStatusCode.NotFound ? "This monthly payment no longer exists." : string.Join(" ", errors);
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task SaveRecurringAsync(BillPayFormVM form, bool isLoan, decimal amount, BillReceiptUIVM receipt)
        {
            var identifier = (form.Identifier ?? string.Empty).Trim();
            var (saved, errors, _) = await PostAsync<RecurringPaymentUIVM>("api/bills/recurring", new
            {
                providerId = isLoan ? (int?)null : form.ProviderId,
                identifier = isLoan ? identifier.ToUpperInvariant() : identifier,
                amount = isLoan ? 0m : amount,
                cardId = form.CardId
            });

            if (saved is not null)
            {
                receipt.RecurringNote = "Saved as a monthly payment. The next one is on "
                    + saved.NextRunDate.ToString("d MMM yyyy", CultureInfo.InvariantCulture) + ".";
            }
            else
            {
                receipt.RecurringFailed = true;
                receipt.RecurringNote = "The payment went through, but we could not save it as a monthly payment. " + string.Join(" ", errors);
            }
        }

        // Uğursuz ödənişdən sonra səhifə yenidən açılır: seçilmiş təchizatçı, yazılanlar və axtarış nəticəsi qalır
        private async Task<IActionResult> ShowPageAsync(BillPayFormVM form, string error)
        {
            var (model, unauthorized) = await LoadPageAsync(form);
            if (unauthorized)
            {
                return await SessionExpiredAsync();
            }

            model.Error = error;
            if (!string.IsNullOrWhiteSpace(form.Identifier))
            {
                var (lookup, _, _) = await FindAsync(form.ProviderId, form.Identifier);
                model.Lookup = lookup;
            }
            return View("Index", model);
        }

        private async Task<(BillsPageVM Model, bool Unauthorized)> LoadPageAsync(BillPayFormVM form)
        {
            var model = new BillsPageVM { Form = form };

            var (providers, providersStatus) = await GetAsync<List<BillProviderUIVM>>("api/bills/providers");
            if (providersStatus == HttpStatusCode.Unauthorized)
            {
                return (model, true);
            }
            if (providers is null)
            {
                model.ApiUnavailable = true;
                return (model, false);
            }

            var baseUrl = Configuration["ApiSettings:BaseUrl"];
            foreach (var provider in providers)
            {
                provider.LogoUrl = ApiUrl.ToAbsolute(baseUrl, provider.LogoUrl);
            }
            var siteLogo = (await SiteSettingsLoader.LoadAsync(HttpContext, HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>(), Configuration)).Logo;
            providers.Add(new BillProviderUIVM
            {
                Id = LoanProviderId,
                Code = "loan",
                Name = "Caspian loan",
                LogoUrl = siteLogo,
                Category = "Loan",
                IdentifierLabel = "Loan code",
                IdentifierHint = "KR-2026-000123",
                IdentifierLength = 14
            });
            model.Providers = providers;

            var (overview, _) = await GetAsync<BillsOverviewUIVM>("api/bills/overview");
            model.Overview = overview ?? new BillsOverviewUIVM();
            foreach (var payment in model.Overview.RecentPayments)
            {
                payment.LogoUrl = ApiUrl.ToAbsolute(baseUrl, payment.LogoUrl);
            }

            var (recurring, _) = await GetAsync<List<RecurringPaymentUIVM>>("api/bills/recurring");
            model.Recurring = recurring ?? new List<RecurringPaymentUIVM>();
            foreach (var item in model.Recurring)
            {
                item.LogoUrl = item.Category == "Loan" ? siteLogo : ApiUrl.ToAbsolute(baseUrl, item.LogoUrl);
            }

            var (cards, _) = await GetAsync<List<CardUIVM>>("api/cards");
            model.Cards = cards is null ? new List<CardUIVM>() : Absolutize(cards);

            // Kart seçilməyibsə pulu çatan ilk kart, yoxdursa ilk blok olmayan kart əvvəlcədən seçilir
            if (model.Form.CardId == 0)
            {
                var usable = model.Cards.Where(c => !c.IsBlocked).ToList();
                var first = usable.FirstOrDefault(c => !c.Tier.Equals("Cashback", StringComparison.OrdinalIgnoreCase) && c.Balance > 0m)
                            ?? usable.FirstOrDefault();
                if (first is not null)
                {
                    model.Form.CardId = first.Id;
                }
            }
            return (model, false);
        }

        // Axtarış: adi təchizatçı API-nin api/bills/lookup-u, kredit kodu isə api/loans/lookup/{code}
        private async Task<(BillLookupUIVM? Lookup, string[] Errors, HttpStatusCode? Status)> FindAsync(int providerId, string? identifier)
        {
            var text = (identifier ?? string.Empty).Trim();
            if (text.Length == 0)
            {
                return (null, new[] { providerId == LoanProviderId ? "Enter the loan code." : "Enter the number or subscriber code." }, null);
            }

            var baseUrl = Configuration["ApiSettings:BaseUrl"];

            if (providerId == LoanProviderId)
            {
                var (loan, loanErrors, loanStatus) = await GetWithErrorsAsync<LoanLookupUIVM>("api/loans/lookup/" + Uri.EscapeDataString(text));
                if (loan is null)
                {
                    // API açıq olmayan kredit üçün boş "tapılmadı" qaytarır: kodun varlığı bilinməsin
                    var message = loanStatus == HttpStatusCode.NotFound
                        ? "We couldn't find an active loan with this code."
                        : string.Join(" ", loanErrors);
                    return (null, new[] { message }, loanStatus);
                }
                if (!loan.CanPay)
                {
                    return (null, new[] { "There is nothing to pay on this loan right now." }, loanStatus);
                }

                return (new BillLookupUIVM
                {
                    ProviderId = LoanProviderId,
                    ProviderName = "Caspian loan",
                    Category = "Loan",
                    Identifier = loan.ContractNumber,
                    SubscriberName = loan.OwnerName,
                    Debt = loan.DueTotal,
                    SuggestedAmount = loan.DueTotal,
                    MinPayment = loan.DueTotal,
                    MaxPayment = loan.DueTotal,
                    FixedAmount = true
                }, Array.Empty<string>(), loanStatus);
            }

            var (lookup, errors, status) = await PostAsync<BillLookupUIVM>("api/bills/lookup", new
            {
                providerId,
                identifier = text
            });
            if (lookup is not null)
            {
                lookup.LogoUrl = ApiUrl.ToAbsolute(baseUrl, lookup.LogoUrl);
            }
            return (lookup, errors, status);
        }

        private BillReceiptUIVM AbsolutizeReceipt(BillReceiptUIVM receipt)
        {
            receipt.LogoUrl = ApiUrl.ToAbsolute(Configuration["ApiSettings:BaseUrl"], receipt.LogoUrl);
            return receipt;
        }

        // Kredit qəbzi eyni qəbz səhifəsində göstərilir (aylıq ödənişin tərkibi ilə)
        private static BillReceiptUIVM FromLoanReceipt(LoanReceiptUIVM loan)
        {
            return new BillReceiptUIVM
            {
                Reference = loan.Reference,
                ProviderName = "Caspian loan",
                Category = "Loan",
                Identifier = loan.ContractNumber,
                Amount = loan.Amount,
                DebtCleared = loan.Amount,
                CardLabel = loan.CardLabel,
                CardBalanceAfter = loan.CardBalanceAfter,
                PaidAt = loan.PaidAt,
                Principal = loan.Principal,
                Interest = loan.Interest,
                Penalty = loan.Penalty
            };
        }

        // "12,5" və "12.5" ikisi də qəbul olunur (server mədəniyyətindən asılı olmasın)
        private static bool TryParseAmount(string? raw, out decimal amount)
        {
            var normalized = (raw ?? string.Empty).Trim().Replace(" ", string.Empty).Replace(',', '.');
            return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out amount);
        }
    }
}
