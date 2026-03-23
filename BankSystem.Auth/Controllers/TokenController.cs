using Microsoft.AspNetCore.Mvc;

namespace BankSystem.Auth.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TokenController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public TokenController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> GetToken([FromBody] TokenRequest request)
        {
            var client = _httpClientFactory.CreateClient();

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var formData = new Dictionary<string, string>
            {
                ["grant_type"]    = "password",
                ["client_id"]     = "bank.client",
                ["client_secret"] = "secret",
                ["username"]      = request.Username,
                ["password"]      = request.Password,
                ["scope"]         = "openid profile email bank.api roles"
            };

            var response = await client.PostAsync(
                $"{baseUrl}/connect/token",
                new FormUrlEncodedContent(formData));

            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, body);

            // Extract access_token and return as plain string for backwards compatibility
            var json = System.Text.Json.JsonDocument.Parse(body);
            if (json.RootElement.TryGetProperty("access_token", out var tokenEl))
                return Ok(tokenEl.GetString());

            return Ok(body);
        }
    }

    public class TokenRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
