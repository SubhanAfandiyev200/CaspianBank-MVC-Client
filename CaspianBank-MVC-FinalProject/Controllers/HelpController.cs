using CaspianBank_MVC_FinalProject.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;

namespace CaspianBank_MVC_FinalProject.Controllers
{
    // Hər səhifədəki "Help" pəncərəsi (chat-widget.js bura göndərir). Giriş etməyən ziyarətçi də yaza bilər: email yazır, cavab ora gedir.
    // Giriş etmiş müştəridə email soruşulmur, cavab bildirişlərinə və "Contact us" yazışmasına düşür. Qaydalar API-dədir (api/help)
    [AllowAnonymous]
    [Route("Help")]
    public class HelpController : ApiControllerBase
    {
        public HelpController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        [HttpPost("Send")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(string? email, string? message, IFormFile? image)
        {
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(email ?? string.Empty), "Email");
            content.Add(new StringContent(message ?? string.Empty), "Message");

            using var stream = image?.OpenReadStream();
            if (image is not null && stream is not null)
            {
                var file = new StreamContent(stream);
                file.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrWhiteSpace(image.ContentType) ? "application/octet-stream" : image.ContentType);
                content.Add(file, "Image", image.FileName);
            }

            var (data, errors, status) = await PostFormAsync<HelpSent>("api/help", content);
            if (data is null)
            {
                return BadRequest(new { errors = TranslateErrors(errors.Length > 0 ? errors : new[] { status == HttpStatusCode.Unauthorized ? "Please sign in again." : UnavailableMessage }) });
            }
            return Ok(new { isGuest = data.IsGuest, ticketId = data.TicketId });
        }

        // Pəncərədəki "Komandaya yaz" söhbəti: müştərinin yazdıqları və işçinin cavabları (şəkillər tam ünvanla). Giriş və rolu API yoxlayır
        [HttpGet("Conversation")]
        public async Task<IActionResult> Conversation()
        {
            var (data, status) = await GetAsync<ConversationData>("api/help/conversation");
            if (data is null)
            {
                return status == HttpStatusCode.Unauthorized ? Unauthorized() : StatusCode(StatusCodes.Status503ServiceUnavailable);
            }

            var baseUrl = Configuration["ApiSettings:BaseUrl"];
            return Ok(data.Messages.Select(m => new
            {
                isStaff = m.IsStaff,
                body = m.Body,
                imageUrl = string.IsNullOrEmpty(m.ImageUrl) ? null : ApiUrl.ToAbsolute(baseUrl, m.ImageUrl)
            }));
        }

        private sealed class ConversationData
        {
            public List<ConversationMessage> Messages { get; set; } = new();
        }

        private sealed class ConversationMessage
        {
            public bool IsStaff { get; set; }
            public string Body { get; set; } = string.Empty;
            public string? ImageUrl { get; set; }
        }

        private sealed class HelpSent
        {
            public int? TicketId { get; set; }
            public bool IsGuest { get; set; }
        }
    }
}
