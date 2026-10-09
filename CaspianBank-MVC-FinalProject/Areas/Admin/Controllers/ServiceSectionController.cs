using CaspianBank_MVC_FinalProject.Controllers;
using CaspianBank_MVC_FinalProject.ViewModels.ServiceItems;
using CaspianBank_MVC_FinalProject.ViewModels.ServiceSections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CaspianBank_MVC_FinalProject.Areas.Admin.Controllers
{
    // UI bölməsi: Home-dakı "Services" blokunun başlıq yazısı (label, title, description). Tək yazıdır: yalnız görünür (Detail) və dəyişdirilir (Edit).
    // Kartları ServiceItemController idarə edir. API: api/admin/service-section
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin,WebDesigner")]
    public class ServiceSectionController : ApiControllerBase
    {
        public ServiceSectionController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
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
            var (section, status) = await GetAsync<ServiceSectionDetailVM>("api/admin/service-section");
            if (section is null)
            {
                return NotFound();
            }

            // Başlığın altında göstərilən kartlar
            var (items, itemsStatus) = await GetAsync<List<ServiceItemVM>>("api/admin/service-items");
            section.Items = items ?? new List<ServiceItemVM>();
            return View(section);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var (section, status) = await GetAsync<ServiceSectionDetailVM>("api/admin/service-section");
            if (section is null)
            {
                return NotFound();
            }

            return View(new ServiceSectionEditVM
            {
                Id = section.Id,
                Label = section.Label,
                Title = section.Title,
                Description = section.Description
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ServiceSectionEditVM model)
        {
            model.Id = id;
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (success, errors, status) = await PutAsync($"api/admin/service-section/{id}", new
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
                    TempData["Error"] = "The services heading was not found. Open it again.";
                    return RedirectToAction(nameof(Detail));
                }

                foreach (var error in errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                return View(model);
            }

            TempData["Success"] = "The services heading was updated.";
            return RedirectToAction(nameof(Detail));
        }
    }
}
