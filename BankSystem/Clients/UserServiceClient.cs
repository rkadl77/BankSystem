using System.Text.Json;

namespace BankSystem.Clients
{
    public class UserServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserServiceClient> _logger;

        public UserServiceClient(HttpClient httpClient, ILogger<UserServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpClient.BaseAddress = new Uri("http://localhost:5002");
        }

        public async Task<bool> UserExistsAsync(Guid userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/users/{userId}/exists");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking user existence for ID: {UserId}", userId);
                return false;
            }
        }
    }
}