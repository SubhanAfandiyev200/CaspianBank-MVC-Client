using CaspianBank_MVC_FinalProject.Handlers;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<BearerTokenHandler>();

// API-yə gedən sorğulara cookie-dəki JWT avtomatik əlavə olunur
builder.Services.AddHttpClient("CaspianApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]!);
})
.AddHttpMessageHandler<BearerTokenHandler>()
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();

    // Lokal işləmədə API-nin HTTPS dev sertifikatı bu kompüterdə etibarlı sayılmaya bilər (başqa kompüter, yeni quraşdırma).
    // Yalnız Development-də və yalnız lokal (localhost) ünvana qarşı yoxlama keçilir.
    if (builder.Environment.IsDevelopment())
    {
        handler.ServerCertificateCustomValidationCallback = (request, _, _, errors) =>
            errors == System.Net.Security.SslPolicyErrors.None || request.RequestUri?.IsLoopback == true;
    }

    return handler;
});

// Giriş cookie-dədir (JWT onun içində, şifrələnmiş saxlanır; JS tokeni görə bilmir)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Welcome"; // giriş tələb edən səhifə: əvvəl email (Welcome), mövcuddursa Login
        options.AccessDeniedPath = "/";
        options.Cookie.Name = "Caspian.Auth";
        options.Cookie.HttpOnly = true;
        // Production-da yalnız HTTPS; lokal işləmədə HTTP profili ilə də giriş işləsin
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.SlidingExpiration = false; // cookie müddəti JWT-nin bitmə vaxtı ilə eynidir
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseStaticFiles(new StaticFileOptions
    {
        OnPrepareResponse = ctx =>
        {
            ctx.Context.Response.Headers["Cache-Control"] = "no-cache, no-store";
            ctx.Context.Response.Headers["Pragma"] = "no-cache";
        }
    });
}
else
{
    app.UseStaticFiles();
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Admin Area: /Admin -> BankDashboard/Index, /Admin/Users/Details/u1 və s. (default route-dan ƏVVƏL olmalıdır)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=BankDashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
