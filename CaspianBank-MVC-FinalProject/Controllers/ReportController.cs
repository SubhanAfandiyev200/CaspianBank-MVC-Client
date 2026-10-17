using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CaspianBank_MVC_FinalProject.Controllers
{
    // Şübhəli əməliyyatı bildirmək (dizayn: CaspianBank-Frontend/report-transaction.html). History cədvəlindəki "Report" düyməsi bura gətirir.
    // Hələlik yalnız görünüşdür: əməliyyatın məlumatı linkdən gəlir və göstərilir, göndərmə düyməsi söndürülüb (fraud komandasının admin tərəfi yazılanda qoşulacaq)
    [Authorize]
    [Route("App/Report")]
    public class ReportController : Controller
    {
        [HttpGet("")]
        public IActionResult Index(string? reference, string? description, decimal? amount, DateTime? date)
        {
            // Link olmadan açılıbsa History-yə qaytarılır: bildiriləcək əməliyyat yoxdur
            if (string.IsNullOrWhiteSpace(reference))
            {
                return RedirectToAction("Index", "History");
            }

            return View(new ReportPreview
            {
                Reference = reference.Trim(),
                Description = string.IsNullOrWhiteSpace(description) ? "Card transaction" : description.Trim(),
                Amount = amount,
                Date = date
            });
        }

        // Linkdən gələn məlumat yalnız ekranda göstərilir, heç yerə yazılmır
        public class ReportPreview
        {
            public string Reference { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public decimal? Amount { get; set; }
            public DateTime? Date { get; set; }
        }
    }
}
