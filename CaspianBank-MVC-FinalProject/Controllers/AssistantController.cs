using CaspianBank_MVC_FinalProject.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace CaspianBank_MVC_FinalProject.Controllers
{
    // AI köməkçi (sağ aşağıdakı pəncərənin "AI" bölməsi). Yalnız daxil olmuş müştəri və işçi üçün; açar API-dədir, brauzerə çatmır
    [Authorize]
    [Route("Assistant")]
    public class AssistantController : ApiControllerBase
    {
        public AssistantController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            : base(httpClientFactory, configuration)
        {
        }

        [HttpPost("Ask")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ask(string? message, string? history)
        {
            object turns = Array.Empty<object>();
            if (!string.IsNullOrWhiteSpace(history))
            {
                try
                {
                    turns = JsonSerializer.Deserialize<List<Turn>>(history, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Turn>();
                }
                catch (JsonException)
                {
                    return BadRequest(new { errors = TranslateErrors(new[] { "The conversation is not valid. Start a new chat." }) });
                }
            }

            var (data, errors, status) = await PostAsync<AssistantReply>("api/assistant", new { message, history = turns });
            if (data is null)
            {
                if (status == HttpStatusCode.Unauthorized)
                {
                    return Unauthorized();
                }
                return BadRequest(new { errors = TranslateErrors(errors.Length > 0 ? errors : new[] { UnavailableMessage }) });
            }
            return Ok(new { reply = TranslateErrors(new[] { data.Reply })[0], isConfigured = data.IsConfigured });
        }

        private sealed class Turn
        {
            public string? Role { get; set; }
            public string? Text { get; set; }
        }

        private sealed class AssistantReply
        {
            public string Reply { get; set; } = string.Empty;
            public bool IsConfigured { get; set; }
        }
    }
}
