using CaspianBank_MVC_FinalProject.Controllers;
using CaspianBank_MVC_FinalProject.ViewModels.BenefitItems;
using CaspianBank_MVC_FinalProject.ViewModels.BenefitSections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CaspianBank_MVC_FinalProject.Areas.Admin.Controllers
{
    // UI bölməsi: Home-dakı "Benefits" blokunun başlıq hissəsi (label, title, description). Kartları BenefitItemController idarə edir.
    // API: api/admin/benefit-sections
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin,WebDesigner")]
    public class BenefitSectionController : ApiControllerBase
    {
        public BenefitSectionController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration) { }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var (sections, status) = await GetAsync<List<BenefitSectionVM>>("api/admin/benefit-sections");
            return View(sections ?? new List<BenefitSectionVM>());
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var (section, status) = await GetAsync<BenefitSectionVM>($"api/admin/benefit-sections/{id}");
            if (section is null)
            {
                return NotFound();
            }

            // Bu bölməyə aid kartlar (silməzdən əvvəl nələrin silinəcəyi görünsün)
            var (items, itemsStatus) = await GetAsync<List<BenefitItemVM>>("api/admin/benefit-items");
            ViewBag.Items = (items ?? new List<BenefitItemVM>()).Where(item => item.BenefitSectionId == id).ToList();
            return View(section);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new BenefitSectionCreateVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BenefitSectionCreateVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (success, errors, status) = await PostAsync("api/admin/benefit-sections", new
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

                foreach (var error in errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                return View(model);
            }

            TempData["Success"] = "The section was added.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var (section, status) = await GetAsync<BenefitSectionVM>($"api/admin/benefit-sections/{id}");
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

            var (success, errors, status) = await PutAsync($"api/admin/benefit-sections/{id}", new
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
                    TempData["Error"] = "This section no longer exists.";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                return View(model);
            }

            TempData["Success"] = "The section was updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, errors, status) = await DeleteAsync($"api/admin/benefit-sections/{id}");
            if (!success)
            {
                if (status == HttpStatusCode.Unauthorized)
                {
                    return await SessionExpiredAsync();
                }

                // Artıq silinibsə (başqa tabda) sadəcə siyahıya qayıdırıq
                TempData["Error"] = status == HttpStatusCode.NotFound ? "This section no longer exists." : errors.FirstOrDefault();
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = "The section and its cards were deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
