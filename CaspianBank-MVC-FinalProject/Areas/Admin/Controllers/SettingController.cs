using CaspianBank_MVC_FinalProject.Controllers;
using CaspianBank_MVC_FinalProject.Helpers;
using CaspianBank_MVC_FinalProject.ViewModels.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;

namespace CaspianBank_MVC_FinalProject.Areas.Admin.Controllers
{
    // UI bölməsi: saytın ümumi ayarları (loqo, şirkət adı, ünvan, email, telefon, footer mətnləri).
    // Açarlar sabitdir: yalnız görünür və dəyərləri dəyişdirilir (əlavə/silmə yoxdur). Loqo şəkildir və ayrıca yüklənir. API: api/admin/settings
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin,WebDesigner")]
    public class SettingController : ApiControllerBase
    {
        private const int MaxImageBytes = 2 * 1024 * 1024;

        public SettingController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var (settings, status) = await GetAsync<List<SettingVM>>("api/admin/settings");
            return View(settings ?? new List<SettingVM>());
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var (setting, status) = await GetAsync<SettingDetailVM>($"api/admin/settings/{id}");
            if (setting is null)
            {
                return NotFound();
            }
            return View(setting);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var (setting, status) = await GetAsync<SettingDetailVM>($"api/admin/settings/{id}");
            if (setting is null)
            {
                return NotFound();
            }

            return View(new SettingEditVM
            {
                Id = setting.Id,
                Key = setting.Key,
                Value = SettingLabels.IsLogo(setting.Key) ? null : setting.Value,
                CurrentImage = SettingLabels.IsLogo(setting.Key) ? setting.Value : string.Empty
            });
        }

        // Mətn ayarları
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SettingEditVM model)
        {
            model.Id = id;

            if (string.IsNullOrWhiteSpace(model.Value))
            {
                ModelState.AddModelError(nameof(model.Value), "Enter the value.");
            }
            if (!ModelState.IsValid)
            {
                return await EditViewAsync(model);
            }

            var (success, errors, status) = await PutAsync($"api/admin/settings/{id}", new
            {
                value = model.Value!.Trim()
            });
            if (!success)
            {
                if (status == HttpStatusCode.Unauthorized)
                {
                    return await SessionExpiredAsync();
                }

                if (status == HttpStatusCode.NotFound)
                {
                    TempData["Error"] = "This setting no longer exists.";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                return await EditViewAsync(model);
            }

            TempData["Success"] = "The setting was updated.";
            return RedirectToAction(nameof(Index));
        }

        // Loqo (şəkil)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLogo(int id, SettingEditVM model)
        {
            model.Id = id;

            if (model.Image is null || model.Image.Length == 0)
            {
                ModelState.AddModelError(nameof(model.Image), "Choose an image.");
            }
            else if (model.Image.Length > MaxImageBytes)
            {
                // API-nin limiti ilə eyni: böyük faylı boş yerə göndərmirik
                ModelState.AddModelError(nameof(model.Image), "The image must be up to 2 MB.");
            }
            if (!ModelState.IsValid)
            {
                return await EditViewAsync(model);
            }

            // Fayl API-yə multipart/form-data kimi göndərilir (JSON-da fayl göndərmək olmur)
            using var content = new MultipartFormDataContent();
            using var stream = model.Image!.OpenReadStream();
            var file = new StreamContent(stream);
            var contentType = string.IsNullOrWhiteSpace(model.Image.ContentType) ? "application/octet-stream" : model.Image.ContentType;
            file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            content.Add(file, "Image", model.Image.FileName);

            var (success, errors, status) = await PutFormAsync($"api/admin/settings/{id}/logo", content);
            if (!success)
            {
                if (status == HttpStatusCode.Unauthorized)
                {
                    return await SessionExpiredAsync();
                }

                if (status == HttpStatusCode.NotFound)
                {
                    TempData["Error"] = "This setting no longer exists.";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                return await EditViewAsync(model);
            }

            TempData["Success"] = "The logo was updated.";
            return RedirectToAction(nameof(Index));
        }

        // Xəta olub formu yenidən göstərəndə açar və cari loqo da görünsün (formadan oxunmur, API-dən alınır)
        private async Task<IActionResult> EditViewAsync(SettingEditVM model)
        {
            var (current, status) = await GetAsync<SettingDetailVM>($"api/admin/settings/{model.Id}");
            model.Key = current?.Key ?? string.Empty;
            model.CurrentImage = current is not null && SettingLabels.IsLogo(current.Key) ? current.Value : string.Empty;
            return View("Edit", model);
        }
    }
}
