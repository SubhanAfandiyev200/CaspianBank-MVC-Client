using CaspianBank_MVC_FinalProject.Helpers;
using CaspianBank_MVC_FinalProject.ViewModels.Cards;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CaspianBank_MVC_FinalProject.Controllers
{
    // Giriş tələb edir: login olmayan /App açsa cookie ayarındakı LoginPath-ə yönləndirilir
    [Authorize]
    public class AppController : ApiControllerBase
    {
        public AppController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        // ---------- Kartlarım ----------

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var (cards, status) = await GetAsync<List<CardUIVM>>("api/cards");
            if (status == HttpStatusCode.Unauthorized) return await SessionExpiredAsync();

            var model = new AppIndexVM
            {
                ApiUnavailable = cards is null
            };
            if (cards is not null) model.Cards = Absolutize(cards);
            return View(model);
        }

        // ---------- Kart əlavə et ----------

        [HttpGet]
        public async Task<IActionResult> AddCard()
        {
            var (cards, status) = await GetAsync<List<CardUIVM>>("api/cards");
            if (status == HttpStatusCode.Unauthorized) return await SessionExpiredAsync();

            var model = new AddCardPageVM
            {
                ApiUnavailable = cards is null
            };
            if (cards is null) return View(model);

            model.Cards = Absolutize(cards);
            model.FirstTime = cards.Count == 0;
            model.HolderName = (User.Identity?.Name ?? string.Empty).ToUpperInvariant();

            // Növlərin qaydaları: ilk dəfə də göstərilir (Cashback istisna: ayrıca seçilmir)
            var (tiers, _) = await GetAsync<List<CardTierUIVM>>("api/card-tiers");
            if (tiers is not null)
            {
                var baseUrl = Configuration["ApiSettings:BaseUrl"];
                foreach (var tier in tiers)
                    tier.DesignImage = ApiUrl.ToAbsolute(baseUrl, tier.DesignImage);

                model.Tiers = tiers.Where(t => !t.Tier.Equals("Cashback", StringComparison.OrdinalIgnoreCase)).ToList();
                model.CashbackDesign = tiers.FirstOrDefault(t => t.Tier.Equals("Cashback", StringComparison.OrdinalIgnoreCase))?.DesignImage ?? string.Empty;
                model.StandardDesign = tiers.FirstOrDefault(t => t.Tier.Equals("Standard", StringComparison.OrdinalIgnoreCase))?.DesignImage ?? string.Empty;
            }

            return View(model);
        }

        // İlk giriş: FİN daxil edilir, Standard + Cashback pulsuz açılır
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFirstCards(string? fin)
        {
            var (success, errors, status) = await PostAsync("api/cards/initial", new { fin = fin?.Trim() ?? string.Empty });
            if (status == HttpStatusCode.Unauthorized) return await SessionExpiredAsync();

            if (!success)
            {
                TempData["CardError"] = string.Join(" ", errors);
                return RedirectToAction(nameof(AddCard));
            }

            TempData["Success"] = "Your Standard and Cashback cards are ready.";
            return RedirectToAction(nameof(Index));
        }

        // Əlavə kart: haqq seçilən kartdan çıxılır
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTierCard(string? tier, int fundingCardId)
        {
            var (success, errors, status) = await PostAsync("api/cards", new { tier = tier ?? string.Empty, fundingCardId });
            if (status == HttpStatusCode.Unauthorized) return await SessionExpiredAsync();

            if (!success)
            {
                TempData["CardError"] = string.Join(" ", errors);
                return RedirectToAction(nameof(AddCard));
            }

            TempData["Success"] = $"Your {tier} card is ready.";
            return RedirectToAction(nameof(Index));
        }

        // ---------- Kartın səhifəsi ----------

        [HttpGet]
        public async Task<IActionResult> Card(int id)
        {
            var (card, status) = await GetAsync<CardDetailUIVM>($"api/cards/{id}");
            if (status == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }

            var model = new CardPageVM
            {
                Card = card,
                Unavailable = card is null && status is null
            };
            if (card is null)
            {
                return View(model);
            }

            card.DesignImage = ApiUrl.ToAbsolute(Configuration["ApiSettings:BaseUrl"], card.DesignImage);

            // Son əməliyyatlar (alınmasa kart yenə də göstərilir)
            var (transactions, _) = await GetAsync<List<TransactionUIVM>>($"api/cards/{id}/transactions?take=20");
            model.Transactions = transactions ?? new List<TransactionUIVM>();
            return View(model);
        }

        // Balansı artırmaq: pul simulyasiya olunmuş xarici kartdan gəlir. Daxil edilən kart məlumatları saxlanmır və geri göstərilmir.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TopUp(int id, string? amount, string? sourceCardNumber, string? expiry, string? cvv)
        {
            // "50,5" və "50.5" ikisi də qəbul olunur (server mədəniyyətindən asılı olmasın)
            var normalized = (amount ?? string.Empty).Trim().Replace(',', '.');
            if (!decimal.TryParse(normalized, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var value))
            {
                TempData["TopUpError"] = "Enter a valid amount.";
                return RedirectToAction(nameof(Card), new
                {
                    id
                });
            }

            var (success, errors, status) = await PostAsync($"api/cards/{id}/top-up", new
            {
                amount = value,
                sourceCardNumber = sourceCardNumber ?? string.Empty,
                expiry = expiry?.Trim() ?? string.Empty,
                cvv = cvv?.Trim() ?? string.Empty
            });
            if (status == HttpStatusCode.Unauthorized) return await SessionExpiredAsync();

            if (success)
                TempData["Success"] = $"{value.ToString("N2", System.Globalization.CultureInfo.InvariantCulture)} AZN added to your card.";
            else
                TempData["TopUpError"] = string.Join(" ", errors);   // pəncərə xəta ilə yenidən açılır

            return RedirectToAction(nameof(Card), new
            {
                id
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BlockCard(int id, string? email, string? password)
        {
            var (success, errors, status) = await PostAsync($"api/cards/{id}/block",
                new
                {
                    email = email?.Trim() ?? string.Empty,
                    password = password ?? string.Empty
                });
            if (status == HttpStatusCode.Unauthorized) return await SessionExpiredAsync();

            if (success)
            {
                TempData["Success"] = "The card is blocked.";
            }
            else
            {
                TempData["BlockError"] = string.Join(" ", errors);   // pəncərə xəta ilə yenidən açılır
            }

            return RedirectToAction(nameof(Card), new
            {
                id
            });
        }

    }
}
