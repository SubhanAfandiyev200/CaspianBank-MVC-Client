using CaspianBank_MVC_FinalProject.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CaspianBank_MVC_FinalProject.Areas.Admin.Controllers
{
    // BANK bölməsi: audit log
    [Area("Admin")]
    [Authorize(Roles = AppRoles.Staff)]
    public class AuditController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
