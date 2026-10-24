using CaspianBank_MVC_FinalProject.Controllers;
using CaspianBank_MVC_FinalProject.Helpers;
using CaspianBank_MVC_FinalProject.ViewModels.Staff;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace CaspianBank_MVC_FinalProject.Areas.Admin.Controllers
{
    // BANK bölməsi: işçilər və rolları. Yalnız SuperAdmin görür və idarə edir (Admin rol verə/silə bilməz).
    // Bütün qaydalar API-dədir (SuperAdmin və öz hesabına toxunmaq olmaz, yalnız dörd rol verilir); burada yalnız siyahı və formalar var
    [Area("Admin")]
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public class StaffController : ApiControllerBase
    {
        public StaffController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var (model, unauthorized) = await LoadAsync(new StaffPageVM());
            return unauthorized ? await SessionExpiredAsync() : View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddStaffFormVM form)
        {
            var (staff, errors, status) = await PostAsync<StaffVM>("api/admin/staff", new
            {
                name = form.Name,
                surname = form.Surname,
                email = form.Email,
                role = form.Role
            });
            if (status == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }
            if (staff is null)
            {
                var (model, unauthorized) = await LoadAsync(new StaffPageVM { Form = form, Error = string.Join(" ", errors) });
                return unauthorized ? await SessionExpiredAsync() : View("Index", model);
            }

            TempData["Success"] = staff.Name + " " + staff.Surname + " was added. An email with a link to choose the password was sent to " + staff.Email + ".";
            if (!string.IsNullOrEmpty(staff.InviteLink))
            {
                // SMTP qurulmayıbsa (yalnız Development) link ekranda göstərilir
                TempData["InviteLink"] = staff.InviteLink;
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(string id, string role)
        {
            var (success, errors, status) = await PutAsync($"api/admin/staff/{Uri.EscapeDataString(id)}/role", new { role });
            return await FinishAsync(success, errors, status, "The role was changed. The person has to sign in again to get the new access.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(string id)
        {
            var (success, errors, status) = await PostAsync($"api/admin/staff/{Uri.EscapeDataString(id)}/deactivate", new { });
            return await FinishAsync(success, errors, status, "The account was switched off. The person is signed out and can't sign in.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(string id)
        {
            var (success, errors, status) = await PostAsync($"api/admin/staff/{Uri.EscapeDataString(id)}/activate", new { });
            return await FinishAsync(success, errors, status, "The account was switched on again.");
        }

        private async Task<IActionResult> FinishAsync(bool success, string[] errors, HttpStatusCode? status, string message)
        {
            if (status == HttpStatusCode.Unauthorized)
            {
                return await SessionExpiredAsync();
            }
            if (success)
            {
                TempData["Success"] = message;
            }
            else
            {
                TempData["Error"] = status == HttpStatusCode.NotFound ? "This staff member no longer exists." : string.Join(" ", errors);
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<(StaffPageVM Model, bool Unauthorized)> LoadAsync(StaffPageVM model)
        {
            var (list, status) = await GetAsync<StaffListVM>("api/admin/staff");
            if (status == HttpStatusCode.Unauthorized)
            {
                return (model, true);
            }
            model.Unavailable = list is null;
            model.List = list ?? new StaffListVM();
            model.CurrentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            return (model, false);
        }
    }
}
