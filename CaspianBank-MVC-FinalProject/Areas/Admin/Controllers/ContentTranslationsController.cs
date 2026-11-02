using CaspianBank_MVC_FinalProject.Controllers;
using CaspianBank_MVC_FinalProject.Helpers;
using CaspianBank_MVC_FinalProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CaspianBank_MVC_FinalProject.Areas.Admin.Controllers
{
    // Admin yaratma/redaktə səhifələrindəki dil seçimi (EN | AZ | RU) redaktədə mövcud tərcümələri bu əməliyyatla gətirir (wwwroot/js/lang-edit.js).
    // Tərcümələrin yazılması forma ilə birlikdə gedir: Helpers/SaveTranslationsFilter.cs
    // Tərcümə ingilis mətnə bağlıdır: açar səhifədə saxlanmış ingilis mətnin özüdür. Yalnız SuperAdmin və Admin
    [Area("Admin")]
    [Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.Admin)]
    public class ContentTranslationsController : ApiControllerBase
    {

        public ContentTranslationsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        // Seçilən dilin tərcümələri: { "ingilis mətn": "tərcümə" }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Lookup(string language, string[] keys)
        {
            var (data, errors, status) = await PostAsync<Dictionary<string, string>>("api/admin/translations/lookup", new { language, keys });
            if (status == HttpStatusCode.Unauthorized)
            {
                return Unauthorized();
            }
            if (data is null)
            {
                return BadRequest(new { errors = Translate(errors) });
            }
            return Json(data);
        }


        private string[] Translate(string[] errors)
        {
            var translator = HttpContext.RequestServices.GetRequiredService<ITranslator>();
            return errors.Select(error => translator[error]).ToArray();
        }
    }
}
