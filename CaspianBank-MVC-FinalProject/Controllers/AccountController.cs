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
        private readonly IConfiguration _configuration;

        public AccountController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // Köhnə "/Account/Welcome" ünvanı (bookmark, köhnə link): artıq ayrıca səhifə yoxdur, giriş səhifəsinə yönləndirilir
        [HttpGet]
        public IActionResult Welcome()
        {
            return RedirectToAction(nameof(Login));
        }

        // Footer-dakı "email yaz və davam et" formu: email sistemdə varsa Login-ə (email hazır dolu gəlir),
        // yoxdursa qeydiyyata (kimlik formu) aparır. Ayrıca səhifəsi yoxdur: xəta olarsa Login-də mesaj göstərilir
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Start(string? email)
        {
            email = email?.Trim() ?? string.Empty;
            if (email.Length == 0)
            {
                TempData["Info"] = "Enter your email.";
                return RedirectToAction(nameof(Login));
            }

            try
            {
                var client = _httpClientFactory.CreateClient("CaspianApi");
                var response = await client.PostAsJsonAsync("api/account/check-email", new { email });

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    TempData["Info"] = "Too many attempts. Please wait a minute and try again.";
                    return RedirectToAction(nameof(Login));
                }

                var result = await ReadJsonAsync<CheckEmailResponseVM>(response);
                if (!response.IsSuccessStatusCode || result is null || !result.IsSuccess)
                {
                    TempData["Info"] = result?.Errors is { Length: > 0 }
                        ? result.Errors[0]
                        : "Could not check the email. Please try again.";
                    return RedirectToAction(nameof(Login));
                }

                // Email növbəti səhifədə hazır dolsun (URL-ə yazılmır)
                TempData["AuthEmail"] = email;

                if (result.Exists)
                    return RedirectToAction(nameof(Login));

                // Yeni email: qeydiyyat kimlikdən başlayır (FİN, vəsiqə, ad, soyad). Email yadda qalır, kimlik təsdiqlənəndən sonra ona OTP göndərilir
                TempData["RegEmail"] = email;
                return RedirectToAction(nameof(Register));
            }
            catch (HttpRequestException)
            {
                TempData["Info"] = "The service is unavailable right now. Please try again later.";
                return RedirectToAction(nameof(Login));
            }
        }

        [HttpGet]
        public IActionResult VerifyOtp()
        {
            var email = TempData.Peek("AuthEmail") as string;
            if (string.IsNullOrEmpty(email))
                return RedirectToAction(nameof(Register));

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

                // Email təsdiqləndi: kimlik artıq yoxlanılıb, qalan telefon və şifrədir. CreateAccount bu tokeni API-yə göndərəcək
                TempData["AuthEmail"] = model.Email;
                TempData["VerificationToken"] = result.VerificationToken;
                TempData.Remove("DevCode");
                return RedirectToAction(nameof(CreateAccount));
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
                return RedirectToAction(nameof(Register));

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

        // Qeydiyyat kimlikdən başlayır, "Open account" düyməsi birbaşa buraya gətirir (login olmayanlar üçün):
        //   1) Register: FİN, vəsiqə nömrəsi, ad, soyad reyestrdə yoxlanılır (uyğun gəlməsə "tapılmadı")
        //   2) ConfirmIdentity: "Bu sizsiniz?" (şəkil, ad, soyadın baş hərfi, doğum ili)
        //   3) RegisterEmail (footer-dakı email formundan gəlibsə email artıq məlumdur, bu addım keçilir) və VerifyOtp: emailə 6 rəqəmli kod
        //   4) CreateAccount: telefon + şifrə, hesab açılır, ilk kartlar avtomatik yaranır
        [HttpGet]
        public IActionResult Register()
        {
            // Hesabı olan və daxil olan adam yenidən qeydiyyatdan keçmir
            if (User.Identity?.IsAuthenticated == true)
            {
                return LocalRedirect(RoleRedirect.HomeUrl(User.FindAll(ClaimTypes.Role).Select(claim => claim.Value)));
            }

            return View(new IdentityVM
            {
                Fin = TempData.Peek("RegFin") as string ?? string.Empty,
                IdCardNumber = TempData.Peek("RegIdCard") as string ?? string.Empty,
                Name = TempData.Peek("RegFirst") as string ?? string.Empty,
                Surname = TempData.Peek("RegLast") as string ?? string.Empty
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(IdentityVM model)
        {
            var fin = model.Fin?.Trim().ToUpperInvariant() ?? string.Empty;
            var idCard = model.IdCardNumber?.Trim().ToUpperInvariant() ?? string.Empty;
            var name = model.Name?.Trim() ?? string.Empty;
            var surname = model.Surname?.Trim() ?? string.Empty;
            model.Fin = fin;
            model.IdCardNumber = idCard;
            model.Name = name;
            model.Surname = surname;

            try
            {
                var client = _httpClientFactory.CreateClient("CaspianApi");
                var response = await client.PostAsJsonAsync("api/account/check-identity", new { fin, idCardNumber = idCard, name, surname });

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    ModelState.AddModelError(string.Empty, "Too many attempts. Please wait a minute and try again.");
                    return View(model);
                }

                var result = await ReadJsonAsync<IdentityResponseVM>(response);
                if (!response.IsSuccessStatusCode || result is null || !result.IsSuccess)
                {
                    var errors = result?.Errors is { Length: > 0 }
                        ? result.Errors
                        : new[] { "Could not check the details. Please try again." };

                    foreach (var error in errors)
                        ModelState.AddModelError(string.Empty, error);

                    return View(model);
                }

                // Növbəti səhifə "Bu sizsiniz?" göstərir (şəkil API-dən gəlir, ona görə tam ünvan lazımdır)
                TempData["RegFin"] = fin;
                TempData["RegIdCard"] = idCard;
                TempData["RegFirst"] = name;
                TempData["RegLast"] = surname;
                TempData["RegName"] = result.Name;
                TempData["RegSurname"] = result.MaskedSurname;
                TempData["RegYear"] = result.BirthYear.ToString();
                TempData["RegPhoto"] = ApiUrl.ToAbsolute(_configuration["ApiSettings:BaseUrl"], result.PhotoUrl);
                TempData.Remove("RegConfirmed");
                return RedirectToAction(nameof(ConfirmIdentity));
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "The service is unavailable right now. Please try again later.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult ConfirmIdentity()
        {
            if (TempData.Peek("RegFin") is not string fin || TempData.Peek("RegName") is not string name)
                return RedirectToAction(nameof(Register));

            return View(BuildConfirmModel(fin, name));
        }

        // "Yes, this is me": footer formundan gəlibsə email məlumdur və dərhal ona kod göndərilir, yoxsa əvvəl email soruşulur
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmIdentityYes()
        {
            if (TempData.Peek("RegFin") is not string fin || TempData.Peek("RegName") is not string name)
                return RedirectToAction(nameof(Register));

            TempData["RegConfirmed"] = "1";

            var email = TempData.Peek("RegEmail") as string;
            if (string.IsNullOrWhiteSpace(email))
                return RedirectToAction(nameof(RegisterEmail));

            try
            {
                var client = _httpClientFactory.CreateClient("CaspianApi");
                var error = await StartEmailVerificationAsync(client, email);
                if (error is null)
                    return RedirectToAction(nameof(VerifyOtp));

                ModelState.AddModelError(string.Empty, error);
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "The service is unavailable right now. Please try again later.");
            }

            return View("ConfirmIdentity", BuildConfirmModel(fin, name));
        }

        // Email soruşulur (Open account düyməsi ilə gələnlər üçün): email sistemdə varsa girişə yönləndirilir, yoxdursa koda keçir
        [HttpGet]
        public IActionResult RegisterEmail()
        {
            if (TempData.Peek("RegConfirmed") is not "1")
                return RedirectToAction(nameof(Register));

            return View(new RegisterEmailVM
            {
                Email = TempData.Peek("RegEmail") as string ?? string.Empty
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterEmail(RegisterEmailVM model)
        {
            if (TempData.Peek("RegConfirmed") is not "1")
                return RedirectToAction(nameof(Register));

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

                if (result.Exists)
                {
                    ModelState.AddModelError(string.Empty, "An account with this email already exists. Log in instead, or use a different email.");
                    return View(model);
                }

                TempData["RegEmail"] = email;
                var sendError = await StartEmailVerificationAsync(client, email);
                if (sendError is null)
                    return RedirectToAction(nameof(VerifyOtp));

                ModelState.AddModelError(string.Empty, sendError);
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "The service is unavailable right now. Please try again later.");
            }

            return View(model);
        }

        // Emailə 6 rəqəmli kod göndərir və VerifyOtp səhifəsi üçün lazım olan məlumatı TempData-ya yazır. Xəta olarsa mətni qaytarır, uğurda null
        private async Task<string?> StartEmailVerificationAsync(HttpClient client, string email)
        {
            var otp = await SendOtpAsync(client, email);
            if (otp.Error is not null)
                return otp.Error;

            TempData["AuthEmail"] = email;
            TempData["Info"] = otp.Info;
            SetDevCode(otp.DevCode);
            MarkResendAvailable(otp.WaitSeconds);
            return null;
        }

        private ConfirmIdentityVM BuildConfirmModel(string fin, string name)
        {
            int.TryParse(TempData.Peek("RegYear") as string, out var year);
            return new ConfirmIdentityVM
            {
                Fin = fin,
                Name = name,
                MaskedSurname = TempData.Peek("RegSurname") as string ?? string.Empty,
                BirthYear = year,
                PhotoUrl = TempData.Peek("RegPhoto") as string
            };
        }

        [HttpGet]
        public IActionResult CreateAccount()
        {
            // Əvvəl kimlik təsdiqlənməli, sonra email OTP ilə təsdiqlənməlidir
            if (TempData.Peek("RegConfirmed") is not "1" ||
                TempData.Peek("RegFin") is not string fin ||
                TempData.Peek("RegIdCard") is not string idCard ||
                TempData.Peek("RegFirst") is not string first ||
                TempData.Peek("RegLast") is not string last)
                return RedirectToAction(nameof(Register));

            var email = TempData.Peek("AuthEmail") as string;
            var token = TempData.Peek("VerificationToken") as string;
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
                return RedirectToAction(nameof(RegisterEmail));

            return View(new RegisterVM
            {
                Email = email,
                VerificationToken = token,
                Fin = fin,
                IdCardNumber = idCard,
                Name = first,
                Surname = last,
                HolderName = $"{TempData.Peek("RegName")} {TempData.Peek("RegSurname")}".Trim()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAccount(RegisterVM model)
        {
            model.HolderName = $"{TempData.Peek("RegName")} {TempData.Peek("RegSurname")}".Trim();

            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Passwords do not match.");
                return View(model);
            }

            var request = new
            {
                email = model.Email?.Trim(),
                phoneNumber = Regex.Replace(model.PhoneNumber ?? string.Empty, @"\D", ""),
                fin = model.Fin,
                idCardNumber = model.IdCardNumber,
                name = model.Name,
                surname = model.Surname,
                password = model.Password,
                verificationToken = model.VerificationToken
            };

            try
            {
                var client = _httpClientFactory.CreateClient("CaspianApi");
                var response = await client.PostAsJsonAsync("api/account/register", request);

                if (response.IsSuccessStatusCode)
                {
                    // Login səhifəsi emailə hazır açılsın; təsdiq tokeni və qeydiyyat addımlarının məlumatı artıq lazım deyil
                    TempData.Remove("VerificationToken");
                    foreach (var key in new[] { "RegFin", "RegIdCard", "RegFirst", "RegLast", "RegName", "RegSurname", "RegYear", "RegPhoto", "RegConfirmed", "RegEmail" })
                        TempData.Remove(key);
                    TempData["AuthEmail"] = model.Email;
                    TempData["Info"] = "Your account is ready, with your first two cards. Log in to continue.";
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
