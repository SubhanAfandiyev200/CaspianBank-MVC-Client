using CaspianBank_MVC_FinalProject.ViewModels.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CaspianBank_MVC_FinalProject.Controllers
{
    // Şübhəli əməliyyatı bildirmək (dizayn: CaspianBank-Frontend/report-transaction.html). History cədvəlindəki "Report" düyməsi bura gətirir.
    // Qaydalar (öz əməliyyatı, bir dəfə bildirmək, səbəbin uzunluğu) API-dədir; burada yalnız forma, uğur mesajı və öz bildirişlərinin siyahısı var
    [Authorize]
    [Route("App/Report")]
    public class ReportController : ApiControllerBase
    {
        public ReportController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        [HttpGet("")]
        public IActionResult Index(int? transactionId, string? reference, string? description, decimal? amount, DateTime? date)
        {
            // Link olmadan açılıbsa History-yə qaytarılır: bildiriləcək əməliyyat yoxdur
            if (!transactionId.HasValue || string.IsNullOrWhiteSpace(reference))
            {
                return RedirectToAction("Index", "History");
            }

            return View(new ReportFormVM
            {
                TransactionId = transactionId.Value,
                Reference = reference.Trim(),
                Description = string.IsNullOrWhiteSpace(description) ? "Card transaction" : description.Trim(),
                Amount = amount,
                Date = date
            });
        }

        [HttpPost("")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(ReportFormVM form)
        {
            var (report, errors, status) = await PostAsync<MyReportUIVM>("api/reports", new
            {
                transactionId = form.TransactionId,
                reason = form.Reason
            });
            if (status == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }
            if (report is null)
            {
                form.Error = string.Join(" ", errors);
                return View("Index", form);
            }

            TempData["Success"] = "Thank you. Your report about " + report.TransactionReference + " was sent. We will look at it and tell you the result.";
            return RedirectToAction(nameof(List));
        }

        [HttpGet("List")]
        public async Task<IActionResult> List()
        {
            var (items, status) = await GetAsync<List<MyReportUIVM>>("api/reports");
            if (status == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }
            return View(new MyReportsPageVM { Items = items ?? new List<MyReportUIVM>(), Unavailable = items is null });
        }
    }
}
