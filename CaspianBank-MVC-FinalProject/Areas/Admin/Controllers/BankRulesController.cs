using CaspianBank_MVC_FinalProject.Controllers;
using CaspianBank_MVC_FinalProject.Helpers;
using CaspianBank_MVC_FinalProject.ViewModels.Bank;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Net;

namespace CaspianBank_MVC_FinalProject.Areas.Admin.Controllers
{
    // BANK bölməsi: bankın qaydaları (rezerv, vergi, əməliyyat xərci, banklararası limit, kredit qaydaları). Maliyyə qərarıdır:
    // SuperAdmin, Admin və Accountant. Aralıqlar və yoxlamalar API-dədir; hər dəyişiklik audit-ə yazılır
    [Area("Admin")]
    [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.Admin + "," + AppRoles.Accountant)]
    public class BankRulesController : ApiControllerBase
    {
        public BankRulesController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var (rules, status) = await GetAsync<BankRulesApiVM>("api/admin/bank-rules");
            if (status == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }
            return View(rules is null ? new BankRulesVM { Unavailable = true } : ToForm(rules));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(BankRulesVM form)
        {
            if (!TryNumber(form.ReservePercent, out var reserve) || !TryNumber(form.ProfitTaxPercent, out var tax)
                || !TryNumber(form.OperatingCostMonthly, out var cost) || !TryNumber(form.MaxInterbankAmount, out var interbank)
                || !TryNumber(form.MinLoanAmount, out var minLoan) || !TryNumber(form.MaxLoanAmount, out var maxLoan)
                || !TryNumber(form.AnnualInterestRate, out var rate) || !TryNumber(form.FixedPenalty, out var fixedPenalty)
                || !TryNumber(form.DailyPenaltyPercent, out var dailyPenalty)
                || !int.TryParse(form.MaxActiveLoans?.Trim(), out var maxActive) || !int.TryParse(form.OfferValidDays?.Trim(), out var offerDays))
            {
                form.Error = "Enter a valid number in every field.";
                return View(form);
            }

            var (success, errors, status) = await PutAsync("api/admin/bank-rules", new
            {
                reservePercent = reserve,
                profitTaxPercent = tax,
                operatingCostMonthly = cost,
                maxInterbankAmount = interbank,
                minLoanAmount = minLoan,
                maxLoanAmount = maxLoan,
                annualInterestRate = rate,
                fixedPenalty,
                dailyPenaltyPercent = dailyPenalty,
                maxActiveLoans = maxActive,
                offerValidDays = offerDays
            });
            if (status == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }
            if (!success)
            {
                form.Error = string.Join(" ", errors);
                return View(form);
            }

            TempData["Success"] = "The bank rules were saved.";
            return RedirectToAction(nameof(Index));
        }

        private static bool TryNumber(string? raw, out decimal value)
        {
            var normalized = (raw ?? string.Empty).Trim().Replace(" ", string.Empty).Replace(',', '.');
            return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
        }

        private static string Text(decimal value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }

        private static BankRulesVM ToForm(BankRulesApiVM rules)
        {
            return new BankRulesVM
            {
                ReservePercent = Text(rules.ReservePercent),
                ProfitTaxPercent = Text(rules.ProfitTaxPercent),
                OperatingCostMonthly = Text(rules.OperatingCostMonthly),
                MaxInterbankAmount = Text(rules.MaxInterbankAmount),
                MinLoanAmount = Text(rules.MinLoanAmount),
                MaxLoanAmount = Text(rules.MaxLoanAmount),
                AnnualInterestRate = Text(rules.AnnualInterestRate),
                FixedPenalty = Text(rules.FixedPenalty),
                DailyPenaltyPercent = Text(rules.DailyPenaltyPercent),
                MaxActiveLoans = rules.MaxActiveLoans.ToString(CultureInfo.InvariantCulture),
                OfferValidDays = rules.OfferValidDays.ToString(CultureInfo.InvariantCulture)
            };
        }
    }
}
