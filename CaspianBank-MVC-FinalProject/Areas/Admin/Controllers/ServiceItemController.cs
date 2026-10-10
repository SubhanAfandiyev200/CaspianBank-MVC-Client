using CaspianBank_MVC_FinalProject.Controllers;
using CaspianBank_MVC_FinalProject.ViewModels.ServiceItems;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;

namespace CaspianBank_MVC_FinalProject.Areas.Admin.Controllers
{
    // UI bölməsi: Home-dakı altı xidmət kartı. Hər kart proqramın bir bölməsinə aparır, ona görə mətnləri və ikonu dəyişir, amma əlavə/silmə yoxdur.
    // Başlıq yazısını ServiceSectionController idarə edir. API: api/admin/service-items
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin,WebDesigner")]
    public class ServiceItemController : ApiControllerBase
    {
        private const int MaxImageBytes = 2 * 1024 * 1024;

        public ServiceItemController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

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
                CurrentIcon = item.Icon
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ServiceItemEditVM model)
        {
            model.Id = id;

            // API-nin limiti ilə eyni: böyük faylı boş yerə göndərmirik
            if (model.Icon is not null && model.Icon.Length > MaxImageBytes)
            {
                ModelState.AddModelError(nameof(model.Icon), "The icon must be up to 2 MB.");
            }

            if (!ModelState.IsValid)
            {
                return await EditViewAsync(model);
            }

            // Fayl API-yə multipart/form-data kimi göndərilir (JSON-da fayl göndərmək olmur)
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(model.Title.Trim()), "Title");
            content.Add(new StringContent(model.Description.Trim()), "Description");

            // İkon seçilməyibsə heç nə göndərilmir: API köhnə ikonu saxlayır
            Stream? stream = null;
            if (model.Icon is not null && model.Icon.Length > 0)
            {
                stream = model.Icon.OpenReadStream();
                var file = new StreamContent(stream);
                var contentType = string.IsNullOrWhiteSpace(model.Icon.ContentType) ? "application/octet-stream" : model.Icon.ContentType;
                file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                content.Add(file, "Icon", model.Icon.FileName);
            }

            using (stream)
            {
                var (success, errors, status) = await PutFormAsync($"api/admin/service-items/{id}", content);
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
        }

        // Xəta olub formu yenidən göstərəndə nömrə və cari ikon da görünsün (formadan oxunmur, API-dən alınır)
        private async Task<IActionResult> EditViewAsync(ServiceItemEditVM model)
        {
            var (current, status) = await GetAsync<ServiceItemDetailVM>($"api/admin/service-items/{model.Id}");
            model.Number = current?.Number ?? 0;
            model.CurrentIcon = current?.Icon ?? string.Empty;
            return View(model);
        }
    }
}
