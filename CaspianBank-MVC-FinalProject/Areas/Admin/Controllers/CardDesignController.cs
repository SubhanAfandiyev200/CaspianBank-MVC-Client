using CaspianBank_MVC_FinalProject.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CaspianBank_MVC_FinalProject.Areas.Admin.Controllers
{
    // UI bölməsi: kart dizaynları
    [Area("Admin")]
    [Authorize(Roles = AppRoles.Staff)]
    public class CardDesignController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
