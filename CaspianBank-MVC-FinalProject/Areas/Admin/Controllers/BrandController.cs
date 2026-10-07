using CaspianBank_MVC_FinalProject.Controllers;
using CaspianBank_MVC_FinalProject.Helpers;
using CaspianBank_MVC_FinalProject.ViewModels.Brands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CaspianBank_MVC_FinalProject.Areas.Admin.Controllers
{
    // UI bölməsi: Home-dakı brend logoları. Hələlik yalnız statik GetAll görünüşü, API-yə qoşulanda burada dolacaq.
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin,WebDesigner")]
    public class BrandController : ApiControllerBase
    {
        public BrandController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration) { }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var (brands, status) = await GetAsync<List<BrandVM>>("api/admin/brands");
            return View(brands ?? new List<BrandVM>());
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var (brand, status) = await GetAsync<BrandVM>($"api/admin/brands/{id}");
            if (brand is null)
            {
                return NotFound();
            }
            return View(brand);
        }
    }
}
