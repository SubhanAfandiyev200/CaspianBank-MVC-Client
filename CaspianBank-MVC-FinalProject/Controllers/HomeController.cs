using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CaspianBank_MVC_FinalProject.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
