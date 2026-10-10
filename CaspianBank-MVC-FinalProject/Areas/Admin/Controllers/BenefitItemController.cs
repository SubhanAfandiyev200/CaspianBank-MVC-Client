using CaspianBank_MVC_FinalProject.Controllers;
using CaspianBank_MVC_FinalProject.ViewModels.BenefitItems;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CaspianBank_MVC_FinalProject.Areas.Admin.Controllers
{
    // UI bölməsi: Home-dakı "Benefits" blokunun sürüşən kartları. Hamısı tək başlıq bölməsinə aiddir (BenefitSectionController).
    // API: api/admin/benefit-items
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class BenefitItemController : ApiControllerBase
    {
        public BenefitItemController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var (items, status) = await GetAsync<List<BenefitItemVM>>("api/admin/benefit-items");
            return View(items ?? new List<BenefitItemVM>());
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var (item, status) = await GetAsync<BenefitItemDetailVM>($"api/admin/benefit-items/{id}");
            if (item is null)
            {
                return NotFound();
            }
            return View(item);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View(new BenefitItemCreateVM
            {
                ButtonUrl = "/Account/Welcome",
                Destinations = await LoadDestinationsAsync()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BenefitItemCreateVM model)
        {
            if (!ModelState.IsValid)
            {
                model.Destinations = await LoadDestinationsAsync();
                return View(model);
            }

            var (success, errors, status) = await PostAsync("api/admin/benefit-items", ToBody(model.Label, model.Title, model.Description,
                model.ButtonText, model.ButtonUrl, model.Text1, model.Text2, model.Text3));
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
                model.Destinations = await LoadDestinationsAsync();
                return View(model);
            }

            TempData["Success"] = "The card was added.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var (item, status) = await GetAsync<BenefitItemVM>($"api/admin/benefit-items/{id}");
            if (item is null)
            {
                return NotFound();
            }

            return View(new BenefitItemEditVM
            {
                Id = item.Id,
                Label = item.Label,
                Title = item.Title,
                Description = item.Description,
                ButtonText = item.ButtonText,
                ButtonUrl = item.ButtonUrl,
                Text1 = item.Text1,
                Text2 = item.Text2,
                Text3 = item.Text3,
                Destinations = await LoadDestinationsAsync()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BenefitItemEditVM model)
        {
            model.Id = id;
            if (!ModelState.IsValid)
            {
                model.Destinations = await LoadDestinationsAsync();
                return View(model);
            }

            var (success, errors, status) = await PutAsync($"api/admin/benefit-items/{id}", ToBody(model.Label, model.Title, model.Description,
                model.ButtonText, model.ButtonUrl, model.Text1, model.Text2, model.Text3));
            if (!success)
            {
                if (status == HttpStatusCode.Unauthorized)
                {
                    return await SessionExpiredAsync();
                }

                if (status == HttpStatusCode.NotFound)
                {
                    TempData["Error"] = "This card no longer exists.";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                model.Destinations = await LoadDestinationsAsync();
                return View(model);
            }

            TempData["Success"] = "The card was updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, errors, status) = await DeleteAsync($"api/admin/benefit-items/{id}");
            if (!success)
            {
                if (status == HttpStatusCode.Unauthorized)
                {
                    return await SessionExpiredAsync();
                }

                // Artıq silinibsə (başqa tabda) sadəcə siyahıya qayıdırıq
                TempData["Error"] = status == HttpStatusCode.NotFound ? "This card no longer exists." : errors.FirstOrDefault();
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = "The card was deleted.";
            return RedirectToAction(nameof(Index));
        }

        // "Düymə hara aparsın" siyahısı API-dən alınır (yerlər API-də saxlanılır, doğrulama da orada edilir)
        private async Task<List<ButtonDestinationVM>> LoadDestinationsAsync()
        {
            var (destinations, status) = await GetAsync<List<ButtonDestinationVM>>("api/admin/benefit-items/destinations");
            return destinations ?? new List<ButtonDestinationVM>();
        }

        // API-yə gedən JSON gövdəsi (kənar boşluqlar silinir, ünvan API-də yenidən yoxlanılır)
        private static object ToBody(string label, string title, string description, string buttonText, string buttonUrl,
                                     string text1, string text2, string text3)
        {
            return new
            {
                label = label.Trim(),
                title = title.Trim(),
                description = description.Trim(),
                buttonText = buttonText.Trim(),
                buttonUrl = buttonUrl.Trim(),
                text1 = text1.Trim(),
                text2 = text2.Trim(),
                text3 = text3.Trim()
            };
        }
    }
}
