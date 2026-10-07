using CaspianBank_MVC_FinalProject.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CaspianBank_MVC_FinalProject.Areas.Admin.Controllers
{
    // BANK bölməsi: bankın ümumi maliyyə vəziyyəti (gəlir, xərc, vergi, ehtiyat, istifadəçilər)
    [Area("Admin")]
    [Authorize(Roles = AppRoles.Staff)]
    public class BankDashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
