using CaspianBank_MVC_FinalProject.Controllers;
using CaspianBank_MVC_FinalProject.ViewModels.CardDesigns;
using CaspianBank_MVC_FinalProject.ViewModels.CardTiers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Net;

namespace CaspianBank_MVC_FinalProject.Areas.Admin.Controllers
{
    // UI bölməsi: kart növlərinin qaydaları (açılış haqqı, cashback, limit, komissiya, dizayn). Dörd növ kodda sabitdir: yalnız görünür və dəyişdirilir (əlavə/silmə yoxdur).
    // Pul qaydalarını Accountant, Admin və SuperAdmin dəyişə bilər. API: api/admin/card-tiers
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin,Accountant")]
    public class CardTierController : ApiControllerBase
    {
        public CardTierController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var (tiers, status) = await GetAsync<List<CardTierVM>>("api/admin/card-tiers");
            return View(tiers ?? new List<CardTierVM>());
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var (tier, status) = await GetAsync<CardTierDetailVM>($"api/admin/card-tiers/{id}");
            if (tier is null)
            {
                return NotFound();
            }
            return View(tier);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var (tier, status) = await GetAsync<CardTierDetailVM>($"api/admin/card-tiers/{id}");
            if (tier is null)
            {
                return NotFound();
            }

            var model = new CardTierEditVM
            {
                Id = tier.Id,
                IssueFee = Format(tier.IssueFee),
                CashbackPercent = Format(tier.CashbackPercent),
                TransferLimit = Format(tier.TransferLimit),
                CommissionPercent = Format(tier.CommissionPercent),
                CardDesignId = tier.CardDesignId
            };
            await FillDisplayAsync(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CardTierEditVM model)
        {
            model.Id = id;

            // Mətn kimi gələn rəqəmlər çevrilir; aralıq və onluq yoxlaması API-dədir
            var issueFee = ParseAmount(model, nameof(model.IssueFee), model.IssueFee);
            var cashback = ParseAmount(model, nameof(model.CashbackPercent), model.CashbackPercent);
            var limit = ParseAmount(model, nameof(model.TransferLimit), model.TransferLimit);
            var commission = ParseAmount(model, nameof(model.CommissionPercent), model.CommissionPercent);

            if (!ModelState.IsValid)
            {
                await FillDisplayAsync(model);
                return View(model);
            }

            var (success, errors, status) = await PutAsync($"api/admin/card-tiers/{id}", new
            {
                issueFee,
                cashbackPercent = cashback,
                transferLimit = limit,
                commissionPercent = commission,
                cardDesignId = model.CardDesignId
            });
            if (!success)
            {
                if (status == HttpStatusCode.Unauthorized)
                {
                    return await SessionExpiredAsync();
                }

                if (status == HttpStatusCode.NotFound)
                {
                    TempData["Error"] = "This card type no longer exists.";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                await FillDisplayAsync(model);
                return View(model);
            }

            TempData["Success"] = "The card type was updated.";
            return RedirectToAction(nameof(Index));
        }

        // Formada göstərilən, amma göndərilməyən məlumat: növün adı, cari dizayn və seçim siyahısı (API-dən alınır)
        private async Task FillDisplayAsync(CardTierEditVM model)
        {
            var (current, status) = await GetAsync<CardTierDetailVM>($"api/admin/card-tiers/{model.Id}");
            model.Tier = current?.Tier ?? string.Empty;
            model.CurrentDesignTitle = current?.DesignTitle ?? string.Empty;
            model.CurrentDesignImage = current?.DesignImage ?? string.Empty;

            var (designs, designsStatus) = await GetAsync<List<CardDesignVM>>("api/admin/card-tiers/designs");
            model.Designs = designs ?? new List<CardDesignVM>();
        }

        // "10,5" və "10.5" ikisi də qəbul olunur. Çevrilmirsə həmin sahəyə xəta yazılır
        private decimal? ParseAmount(CardTierEditVM model, string field, string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return null;
            }

            var normalized = raw.Trim().Replace(',', '.');
            if (!decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
            {
                ModelState.AddModelError(field, "Enter a valid number.");
                return null;
            }
            return value;
        }

        private static string Format(decimal value)
        {
            return value.ToString("0.00", CultureInfo.InvariantCulture);
        }
    }
}
