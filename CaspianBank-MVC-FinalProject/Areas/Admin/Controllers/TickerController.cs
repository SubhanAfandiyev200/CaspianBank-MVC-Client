using CaspianBank_MVC_FinalProject.Controllers;
using CaspianBank_MVC_FinalProject.ViewModels.HomeTickers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CaspianBank_MVC_FinalProject.Areas.Admin.Controllers
{
    // UI bölməsi: Home-dakı hərəkət edən zolaq (ticker). API: api/admin/tickers
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class TickerController : ApiControllerBase
    {
        public TickerController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var (tickers, status) = await GetAsync<List<HomeTickerVM>>("api/admin/tickers");
            return View(tickers ?? new List<HomeTickerVM>());
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var (ticker, status) = await GetAsync<HomeTickerDetailVM>($"api/admin/tickers/{id}");
            if (ticker is null)
            {
                return NotFound();
            }
            return View(ticker);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new HomeTickerCreateVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HomeTickerCreateVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (success, errors, status) = await PostAsync("api/admin/tickers", new { text = model.Text.Trim() });
            if (!success)
            {
                if (status == HttpStatusCode.Unauthorized)
                {
                    return await SessionExpiredAsync();
                }

                foreach (var error in errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                return View(model);
            }

            TempData["Success"] = "The ticker text was added.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var (ticker, status) = await GetAsync<HomeTickerVM>($"api/admin/tickers/{id}");
            if (ticker is null)
            {
                return NotFound();
            }

            return View(new HomeTickerEditVM
            {
                Id = ticker.Id,
                Text = ticker.Text.Trim()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, HomeTickerEditVM model)
        {
            model.Id = id;
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (success, errors, status) = await PutAsync($"api/admin/tickers/{id}", new { text = model.Text.Trim() });
            if (!success)
            {
                if (status == HttpStatusCode.Unauthorized)
                {
                    return await SessionExpiredAsync();
                }

                if (status == HttpStatusCode.NotFound)
                {
                    TempData["Error"] = "This ticker text no longer exists.";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                return View(model);
            }

            TempData["Success"] = "The ticker text was updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, errors, status) = await DeleteAsync($"api/admin/tickers/{id}");
            if (!success)
            {
                if (status == HttpStatusCode.Unauthorized)
                {
                    return await SessionExpiredAsync();
                }

                // Artıq silinibsə (başqa tabda) sadəcə siyahıya qayıdırıq
                TempData["Error"] = status == HttpStatusCode.NotFound ? "This ticker text no longer exists." : errors.FirstOrDefault();
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = "The ticker text was deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
