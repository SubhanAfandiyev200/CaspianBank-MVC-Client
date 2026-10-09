using CaspianBank_MVC_FinalProject.Controllers;
using CaspianBank_MVC_FinalProject.ViewModels.AboutPillars;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;

namespace CaspianBank_MVC_FinalProject.Areas.Admin.Controllers
{
    // UI bölməsi: Home-dakı About blokunun altındakı sütunlar (şəkil, başlıq, açıqlama). Mətn blokunu AboutController idarə edir.
    // API: api/admin/about-pillars
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin,WebDesigner")]
    public class AboutPillarController : ApiControllerBase
    {
        private const int MaxImageBytes = 2 * 1024 * 1024;

        public AboutPillarController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration) { }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var (pillars, status) = await GetAsync<List<AboutPillarVM>>("api/admin/about-pillars");
            return View(pillars ?? new List<AboutPillarVM>());
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var (pillar, status) = await GetAsync<AboutPillarDetailVM>($"api/admin/about-pillars/{id}");
            if (pillar is null)
            {
                return NotFound();
            }
            return View(pillar);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new AboutPillarCreateVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AboutPillarCreateVM model)
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
            content.Add(new StringContent(model.Title.Trim()), "Title");
            content.Add(new StringContent(model.Description.Trim()), "Description");

            using var stream = model.Image!.OpenReadStream();
            AddImage(content, stream, model.Image);

            var (success, errors, status) = await PostFormAsync("api/admin/about-pillars", content);
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

            TempData["Success"] = $"{model.Title.Trim()} was added.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var (pillar, status) = await GetAsync<AboutPillarVM>($"api/admin/about-pillars/{id}");
            if (pillar is null)
            {
                return NotFound();
            }

            return View(new AboutPillarEditVM
            {
                Id = pillar.Id,
                Title = pillar.Title,
                Description = pillar.Description,
                CurrentImage = pillar.Image
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AboutPillarEditVM model)
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
            content.Add(new StringContent(model.Title.Trim()), "Title");
            content.Add(new StringContent(model.Description.Trim()), "Description");

            // Şəkil seçilməyibsə heç nə göndərilmir: API köhnə şəkli saxlayır
            Stream? stream = null;
            if (model.Image is not null && model.Image.Length > 0)
            {
                stream = model.Image.OpenReadStream();
                AddImage(content, stream, model.Image);
            }

            using (stream)
            {
                var (success, errors, status) = await PutFormAsync($"api/admin/about-pillars/{id}", content);
                if (!success)
                {
                    if (status == HttpStatusCode.Unauthorized)
                    {
                        return await SessionExpiredAsync();
                    }

                    if (status == HttpStatusCode.NotFound)
                    {
                        TempData["Error"] = "This pillar no longer exists.";
                        return RedirectToAction(nameof(Index));
                    }

                    foreach (var error in errors)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }
                    return await EditViewAsync(model);
                }

                TempData["Success"] = $"{model.Title.Trim()} was updated.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, errors, status) = await DeleteAsync($"api/admin/about-pillars/{id}");
            if (!success)
            {
                if (status == HttpStatusCode.Unauthorized)
                {
                    return await SessionExpiredAsync();
                }

                // Artıq silinibsə (başqa tabda) sadəcə siyahıya qayıdırıq
                TempData["Error"] = status == HttpStatusCode.NotFound ? "This pillar no longer exists." : errors.FirstOrDefault();
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = "The pillar was deleted.";
            return RedirectToAction(nameof(Index));
        }

        // Xəta olub formu yenidən göstərəndə cari şəkil də görünsün (yol formadan yox, API-dən alınır)
        private async Task<IActionResult> EditViewAsync(AboutPillarEditVM model)
        {
            var (current, status) = await GetAsync<AboutPillarVM>($"api/admin/about-pillars/{model.Id}");
            model.CurrentImage = current?.Image ?? string.Empty;
            return View(model);
        }

        private static void AddImage(MultipartFormDataContent content, Stream stream, IFormFile image)
        {
            var file = new StreamContent(stream);
            var contentType = string.IsNullOrWhiteSpace(image.ContentType) ? "application/octet-stream" : image.ContentType;
            file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            content.Add(file, "Image", image.FileName);
        }
    }
}
