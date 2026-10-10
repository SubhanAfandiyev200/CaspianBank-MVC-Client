using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CaspianBank_MVC_FinalProject.Controllers
{
    // Müştərinin kredit müraciəti səhifəsi (dizayn: CaspianBank-Frontend/loan-application.html).
    // Hələlik yalnız statik görünüş: müraciət göndərmə, aylıq ödənişin hesablanması və müraciətlər cədvəli sonra yazılacaq
    [Authorize]
    [Route("App/Loans")]
    public class LoansController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
