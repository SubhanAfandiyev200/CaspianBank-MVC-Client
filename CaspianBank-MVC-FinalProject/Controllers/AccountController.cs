using CaspianBank_MVC_FinalProject.ViewModels.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using CaspianBank_MVC_FinalProject.Helpers;
using System.Security.Claims;
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
        public IActionResult Welcome()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return LocalRedirect(RoleRedirect.HomeUrl(User.FindAll(ClaimTypes.Role).Select(claim => claim.Value)));
            }

            // Geri düyməsi ilə qayıdanda yazılan email yenidən dolu gəlsin
            return View(new WelcomeVM
            {
                Email = TempData.Peek("AuthEmail") as string ?? string.Empty
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Welcome(WelcomeVM model)
        {
            var email = model.Email?.Trim() ?? string.Empty;
            if (email.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Enter your email.");
                return View(model);
            }

            try
            {
                var client = _httpClientFactory.CreateClient("CaspianApi");
                var response = await client.PostAsJsonAsync("api/account/check-email", new { email });

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    ModelState.AddModelError(string.Empty, "Too many attempts. Please wait a minute and try again.");
                    return View(model);
                }

                var result = await ReadJsonAsync<CheckEmailResponseVM>(response);
                if (!response.IsSuccessStatusCode || result is null || !result.IsSuccess)
                {
                    var errors = result?.Errors is { Length: > 0 }
                        ? result.Errors
                        : new[] { "Could not check the email. Please try again." };

                    foreach (var error in errors)
                        ModelState.AddModelError(string.Empty, error);

                    return View(model);
                }

                // Email növbəti səhifədə hazır dolsun (URL-ə yazılmır)
                TempData["AuthEmail"] = email;

                if (result.Exists)
                    return RedirectToAction(nameof(Login));

                // Yeni email: Register-dən əvvəl emailin sahibi olduğunu OTP kodu ilə təsdiqləyir
                var otp = await SendOtpAsync(client, email);
                if (otp.Error is not null)
                {
                    ModelState.AddModelError(string.Empty, otp.Error);
                    return View(model);
                }

                TempData["Info"] = otp.Info;
                SetDevCode(otp.DevCode);
                MarkResendAvailable(otp.WaitSeconds);
                return RedirectToAction(nameof(VerifyOtp));
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "The service is unavailable right now. Please try again later.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult VerifyOtp()
        {
            var email = TempData.Peek("AuthEmail") as string;
            if (string.IsNullOrEmpty(email))
                return RedirectToAction(nameof(Welcome));

            return View(new VerifyOtpVM
            {
                Email = email,
                MaskedEmail = MaskEmail(email),
                DevCode = TempData.Peek("DevCode") as string,
                ResendWaitSeconds = RemainingResendSeconds()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(VerifyOtpVM model)
        {
            model.MaskedEmail = MaskEmail(model.Email);
            model.DevCode = TempData.Peek("DevCode") as string;
            // Səhv kod yazılanda səhifə yenidən açılır: geri sayım sıfırdan yox, qalan vaxtdan davam etsin
            model.ResendWaitSeconds = RemainingResendSeconds();
            var code = model.Code ?? string.Empty;

            if (!Regex.IsMatch(code, @"^\d{6}$"))
            {
                ModelState.AddModelError(string.Empty, "Enter the 6-digit code.");
                return View(model);
            }

            try
            {
                var client = _httpClientFactory.CreateClient("CaspianApi");
                var response = await client.PostAsJsonAsync("api/account/verify-otp", new { email = model.Email, code });

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    ModelState.AddModelError(string.Empty, "Too many attempts. Please wait a minute and try again.");
                    return View(model);
                }

                var result = await ReadJsonAsync<VerifyOtpResponseVM>(response);
                if (!response.IsSuccessStatusCode || result is null || !result.IsSuccess || string.IsNullOrEmpty(result.VerificationToken))
                {
                    var errors = result?.Errors is { Length: > 0 }
                        ? result.Errors
                        : new[] { "Could not verify the code. Please try again." };

                    foreach (var error in errors)
                        ModelState.AddModelError(string.Empty, error);

                    return View(model);
                }

                // Email təsdiqləndi: Register bu tokeni API-yə göndərəcək
                TempData["AuthEmail"] = model.Email;
                TempData["VerificationToken"] = result.VerificationToken;
                TempData.Remove("DevCode");
                return RedirectToAction(nameof(Register));
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "The service is unavailable right now. Please try again later.");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendOtp(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return RedirectToAction(nameof(Welcome));

            email = email.Trim();
            TempData["AuthEmail"] = email;

            try
            {
                var client = _httpClientFactory.CreateClient("CaspianApi");
                var otp = await SendOtpAsync(client, email);
                TempData["Info"] = otp.Error ?? otp.Info;
                SetDevCode(otp.DevCode);
                // Yeni kod göndərildisə geri sayım 60 saniyədən başlayır; cooldown səbəbilə göndərilməyibsə API-nin dediyi qalan vaxtdan davam edir
                if (otp.Error is null)
                {
                    MarkResendAvailable(otp.WaitSeconds);
                }
            }
            catch (HttpRequestException)
            {
                TempData["Info"] = "The service is unavailable right now. Please try again later.";
            }

            return RedirectToAction(nameof(VerifyOtp));
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return LocalRedirect(RoleRedirect.HomeUrl(User.FindAll(ClaimTypes.Role).Select(claim => claim.Value)));
            }

            return View(new ForgotPasswordVM
            {
                Email = TempData.Peek("AuthEmail") as string ?? string.Empty
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordVM model)
        {
            var email = model.Email?.Trim() ?? string.Empty;
            if (email.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Enter your email.");
                return View(model);
            }

            try
            {
                var client = _httpClientFactory.CreateClient("CaspianApi");
                var response = await client.PostAsJsonAsync("api/account/forgot-password", new { email });

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    ModelState.AddModelError(string.Empty, "Too many attempts. Please wait a minute and try again.");
                    return View(model);
                }

                var result = await ReadJsonAsync<OperationResponseVM>(response);
                if (!response.IsSuccessStatusCode || result is null || !result.IsSuccess)
                {
                    var errors = result?.Errors is { Length: > 0 }
                        ? result.Errors
                        : new[] { "Could not send the reset link. Please try again." };

                    foreach (var error in errors)
                        ModelState.AddModelError(string.Empty, error);

                    return View(model);
                }

                // Email olsa da olmasa da eyni cavab göstərilir
                model.Email = email;
                model.MaskedEmail = MaskEmail(email);
                model.Sent = true;
                model.DevLink = result.DevLink;
                return View(model);
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "The service is unavailable right now. Please try again later.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult ResetPassword(string? email, string? token)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
            {
                TempData["Info"] = "That reset link is not valid. Request a new one below.";
                return RedirectToAction(nameof(ForgotPassword));
            }

            return View(new ResetPasswordVM
            {
                Email = email,
                Token = token
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM model)
        {
            var password = model.Password;
            var confirm = model.ConfirmPassword;
            model.Password = string.Empty;       // şifrələr forma geri qaytarılmasın
            model.ConfirmPassword = string.Empty;

            if (password != confirm)
            {
                ModelState.AddModelError(string.Empty, "Passwords do not match.");
                return View(model);
            }

            try
            {
                var client = _httpClientFactory.CreateClient("CaspianApi");
                var response = await client.PostAsJsonAsync("api/account/reset-password", new
                {
                    email = model.Email,
                    token = model.Token,
                    newPassword = password
                });

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    ModelState.AddModelError(string.Empty, "Too many attempts. Please wait a minute and try again.");
                    return View(model);
                }

                var result = await ReadJsonAsync<OperationResponseVM>(response);
                if (!response.IsSuccessStatusCode || result is null || !result.IsSuccess)
                {
                    var errors = result?.Errors is { Length: > 0 }
                        ? result.Errors
                        : new[] { "Could not update the password. Please try again." };

                    foreach (var error in errors)
                        ModelState.AddModelError(string.Empty, error);

                    return View(model);
                }

                TempData["AuthEmail"] = model.Email;
                TempData["Info"] = "Your password has been updated. Log in with the new password.";
                return RedirectToAction(nameof(Login));
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "The service is unavailable right now. Please try again later.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            // Email OTP ilə təsdiqlənməyibsə Register açılmır
            var email = TempData.Peek("AuthEmail") as string;
            var token = TempData.Peek("VerificationToken") as string;
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
                return RedirectToAction(nameof(Welcome));

            return View(new RegisterVM
            {
                Email = email,
                VerificationToken = token
            });
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
                password = model.Password,
                verificationToken = model.VerificationToken
            };

            try
            {
                var client = _httpClientFactory.CreateClient("CaspianApi");
                var response = await client.PostAsJsonAsync("api/account/register", request);

                if (response.IsSuccessStatusCode)
                {
                    // Login səhifəsi emailə hazır açılsın; təsdiq tokeni artıq istifadə olunub
                    TempData.Remove("VerificationToken");
                    TempData["AuthEmail"] = model.Email;
                    TempData["Info"] = "Your account is ready. Log in to continue.";
                    return RedirectToAction(nameof(Login));
                }

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
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return LocalRedirect(RoleRedirect.HomeUrl(User.FindAll(ClaimTypes.Role).Select(claim => claim.Value)));
            }

            return View(new LoginVM
            {
                ReturnUrl = returnUrl,
                Email = TempData["AuthEmail"] as string ?? string.Empty
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM model)
        {
            var password = model.Password;
            model.Password = string.Empty; // şifrə forma geri qaytarılmasın

            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError(string.Empty, "Enter your email and password.");
                return View(model);
            }

            try
            {
                var client = _httpClientFactory.CreateClient("CaspianApi");
                var response = await client.PostAsJsonAsync("api/account/login", new
                {
                    email = model.Email.Trim(),
                    password
                });

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    ModelState.AddModelError(string.Empty, "Too many attempts. Please wait a minute and try again.");
                    return View(model);
                }

                var result = await ReadJsonAsync<LoginResponseVM>(response);

                if (!response.IsSuccessStatusCode || result is null || !result.IsSuccess || string.IsNullOrEmpty(result.Token))
                {
                    var errors = result?.Errors is { Length: > 0 }
                        ? result.Errors
                        : new[] { "Could not log in. Check your details and try again." };

                    foreach (var error in errors)
                        ModelState.AddModelError(string.Empty, error);

                    return View(model);
                }

                await SignInAsync(result);

                // Əvvəl getmək istədiyi yerə (ReturnUrl), yoxdursa rola görə: işçi admin panelinə, müştəri kartlarına
                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return LocalRedirect(model.ReturnUrl);
                }

                return LocalRedirect(RoleRedirect.HomeUrl(result.Roles));
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "The service is unavailable right now. Please try again later.");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // API-yə OTP göndərmə sorğusu. Error: istifadəçiyə göstəriləcək xəta, Info: məlumat mesajı,
        // WaitSeconds: "Resend code" düyməsinin açılmasına qalan saniyə (yeni kod göndərilibsə 60, cooldown-dursa API-nin dediyi qalan vaxt)
        private static async Task<(string? Error, string? Info, string? DevCode, int WaitSeconds)> SendOtpAsync(HttpClient client, string email)
        {
            var response = await client.PostAsJsonAsync("api/account/send-otp", new { email });
            var result = await ReadJsonAsync<SendOtpResponseVM>(response);

            if (response.IsSuccessStatusCode)
                return (null, "We sent a code to your email.", result?.DevCode, ResendWaitTotalSeconds);

            // Eyni email-ə az əvvəl kod göndərilib (gözləmə müddəti): əvvəlki kod hələ etibarlıdır
            if (response.StatusCode == HttpStatusCode.TooManyRequests && result?.RetryAfterSeconds > 0)
                return (null, $"A code was sent a moment ago. You can request a new one in {result.RetryAfterSeconds} seconds.", null, result.RetryAfterSeconds);

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
                return ("Too many attempts. Please wait a minute and try again.", null, null, 0);

            return (result?.Errors is { Length: > 0 } ? result.Errors[0] : "We could not send the code. Please try again.", null, null, 0);
        }

        // "Resend code" üçün gözləmə müddəti (API-nin cooldown-u ilə eyni, saniyə)
        private const int ResendWaitTotalSeconds = 60;

        // Düymənin nə vaxt açılacağı yadda saxlanır ki, səhv kod yazılıb səhifə yenilənəndə geri sayım sıfırlanmasın
        private void MarkResendAvailable(int waitSeconds)
        {
            TempData["OtpResendAt"] = DateTime.UtcNow.AddSeconds(waitSeconds).Ticks.ToString();
        }

        // Peek: oxunmuş sayılmır, ona görə səhv kod yazılıb səhifə yenidən açılanda da qalır
        private int RemainingResendSeconds()
        {
            if (TempData.Peek("OtpResendAt") is string stored && long.TryParse(stored, out var ticks))
            {
                var left = (int)Math.Ceiling((new DateTime(ticks, DateTimeKind.Utc) - DateTime.UtcNow).TotalSeconds);
                return Math.Clamp(left, 0, ResendWaitTotalSeconds);
            }

            return ResendWaitTotalSeconds;
        }

        // API-də SMTP qurulmayıbsa (Development) kod ekranda göstərilir; yoxdursa köhnə kod silinir
        private void SetDevCode(string? code)
        {
            if (string.IsNullOrEmpty(code)) TempData.Remove("DevCode");
            else TempData["DevCode"] = code;
        }

        private static string MaskEmail(string email)
        {
            var parts = email.Split('@');
            return parts.Length == 2 && parts[0].Length > 0 ? parts[0][..1] + "****@" + parts[1] : email;
        }

        private async Task SignInAsync(LoginResponseVM login)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, login.UserId ?? string.Empty),
                new(ClaimTypes.Email, login.Email ?? string.Empty),
                new(ClaimTypes.Name, login.FullName ?? login.Email ?? string.Empty)
            };
            claims.AddRange(login.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var principal = new ClaimsPrincipal(
                new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

            var properties = new AuthenticationProperties
            {
                IsPersistent = false,                  // brauzer bağlananda giriş bitsin
                AllowRefresh = false,
                ExpiresUtc = login.ExpiresAt           // cookie JWT ilə eyni vaxtda bitir
            };

            // JWT cookie-nin içində (şifrələnmiş) saxlanır; BearerTokenHandler onu oradan oxuyur
            properties.StoreTokens(new[]
            {
                new AuthenticationToken
                {
                    Name = "access_token",
                    Value = login.Token!
                }
            });

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);
        }

        private static async Task<RegisterResponseVM?> ReadResponseAsync(HttpResponseMessage response)
            => await ReadJsonAsync<RegisterResponseVM>(response);

        private static async Task<T?> ReadJsonAsync<T>(HttpResponseMessage response) where T : class
        {
            try
            {
                return await response.Content.ReadFromJsonAsync<T>();
            }
            catch (JsonException)
            {
                return null; // API 400-ü başqa formatda qaytarıbsa (məs. model binding xətası)
            }
        }
    }
}
