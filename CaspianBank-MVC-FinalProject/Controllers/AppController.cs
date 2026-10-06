using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CaspianBank_MVC_FinalProject.Controllers
{
    // Giriş tələb edir: login olmayan /App açsa cookie ayarındakı LoginPath-ə yönləndirilir
    [Authorize]
    public class AppController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
