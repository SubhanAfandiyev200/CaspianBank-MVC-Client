using CaspianBank_MVC_FinalProject.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CaspianBank_MVC_FinalProject.Areas.Admin.Controllers
{
    // BANK bölməsi: işçilər və rolları. Yalnız SuperAdmin görür və idarə edir (Admin rol verə/silə bilməz).
    // Hələlik statik dizayndır (nümunə sətirlər): siyahı, əlavə etmə, rol dəyişmə və deaktiv etmə API hazır olanda işləyəcək
    [Area("Admin")]
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public class StaffController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
