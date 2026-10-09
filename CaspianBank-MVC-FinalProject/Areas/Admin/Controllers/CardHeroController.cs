using CaspianBank_MVC_FinalProject.Controllers;
using CaspianBank_MVC_FinalProject.ViewModels.CardHeroes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CaspianBank_MVC_FinalProject.Areas.Admin.Controllers
{
    // UI bölməsi: Home-un yuxarı hissəsindəki sol mətn. Tək yazıdır: yalnız görünür (Detail) və dəyişdirilir (Edit).
    // Sağdakı kartlar Card designs səhifəsindədir. API: api/admin/card-hero
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin,WebDesigner")]
    public class CardHeroController : ApiControllerBase
    {
        public CardHeroController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
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
            var (hero, status) = await GetAsync<CardHeroDetailVM>("api/admin/card-hero");
            if (hero is null)
            {
                return NotFound();
            }
            return View(hero);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var (hero, status) = await GetAsync<CardHeroDetailVM>("api/admin/card-hero");
            if (hero is null)
            {
                return NotFound();
            }

            return View(new CardHeroEditVM
            {
                Id = hero.Id,
                Label = hero.Label,
                Title = hero.Title,
                Description = hero.Description
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CardHeroEditVM model)
        {
            model.Id = id;
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (success, errors, status) = await PutAsync($"api/admin/card-hero/{id}", new
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
                    TempData["Error"] = "The hero text was not found. Open it again.";
                    return RedirectToAction(nameof(Detail));
                }

                foreach (var error in errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                return View(model);
            }

            TempData["Success"] = "The hero text was updated.";
            return RedirectToAction(nameof(Detail));
        }
    }
}
