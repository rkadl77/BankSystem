using System;
using System.Net.Http;
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
            _httpClient.BaseAddress = new Uri("http://localhost:5002"); // UsersService
        }

        public async Task<UserProfile?> GetUserProfileAsync(string email)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/users/email/{email}");
                if (response.IsSuccessStatusCode)
                {
                    var userDto = await response.Content.ReadFromJsonAsync<UserDto>();
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
                var response = await _httpClient.GetAsync($"/api/users/{userId}");
                if (response.IsSuccessStatusCode)
                {
                    var userDto = await response.Content.ReadFromJsonAsync<UserDto>();
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