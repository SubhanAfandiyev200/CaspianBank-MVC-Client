using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;

namespace CaspianBank_MVC_FinalProject.Handlers
{
    // API-yə gedən hər sorğuya:
    //  - istifadəçi daxil olubsa, cookie-də saxlanmış JWT-ni Authorization başlığı kimi əlavə edir
    //  - real istifadəçi IP-sini X-Forwarded-For ilə ötürür (API-nin rate limiting-i IP-yə görə işləyir)
    public class BearerTokenHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BearerTokenHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context is not null)
            {
                var clientIp = context.Connection.RemoteIpAddress?.ToString();
                if (!string.IsNullOrEmpty(clientIp))
                    request.Headers.TryAddWithoutValidation("X-Forwarded-For", clientIp);

                var token = await context.GetTokenAsync("access_token");
                if (!string.IsNullOrEmpty(token))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
