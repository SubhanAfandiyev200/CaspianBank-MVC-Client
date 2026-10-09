using CaspianBank_MVC_FinalProject.Controllers;
using CaspianBank_MVC_FinalProject.ViewModels.ServiceItems;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CaspianBank_MVC_FinalProject.Areas.Admin.Controllers
{
    // UI bölməsi: Home-dakı altı xidmət kartı. Hər kart proqramın bir bölməsinə aparır, ona görə yalnız mətnləri dəyişir (əlavə/silmə yoxdur).
    // Başlıq yazısını ServiceSectionController idarə edir. API: api/admin/service-items
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin,WebDesigner")]
    public class ServiceItemController : ApiControllerBase
    {
        public ServiceItemController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration) { }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var (items, status) = await GetAsync<List<ServiceItemVM>>("api/admin/service-items");
            return View(items ?? new List<ServiceItemVM>());
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var (item, status) = await GetAsync<ServiceItemDetailVM>($"api/admin/service-items/{id}");
            if (item is null)
            {
                return NotFound();
            }
            return View(item);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var (item, status) = await GetAsync<ServiceItemDetailVM>($"api/admin/service-items/{id}");
            if (item is null)
            {
                return NotFound();
            }

            return View(new ServiceItemEditVM
            {
                Id = item.Id,
                Title = item.Title,
                Description = item.Description,
                Number = item.Number,
                Icon = item.Icon
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ServiceItemEditVM model)
        {
            model.Id = id;
            if (!ModelState.IsValid)
            {
                return await EditViewAsync(model);
            }

            var (success, errors, status) = await PutAsync($"api/admin/service-items/{id}", new
            {
                title = model.Title.Trim(),
                description = model.Description.Trim()
            });
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
                return await EditViewAsync(model);
            }

            TempData["Success"] = "The card was updated.";
            return RedirectToAction(nameof(Index));
        }

        // Xəta olub formu yenidən göstərəndə nömrə və ikon da görünsün (formadan oxunmur, API-dən alınır)
        private async Task<IActionResult> EditViewAsync(ServiceItemEditVM model)
        {
            var (current, status) = await GetAsync<ServiceItemDetailVM>($"api/admin/service-items/{model.Id}");
            model.Number = current?.Number ?? 0;
            model.Icon = current?.Icon ?? string.Empty;
            return View(model);
        }
    }
}
