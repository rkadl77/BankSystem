using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BankSystem.Services
{
    public class UserRoleService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserRoleService> _logger;

        public UserRoleService(HttpClient httpClient, ILogger<UserRoleService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpClient.BaseAddress = new Uri("http://localhost:5002");
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
        }

        public async Task<string?> GetUserRoleAsync(Guid userId)
        {
            try
            {
                Console.WriteLine($"Getting role for user: {userId}");
                var response = await _httpClient.GetAsync($"/api/users/{userId}");
                Console.WriteLine($"Response status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Response body: {json}");
                    var user = JsonSerializer.Deserialize<UserDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return user?.Role;
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user role for {UserId}", userId);
                Console.WriteLine($"Exception: {ex.Message}");
                return null;
            }
        }

        private class UserDto
        {
            public Guid Id { get; set; }
            public string Role { get; set; } = string.Empty;
        }
    }
}