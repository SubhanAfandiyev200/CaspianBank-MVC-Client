using CaspianBank_MVC_FinalProject.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CaspianBank_MVC_FinalProject.Areas.Admin.Controllers
{
    // BANK bölməsi: şübhəli əməliyyat şikayətləri
    [Area("Admin")]
    [Authorize(Roles = AppRoles.Staff)]
    public class ReportsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
