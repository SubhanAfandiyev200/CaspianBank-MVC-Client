using CaspianBank_MVC_FinalProject.Controllers;
using CaspianBank_MVC_FinalProject.ViewModels.CardDesigns;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;

namespace CaspianBank_MVC_FinalProject.Areas.Admin.Controllers
{
    // UI bölməsi: kart dizaynları (Home-dakı yelpazə və müştərilərin kartları). API: api/admin/card-designs
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin,WebDesigner")]
    public class CardDesignController : ApiControllerBase
    {
        private const int MaxImageBytes = 2 * 1024 * 1024;

        public CardDesignController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration) { }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var (designs, status) = await GetAsync<List<CardDesignVM>>("api/admin/card-designs");
            return View(designs ?? new List<CardDesignVM>());
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var (design, status) = await GetAsync<CardDesignDetailVM>($"api/admin/card-designs/{id}");
            if (design is null)
            {
                return NotFound();
            }
            return View(design);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CardDesignCreateVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CardDesignCreateVM model)
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
            AddFields(content, model.Title, model.DisplayOrder, model.ShowOnHome);

            using var stream = model.Image!.OpenReadStream();
            AddImage(content, stream, model.Image);

            var (success, errors, status) = await PostFormAsync("api/admin/card-designs", content);
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
            var (design, status) = await GetAsync<CardDesignVM>($"api/admin/card-designs/{id}");
            if (design is null)
            {
                return NotFound();
            }

            return View(new CardDesignEditVM
            {
                Id = design.Id,
                Title = design.Title,
                DisplayOrder = design.DisplayOrder,
                ShowOnHome = design.ShowOnHome,
                CurrentImage = design.Image
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CardDesignEditVM model)
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
            AddFields(content, model.Title, model.DisplayOrder, model.ShowOnHome);

            // Şəkil seçilməyibsə heç nə göndərilmir: API köhnə şəkli saxlayır
            Stream? stream = null;
            if (model.Image is not null && model.Image.Length > 0)
            {
                stream = model.Image.OpenReadStream();
                AddImage(content, stream, model.Image);
            }

            using (stream)
            {
                var (success, errors, status) = await PutFormAsync($"api/admin/card-designs/{id}", content);
                if (!success)
                {
                    if (status == HttpStatusCode.Unauthorized)
                    {
                        return await SessionExpiredAsync();
                    }

                    if (status == HttpStatusCode.NotFound)
                    {
                        TempData["Error"] = "This design no longer exists.";
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
            var (success, errors, status) = await DeleteAsync($"api/admin/card-designs/{id}");
            if (!success)
            {
                if (status == HttpStatusCode.Unauthorized)
                {
                    return await SessionExpiredAsync();
                }

                // Artıq silinibsə (başqa tabda) sadəcə siyahıya qayıdırıq. İstifadədəki dizayn üçün API-nin mesajı göstərilir
                TempData["Error"] = status == HttpStatusCode.NotFound ? "This design no longer exists." : errors.FirstOrDefault();
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = "The design was deleted.";
            return RedirectToAction(nameof(Index));
        }

        // Xəta olub formu yenidən göstərəndə cari şəkil də görünsün (yol formadan yox, API-dən alınır)
        private async Task<IActionResult> EditViewAsync(CardDesignEditVM model)
        {
            var (current, status) = await GetAsync<CardDesignVM>($"api/admin/card-designs/{model.Id}");
            model.CurrentImage = current?.Image ?? string.Empty;
            return View(model);
        }

        private static void AddFields(MultipartFormDataContent content, string title, int displayOrder, bool showOnHome)
        {
            content.Add(new StringContent(title.Trim()), "Title");
            content.Add(new StringContent(displayOrder.ToString(CultureInfo.InvariantCulture)), "DisplayOrder");
            content.Add(new StringContent(showOnHome ? "true" : "false"), "ShowOnHome");
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
