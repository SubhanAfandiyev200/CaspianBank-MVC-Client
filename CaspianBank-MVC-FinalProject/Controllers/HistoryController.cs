using CaspianBank_MVC_FinalProject.ViewModels.Cards;
using CaspianBank_MVC_FinalProject.ViewModels.History;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Net;

namespace CaspianBank_MVC_FinalProject.Controllers
{
    // Müştərinin əməliyyat tarixçəsi (bütün kartlar) və kart çıxarışı. Məlumat API-dən gəlir: api/history
    [Authorize]
    [Route("App/History")]
    public class HistoryController : ApiControllerBase
    {
        private const int PageSize = 20;

        public HistoryController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        [HttpGet("")]
        public async Task<IActionResult> Index([FromQuery] HistoryFilterVM filter)
        {
            var model = new HistoryIndexVM
            {
                Filter = filter
            };

            // Kartlar: filtrdəki kart seçimi və çıxarış üçün
            var (cards, cardsStatus) = await GetAsync<List<CardUIVM>>("api/cards");
            if (cardsStatus == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }
            if (cards is null)
            {
                model.ApiUnavailable = true;
                return View(model);
            }
            model.Cards = Absolutize(cards);

            // Səhv tarix aralığını API-yə göndərmirik (GetAsync xəta mətnini qaytarmır): xəbərdarlıq verib tarixləri atırıq
            if (filter.From.HasValue && filter.To.HasValue && filter.From.Value.Date > filter.To.Value.Date)
            {
                model.Error = "The start date must not be after the end date. The dates were ignored.";
                filter.From = null;
                filter.To = null;
            }

            var (result, status) = await GetAsync<HistoryPageUIVM>("api/history" + BuildQuery(filter));
            if (status == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }
            if (result is null)
            {
                // API işləyir, amma filtri qəbul etmədi (məsələn kart tapılmadı): filtrsiz boş nəticə və izah
                model.Error = status is null ? null : "These filters could not be applied. Reset them and try again.";
                model.ApiUnavailable = status is null;
                return View(model);
            }

            // API mövcud olmayan səhifə üçün son səhifəni qaytarır: formadakı nömrə də ona uyğun olsun
            filter.Page = result.Page;
            model.Result = result;
            return View(model);
        }

        // Çıxarış: bir kart, bir tarix aralığı. Çap olunmaq üçün hazırlanıb ("Save as PDF")
        [HttpGet("Statement")]
        public async Task<IActionResult> Statement(int? cardId, DateTime? from, DateTime? to)
        {
            if (cardId is null || from is null || to is null)
            {
                TempData["CardError"] = "Choose a card and both dates to open a statement.";
                return RedirectToAction(nameof(Index));
            }

            if (from.Value.Date > to.Value.Date)
            {
                TempData["CardError"] = "The start date must not be after the end date.";
                return RedirectToAction(nameof(Index), new { cardId });
            }

            var query = new QueryBuilder
            {
                { "cardId", cardId.Value.ToString(CultureInfo.InvariantCulture) },
                { "from", from.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) },
                { "to", to.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) }
            };
            var (statement, status) = await GetAsync<StatementUIVM>("api/history/statement" + query.ToQueryString());
            if (status == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }
            if (statement is null)
            {
                TempData["CardError"] = status == HttpStatusCode.NotFound
                    ? "That card was not found."
                    : "The statement could not be created. Check the dates (at most one year) and try again.";
                return RedirectToAction(nameof(Index));
            }

            await FillBankInfoAsync(statement);
            return View(statement);
        }

        // API-yə gedən sorğu: yalnız dolu filtrlər (boşlar göndərilmir)
        private static string BuildQuery(HistoryFilterVM filter)
        {
            var query = new QueryBuilder();
            if (filter.CardId.HasValue)
            {
                query.Add("cardId", filter.CardId.Value.ToString(CultureInfo.InvariantCulture));
            }
            if (!string.IsNullOrWhiteSpace(filter.Type))
            {
                query.Add("type", filter.Type);
            }
            if (!string.IsNullOrWhiteSpace(filter.Direction))
            {
                query.Add("direction", filter.Direction);
            }
            if (filter.From.HasValue)
            {
                query.Add("from", filter.From.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            }
            if (filter.To.HasValue)
            {
                query.Add("to", filter.To.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            }
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                query.Add("search", filter.Search.Trim());
            }
            query.Add("page", Math.Max(1, filter.Page).ToString(CultureInfo.InvariantCulture));
            query.Add("pageSize", PageSize.ToString(CultureInfo.InvariantCulture));
            return query.ToQueryString().Value ?? string.Empty;
        }

        // Çıxarışın başlığındakı bank adı və ünvan Settings-dən gəlir (alınmasa ehtiyat mətn qalır)
        private async Task FillBankInfoAsync(StatementUIVM statement)
        {
            try
            {
                var settings = await Api.GetFromJsonAsync<Dictionary<string, string>>("api/settings");
                if (settings is not null)
                {
                    if (settings.TryGetValue("CompanyName", out var name) && !string.IsNullOrWhiteSpace(name))
                    {
                        statement.CompanyName = name.Trim();
                    }
                    if (settings.TryGetValue("Address", out var address) && !string.IsNullOrWhiteSpace(address))
                    {
                        statement.Address = address.Trim();
                    }
                }
            }
            catch (HttpRequestException)
            {
                // API-dən ayarlar gəlmədi: çıxarış yenə də göstərilir
            }
        }
    }
}
