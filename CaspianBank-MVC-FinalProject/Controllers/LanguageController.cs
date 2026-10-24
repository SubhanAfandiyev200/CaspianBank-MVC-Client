using CaspianBank_MVC_FinalProject.Services;
using Microsoft.AspNetCore.Mvc;

namespace CaspianBank_MVC_FinalProject.Controllers
{
    // Dil seçimi: seçim cookie-də bir il saxlanılır, sonra istifadəçi gəldiyi səhifəyə qayıdır
    [Route("Language")]
    public class LanguageController : Controller
    {
        [HttpGet("Set")]
        public IActionResult Set(string? lang, string? returnUrl)
        {
            if (lang is not null && LanguageMiddleware.Supported.Contains(lang))
            {
                Response.Cookies.Append(LanguageMiddleware.CookieName, lang, new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    HttpOnly = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax,
                    Secure = Request.IsHttps
                });
            }

            // Yalnız bu saytın daxili ünvanına qayıdılır (başqa saytın ünvanı verilsə ana səhifəyə)
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
