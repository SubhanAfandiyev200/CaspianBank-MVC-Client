# Caspian Bank: MVC (UI)

Caspian Bank layihəsinin istifadəçi interfeysi (ASP.NET Core 8 MVC + Razor). Bütün məlumat və biznes məntiqi
**CaspianBank-API-FinalProject**-dədir; bu layihə yalnız UI-dır və API-ni `HttpClient` ilə çağırır.

## İşə salma
1. **Əvvəlcə API-ni** işə salın (`CaspianBank-API-FinalProject`, **`https`** profili, `https://localhost:7293`).
2. Sonra bu layihəni (`CaspianBank-MVC-FinalProject`) **`https`** profili ilə işə salın: `https://localhost:7082`.

API ünvanı `CaspianBank-MVC-FinalProject/appsettings.json` → `ApiSettings:BaseUrl`-dadır (`https://localhost:7293/`).
API işləmirsə, səhifələr açılır, amma dinamik hissələr (ticker, brendlər, giriş) görünmür.

## Qeydiyyat və giriş
`Open account` → `Welcome` (email). Email qurulmayıbsa **demo rejimi** işləyir: OTP kodu və şifrə bərpası linki
səhifənin özündə göstərilir (API-nin README-sinə bax).

## Qeydlər
- Giriş cookie-dədir; JWT onun içində şifrələnmiş saxlanır və API-yə hər sorğuda avtomatik göndərilir.
- Home səhifəsinin hissələri (ticker, About, brendlər) ViewComponent-lərlə API-dən gəlir.
