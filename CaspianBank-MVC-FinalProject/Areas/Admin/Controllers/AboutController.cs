using CaspianBank_MVC_FinalProject.Controllers;
using CaspianBank_MVC_FinalProject.ViewModels.AboutPillars;
using CaspianBank_MVC_FinalProject.ViewModels.Abouts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CaspianBank_MVC_FinalProject.Areas.Admin.Controllers
{
    // UI bölməsi: Home-dakı "About" blokunun mətni (label, title, description) və videosu. Tək yazıdır: yalnız görünür (Detail) və dəyişdirilir (Edit).
    // Video faylı dəyişdirilmir. Sütunları AboutPillarController idarə edir. API: api/admin/about
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin,WebDesigner")]
    public class AboutController : ApiControllerBase
    {
        public AboutController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration) { }

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
    }
}
