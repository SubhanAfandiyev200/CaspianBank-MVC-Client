using CaspianBank_MVC_FinalProject.ViewModels.Analytics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CaspianBank_MVC_FinalProject.Controllers
{
    // Xərc analitikası: son aylarda kartlardan çıxan pul kateqoriyalar üzrə (öz kartları arasında köçürmə sayılmır). Rəqəmləri API hesablayır
    [Authorize]
    [Route("App/Spending")]
    public class SpendingController : ApiControllerBase
    {
        private static readonly int[] Options = { 3, 6, 12 };

        public SpendingController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(int months = 6)
        {
            var chosen = Options.Contains(months) ? months : 6;
            var (data, status) = await GetAsync<SpendingUIVM>("api/analytics/spending?months=" + chosen);
            if (status == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }
            return View(new SpendingPageVM { Data = data ?? new SpendingUIVM(), Months = chosen, Unavailable = data is null });
        }
    }
}
