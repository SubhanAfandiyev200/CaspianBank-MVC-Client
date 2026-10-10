using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CaspianBank_MVC_FinalProject.Controllers
{
    // Kommunal ödənişlər səhifəsi (dizayn: CaspianBank-Frontend/bill-payment.html).
    // Hələlik yalnız statik görünüş: ödəniş, təkrarlanan ödənişlər və kredit ödənişi sonra yazılacaq
    [Authorize]
    [Route("App/Bills")]
    public class BillsController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
