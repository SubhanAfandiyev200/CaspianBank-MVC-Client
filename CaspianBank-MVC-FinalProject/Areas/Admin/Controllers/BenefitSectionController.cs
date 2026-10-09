using CaspianBank_MVC_FinalProject.Controllers;
using CaspianBank_MVC_FinalProject.ViewModels.BenefitItems;
using CaspianBank_MVC_FinalProject.ViewModels.BenefitSections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CaspianBank_MVC_FinalProject.Areas.Admin.Controllers
{
    // UI bölməsi: Home-dakı "Benefits" blokunun başlıq yazısı (label, title, description). Tək yazıdır: yalnız görünür (Detail) və dəyişdirilir (Edit).
    // Kartları BenefitItemController idarə edir. API: api/admin/benefit-section
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin,WebDesigner")]
    public class BenefitSectionController : ApiControllerBase
    {
        public BenefitSectionController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
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
            var (section, status) = await GetAsync<BenefitSectionDetailVM>("api/admin/benefit-section");
            if (section is null)
            {
                return NotFound();
            }

            // Başlığın altında göstərilən kartlar (hamısı bu bölməyə aiddir)
            var (items, itemsStatus) = await GetAsync<List<BenefitItemVM>>("api/admin/benefit-items");
            section.Items = items ?? new List<BenefitItemVM>();
            return View(section);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var (section, status) = await GetAsync<BenefitSectionDetailVM>("api/admin/benefit-section");
            if (section is null)
            {
                return NotFound();
            }

            return View(new BenefitSectionEditVM
            {
                Id = section.Id,
                Label = section.Label,
                Title = section.Title,
                Description = section.Description
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BenefitSectionEditVM model)
        {
            model.Id = id;
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (success, errors, status) = await PutAsync($"api/admin/benefit-section/{id}", new
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
                    TempData["Error"] = "The benefits heading was not found. Open it again.";
                    return RedirectToAction(nameof(Detail));
                }

                foreach (var error in errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                return View(model);
            }

            TempData["Success"] = "The benefits heading was updated.";
            return RedirectToAction(nameof(Detail));
        }
    }
}
