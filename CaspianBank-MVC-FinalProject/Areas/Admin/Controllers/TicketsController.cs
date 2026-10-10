using CaspianBank_MVC_FinalProject.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CaspianBank_MVC_FinalProject.Areas.Admin.Controllers
{
    // BANK bölməsi: əlaqə mesajları
    [Area("Admin")]
    [Authorize(Roles = AppRoles.Staff)]
    public class TicketsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Detail()
        {
            return View();
        }
    }
}
