using CaspianBank_MVC_FinalProject.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CaspianBank_MVC_FinalProject.Areas.Admin.Controllers
{
    // BANK bölməsi: kredit müraciətləri
    [Area("Admin")]
    [Authorize(Roles = AppRoles.Staff)]
    public class LoansController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
