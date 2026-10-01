using CaspianBank_MVC_FinalProject.ViewModels.Account;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CaspianBank_MVC_FinalProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (model.BirthDay is null)
            {
                ModelState.AddModelError(string.Empty, "Enter your date of birth.");
                return View(model);
            }

            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Passwords do not match.");
                return View(model);
            }

            var request = new
            {
                email = model.Email?.Trim(),
                phoneNumber = Regex.Replace(model.PhoneNumber ?? string.Empty, @"\D", ""), 
                name = model.Name?.Trim(),
                surname = model.Surname?.Trim(),
                birthDay = model.BirthDay,
                password = model.Password
            };

            try
            {
                var client = _httpClientFactory.CreateClient("CaspianApi");
                var response = await client.PostAsJsonAsync("api/account/register", request);

                if (response.IsSuccessStatusCode)
                    return RedirectToAction(nameof(Login));

                var result = await ReadResponseAsync(response);
                var errors = result?.Errors is { Length: > 0 }
                    ? result.Errors
                    : new[] { "Could not create the account. Check the details and try again." };

                foreach (var error in errors)
                    ModelState.AddModelError(string.Empty, error);
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "The service is unavailable right now. Please try again later.");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        private static async Task<RegisterResponseVM?> ReadResponseAsync(HttpResponseMessage response)
        {
            try
            {
                return await response.Content.ReadFromJsonAsync<RegisterResponseVM>();
            }
            catch (JsonException)
            {
                return null; // API 400-ü başqa formatda qaytarıbsa (məs. model binding xətası)
            }
        }
    }
}
