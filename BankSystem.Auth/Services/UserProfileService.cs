using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BankSystem.Auth.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserProfileService> _logger;

        public UserProfileService(HttpClient httpClient, ILogger<UserProfileService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<UserProfile?> GetUserProfileAsync(string email)
        {
            try
            {
                _logger.LogInformation("Getting user profile for email: {Email}", email);

                var response = await _httpClient.GetAsync($"/api/users/email/{email}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get user profile for email {Email}: {StatusCode}", email, response.StatusCode);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var userDto = JsonSerializer.Deserialize<UserDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (userDto != null)
                {
                    return new UserProfile
                    {
                        Id = userDto.Id,
                        FirstName = userDto.FirstName,
                        LastName = userDto.LastName,
                        Email = userDto.Email,
                        Role = userDto.Role,
                        IsActive = userDto.IsActive
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile for email {Email}", email);
                return null;
            }
        }

        public async Task<UserProfile?> GetUserProfileByIdAsync(Guid userId)
        {
            try
            {
                _logger.LogInformation("Getting user profile for id: {UserId}", userId);

                var response = await _httpClient.GetAsync($"/api/users/internal/{userId}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get user profile for id {UserId}: {StatusCode}", userId, response.StatusCode);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var userDto = JsonSerializer.Deserialize<UserDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (userDto != null)
                {
                    return new UserProfile
                    {
                        Id = userDto.Id,
                        FirstName = userDto.FirstName,
                        LastName = userDto.LastName,
                        Email = userDto.Email,
                        Role = userDto.Role,
                        IsActive = userDto.IsActive
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile for id {UserId}", userId);
                return null;
            }
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            var user = await GetUserProfileAsync(email);
            return user != null;
        }

        private class UserDto
        {
            public Guid Id { get; set; }
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
            public bool IsActive { get; set; }
        }
    }
}