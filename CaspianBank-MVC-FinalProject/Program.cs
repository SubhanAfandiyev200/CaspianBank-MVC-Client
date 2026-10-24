using CaspianBank_MVC_FinalProject.Handlers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Jurnal (Serilog): konsola və layihə qovluğundakı logs/ qovluğuna (gündə bir fayl, 14 gün). Səviyyələr appsettings.json-dakı "Serilog" bölməsindədir
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        Path.Combine(context.HostingEnvironment.ContentRootPath, "logs", "mvc-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14,
        shared: true,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"));

// Add services to the container.
builder.Services.AddControllersWithViews(options => options.Filters.Add<CaspianBank_MVC_FinalProject.Helpers.TranslateModelErrorsFilter>());

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<BearerTokenHandler>();

// Saytın dilləri: tərcümələr verilənlər bazasından (API) gəlir, yaddaşda saxlanılır; səhifələrdə @T["English text"]
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<CaspianBank_MVC_FinalProject.Services.ITranslationStore, CaspianBank_MVC_FinalProject.Services.TranslationStore>();
builder.Services.AddScoped<CaspianBank_MVC_FinalProject.Services.ITranslator, CaspianBank_MVC_FinalProject.Services.Translator>();

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
        options.LoginPath = "/Account/Login"; // giriş tələb edən səhifə
        options.AccessDeniedPath = "/Home/AccessDenied";
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

// 404, 403 və s. (məs. mövcud olmayan id) boş səhifə əvəzinə xəta səhifəsini göstərsin
app.UseStatusCodePagesWithReExecute("/Home/Error", "?code={0}");

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

// Hər sorğu bir sətirdə: metod, yol, status, müddət, giriş edənin adı yox, id-si
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("UserId", httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "anonymous");
    };
});

// Hər sorğuda dil seçilir (cookie, brauzerin dili) və tərcümə lüğəti sorğuya qoyulur
app.UseMiddleware<CaspianBank_MVC_FinalProject.Services.LanguageMiddleware>();

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
