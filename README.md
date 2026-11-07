# Caspian Bank: MVC (müştəri saytı və admin panel)

Bu repo Caspian Bank layihəsinin **ön tərəfidir** (ASP.NET Core 8 MVC). Bütün məlumat və iş qaydaları API repo-sundadır:
**`CaspianBank-API-FinalProject`**. Qurulum, bazanın importu, giriş məlumatları və yoxlama ssenarisi oradakı `README.md` faylındadır.

Qısa: iki repo-nu yan-yana klonlayın, bazanı import edin və API repo-sunda

```powershell
powershell -ExecutionPolicy Bypass -File scripts\start.ps1
```

yazın. Sayt `https://localhost:7082` ünvanında açılır. MVC API ünvanını `CaspianBank-MVC-FinalProject/appsettings.json` faylındakı `ApiSettings:BaseUrl` (`https://localhost:7293/`) açarından oxuyur.

Qovluqlar: `Controllers` (müştəri), `Areas/Admin` (işçi paneli), `Views`, `ViewModels`, `wwwroot` (CSS və JS).
