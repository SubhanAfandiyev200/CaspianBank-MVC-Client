using CaspianBank_MVC_FinalProject.ViewModels.HomeTickers;
using Microsoft.AspNetCore.Mvc;

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