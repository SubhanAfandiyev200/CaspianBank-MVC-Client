using CaspianBank_MVC_FinalProject.Controllers;
using CaspianBank_MVC_FinalProject.Helpers;
using CaspianBank_MVC_FinalProject.ViewModels.Brands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;

namespace CaspianBank_MVC_FinalProject.Areas.Admin.Controllers
{
    // UI bölməsi: Home-dakı brend logoları. Hələlik yalnız statik GetAll görünüşü, API-yə qoşulanda burada dolacaq.
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin,WebDesigner")]
    public class BrandController : ApiControllerBase
    {
        private const int MaxImageBytes = 2 * 1024 * 1024;

        public BrandController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var (brands, status) = await GetAsync<List<BrandVM>>("api/admin/brands");
            return View(brands ?? new List<BrandVM>());
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var (brand, status) = await GetAsync<BrandDetailVM>($"api/admin/brands/{id}");
            if (brand is null)
            {
                return NotFound();
            }
            return View(brand);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new BrandCreateVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BrandCreateVM model)
        {
            // API-nin limiti ilə eyni: böyük faylı boş yerə göndərmirik
            if (model.Image is not null && model.Image.Length > MaxImageBytes)
            {
                ModelState.AddModelError(nameof(model.Image), "The image must be up to 2 MB.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Fayl API-yə multipart/form-data kimi göndərilir (JSON-da fayl göndərmək olmur)
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(model.Name.Trim()), "Name");

            using var stream = model.Image!.OpenReadStream();
            var file = new StreamContent(stream);
            var contentType = string.IsNullOrWhiteSpace(model.Image.ContentType) ? "application/octet-stream" : model.Image.ContentType;
            file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            content.Add(file, "Image", model.Image.FileName);

            var (success, errors, status) = await PostFormAsync("api/admin/brands", content);
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

            TempData["Success"] = $"{model.Name.Trim()} was added.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var (brand, status) = await GetAsync<BrandVM>($"api/admin/brands/{id}");
            if (brand is null)
            {
                return NotFound();
            }

            return View(new BrandEditVM
            {
                Id = brand.Id,
                Name = brand.Name,
                CurrentImage = brand.Image
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BrandEditVM model)
        {
            model.Id = id;

            // API-nin limiti ilə eyni: böyük faylı boş yerə göndərmirik
            if (model.Image is not null && model.Image.Length > MaxImageBytes)
            {
                ModelState.AddModelError(nameof(model.Image), "The image must be up to 2 MB.");
            }

            if (!ModelState.IsValid)
            {
                return await EditViewAsync(model);
            }

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(model.Name.Trim()), "Name");

            // Şəkil seçilməyibsə heç nə göndərilmir: API köhnə şəkli saxlayır
            Stream? stream = null;
            if (model.Image is not null && model.Image.Length > 0)
            {
                stream = model.Image.OpenReadStream();
                var file = new StreamContent(stream);
                var contentType = string.IsNullOrWhiteSpace(model.Image.ContentType) ? "application/octet-stream" : model.Image.ContentType;
                file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                content.Add(file, "Image", model.Image.FileName);
            }

            using (stream)
            {
                var (success, errors, status) = await PutFormAsync($"api/admin/brands/{id}", content);
                if (!success)
                {
                    if (status == HttpStatusCode.Unauthorized)
                    {
                        return await SessionExpiredAsync();
                    }

                    if (status == HttpStatusCode.NotFound)
                    {
                        TempData["Error"] = "This brand no longer exists.";
                        return RedirectToAction(nameof(Index));
                    }

                    foreach (var error in errors)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }
                    return await EditViewAsync(model);
                }

                TempData["Success"] = $"{model.Name.Trim()} was updated.";
                return RedirectToAction(nameof(Index));
            }
        }

        // Xəta olub formu yenidən göstərəndə cari logo da görünsün (yol formadan yox, API-dən alınır)
        private async Task<IActionResult> EditViewAsync(BrandEditVM model)
        {
            var (current, status) = await GetAsync<BrandVM>($"api/admin/brands/{model.Id}");
            model.CurrentImage = current?.Image ?? string.Empty;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, errors, status) = await DeleteAsync($"api/admin/brands/{id}");
            if (!success)
            {
                if (status == HttpStatusCode.Unauthorized)
                {
                    return await SessionExpiredAsync();
                }

                // Artıq silinibsə (başqa tabda) sadəcə siyahıya qayıdırıq
                TempData["Error"] = status == HttpStatusCode.NotFound ? "This brand no longer exists." : errors.FirstOrDefault();
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = "The brand was deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
