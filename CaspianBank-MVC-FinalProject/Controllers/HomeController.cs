using CaspianBank_MVC_FinalProject.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;

namespace CaspianBank_MVC_FinalProject.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        // Xəta səhifəsi. Program.cs-dəki UseStatusCodePagesWithReExecute (404, 403 və s.) və UseExceptionHandler (500) bura yönləndirir
        public IActionResult Error(int? code)
        {
            var status = code ?? StatusCodes.Status500InternalServerError;
            Response.StatusCode = status;
            return View(new ErrorVM(status));
        }

        // Rolu uyğun olmayan istifadəçi (cookie AccessDeniedPath) buraya düşür
        public IActionResult AccessDenied()
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return View("Error", new ErrorVM(StatusCodes.Status403Forbidden));
        }
    }
}
