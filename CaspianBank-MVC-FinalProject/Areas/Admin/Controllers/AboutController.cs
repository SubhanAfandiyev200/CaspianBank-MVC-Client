using CaspianBank_MVC_FinalProject.Controllers;
using CaspianBank_MVC_FinalProject.ViewModels.AboutPillars;
using CaspianBank_MVC_FinalProject.ViewModels.Abouts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;

namespace CaspianBank_MVC_FinalProject.Areas.Admin.Controllers
{
    // UI bölməsi: Home-dakı "About" blokunun mətni (label, title, description) və videosu. Tək yazıdır: yalnız görünür (Detail) və dəyişdirilir (Edit).
    // Video ayrıca dəyişdirilir (EditVideo). Sütunları AboutPillarController idarə edir. API: api/admin/about
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin,WebDesigner")]
    public class AboutController : ApiControllerBase
    {
        private const long MaxVideoBytes = 50L * 1024 * 1024;
        private const long MaxRequestBytes = 60L * 1024 * 1024;

        public AboutController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        // Tək yazı olduğu üçün siyahı yoxdur: birbaşa Detail açılır
        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction(nameof(Detail));
        }

        [HttpGet]
        public async Task<IActionResult> Detail()
        {
            var (about, status) = await GetAsync<AboutDetailVM>("api/admin/about");
            if (about is null)
            {
                return NotFound();
            }

            // Mətnin altında göstərilən sütunlar
            var (pillars, pillarsStatus) = await GetAsync<List<AboutPillarVM>>("api/admin/about-pillars");
            about.Pillars = pillars ?? new List<AboutPillarVM>();
            return View(about);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var (about, status) = await GetAsync<AboutDetailVM>("api/admin/about");
            if (about is null)
            {
                return NotFound();
            }

            return View(new AboutEditVM
            {
                Id = about.Id,
                Label = about.Label,
                Title = about.Title,
                Description = about.Description
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AboutEditVM model)
        {
            model.Id = id;
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (success, errors, status) = await PutAsync($"api/admin/about/{id}", new
            {
                label = model.Label.Trim(),
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
                    TempData["Error"] = "The About text was not found. Open it again.";
                    return RedirectToAction(nameof(Detail));
                }

                foreach (var error in errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                return View(model);
            }

            TempData["Success"] = "The About text was updated.";
            return RedirectToAction(nameof(Detail));
        }

        [HttpGet]
        public async Task<IActionResult> EditVideo()
        {
            var (about, status) = await GetAsync<AboutDetailVM>("api/admin/about");
            if (about is null)
            {
                return NotFound();
            }

            return View(new AboutVideoEditVM
            {
                Id = about.Id,
                CurrentVideo = about.VideoPath
            });
        }

        // Video 50 MB-a qədərdir: Kestrel-in ümumi 30 MB limiti və form limiti yalnız bu əməliyyat üçün yüksəldilir (API-dəki kimi)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(MaxRequestBytes)]
        [RequestFormLimits(MultipartBodyLengthLimit = MaxRequestBytes)]
        public async Task<IActionResult> EditVideo(int id, AboutVideoEditVM model)
        {
            model.Id = id;

            if (model.Video is null || model.Video.Length == 0)
            {
                ModelState.AddModelError(nameof(model.Video), "Choose a video.");
            }
            else if (model.Video.Length > MaxVideoBytes)
            {
                // API-nin limiti ilə eyni: böyük faylı boş yerə göndərmirik
                ModelState.AddModelError(nameof(model.Video), "The video must be up to 50 MB.");
            }
            if (!ModelState.IsValid)
            {
                return await EditVideoViewAsync(model);
            }

            // Fayl API-yə multipart/form-data kimi axınla göndərilir (video böyükdür, yaddaşa oxunmur)
            using var content = new MultipartFormDataContent();
            using var stream = model.Video!.OpenReadStream();
            var file = new StreamContent(stream);
            var contentType = string.IsNullOrWhiteSpace(model.Video.ContentType) ? "application/octet-stream" : model.Video.ContentType;
            file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            content.Add(file, "Video", model.Video.FileName);

            var (success, errors, status) = await PutFormAsync($"api/admin/about/{id}/video", content);
            if (!success)
            {
                if (status == HttpStatusCode.Unauthorized)
                {
                    return await SessionExpiredAsync();
                }

                if (status == HttpStatusCode.NotFound)
                {
                    TempData["Error"] = "The About text was not found. Open it again.";
                    return RedirectToAction(nameof(Detail));
                }

                foreach (var error in errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                return await EditVideoViewAsync(model);
            }

            TempData["Success"] = "The video was updated.";
            return RedirectToAction(nameof(Detail));
        }

        // Xəta olub formu yenidən göstərəndə hazırkı video da görünsün (yol formadan yox, API-dən alınır)
        private async Task<IActionResult> EditVideoViewAsync(AboutVideoEditVM model)
        {
            var (current, status) = await GetAsync<AboutDetailVM>("api/admin/about");
            model.CurrentVideo = current?.VideoPath ?? string.Empty;
            return View("EditVideo", model);
        }
    }
}
