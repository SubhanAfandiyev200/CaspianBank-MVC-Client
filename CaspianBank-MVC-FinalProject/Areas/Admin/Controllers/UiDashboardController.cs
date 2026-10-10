using CaspianBank_MVC_FinalProject.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CaspianBank_MVC_FinalProject.Areas.Admin.Controllers
{
    // UI bölməsi: saytın görünüşünü idarə edən səhifələrin ümumi paneli
    [Area("Admin")]
    [Authorize(Roles = AppRoles.Staff)]
    public class UiDashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
