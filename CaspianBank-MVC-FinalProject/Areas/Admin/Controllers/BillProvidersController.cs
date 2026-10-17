using CaspianBank_MVC_FinalProject.Controllers;
using CaspianBank_MVC_FinalProject.Helpers;
using CaspianBank_MVC_FinalProject.ViewModels.Bills;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;

namespace CaspianBank_MVC_FinalProject.Areas.Admin.Controllers
{
    // Bank bölməsi: ödəniş təchizatçılarının adı, loqosu və aktivliyi (Admin, SuperAdmin). API: api/admin/bill-providers.
    // Əlavə etmə və silmə yoxdur: təchizatçılar və qaydalar (operator nömrələri, qiymətlər) koddadır
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class BillProvidersController : ApiControllerBase
    {
        private const int MaxImageBytes = 2 * 1024 * 1024;

        public BillProvidersController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var (providers, status) = await GetAsync<List<BillProviderVM>>("api/admin/bill-providers");
            if (status == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }

            var baseUrl = Configuration["ApiSettings:BaseUrl"];
            var list = providers ?? new List<BillProviderVM>();
            foreach (var provider in list)
            {
                provider.LogoUrl = ApiUrl.ToAbsolute(baseUrl, provider.LogoUrl);
            }
            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var (provider, status) = await GetAsync<BillProviderVM>($"api/admin/bill-providers/{id}");
            if (status == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }
            if (provider is null)
            {
                return NotFound();
            }

            return View(new BillProviderEditVM
            {
                Id = provider.Id,
                Name = provider.Name,
                IsActive = provider.IsActive,
                Code = provider.Code,
                Category = provider.Category,
                CurrentLogo = ApiUrl.ToAbsolute(Configuration["ApiSettings:BaseUrl"], provider.LogoUrl)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BillProviderEditVM model)
        {
            model.Id = id;

            // API-nin limiti ilə eyni: böyük faylı boş yerə göndərmirik
            if (model.Logo is not null && model.Logo.Length > MaxImageBytes)
            {
                ModelState.AddModelError(nameof(model.Logo), "The logo must be up to 2 MB.");
            }

            if (!ModelState.IsValid)
            {
                return await EditViewAsync(model);
            }

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(model.Name.Trim()), "Name");
            content.Add(new StringContent(model.IsActive ? "true" : "false"), "IsActive");

            // Loqo seçilməyibsə heç nə göndərilmir: API köhnə loqonu saxlayır
            Stream? stream = null;
            if (model.Logo is not null && model.Logo.Length > 0)
            {
                stream = model.Logo.OpenReadStream();
                var file = new StreamContent(stream);
                var contentType = string.IsNullOrWhiteSpace(model.Logo.ContentType) ? "application/octet-stream" : model.Logo.ContentType;
                file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                content.Add(file, "Logo", model.Logo.FileName);
            }

            using (stream)
            {
                var (success, errors, status) = await PutFormAsync($"api/admin/bill-providers/{id}", content);
                if (!success)
                {
                    if (status == HttpStatusCode.Unauthorized)
                    {
                        return await SessionExpiredAsync();
                    }
                    if (status == HttpStatusCode.NotFound)
                    {
                        TempData["Error"] = "This provider no longer exists.";
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

        // Xəta olub formu yenidən göstərəndə kod, kateqoriya və cari loqo API-dən alınır (formadan oxunmur)
        private async Task<IActionResult> EditViewAsync(BillProviderEditVM model)
        {
            var (current, _) = await GetAsync<BillProviderVM>($"api/admin/bill-providers/{model.Id}");
            if (current is not null)
            {
                model.Code = current.Code;
                model.Category = current.Category;
                model.CurrentLogo = ApiUrl.ToAbsolute(Configuration["ApiSettings:BaseUrl"], current.LogoUrl);
            }
            return View("Edit", model);
        }
    }
}
