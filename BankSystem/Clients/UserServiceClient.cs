using System.Text.Json;

namespace BankSystem.Clients
{
    public interface IUserServiceClient
    {
        Task<bool> UserExistsAsync(Guid userId);
    }

    public class UserServiceClient : IUserServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserServiceClient> _logger;

        public UserServiceClient(HttpClient httpClient, ILogger<UserServiceClient> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            var usersServiceUrl = configuration["Services:UsersService"] ?? "http://localhost:5276";
            _httpClient.BaseAddress = new Uri(usersServiceUrl);
        }

        public virtual async Task<bool> UserExistsAsync(Guid userId)
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