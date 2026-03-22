using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BankSystem.Auth.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly ILogger<UserProfileService> _logger;

        public UserProfileService(ILogger<UserProfileService> logger)
        {
            _logger = logger;
        }

        public async Task<UserProfile?> GetUserProfileAsync(string email)
        {
            try
            {
                _logger.LogInformation($"Getting user profile for email: {email}");

                var tempFile = Path.GetTempFileName() + ".json";
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "curl.exe",
                        Arguments = $"-X GET http://127.0.0.1:5002/api/users/email/{email}",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                var response = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();

                _logger.LogInformation($"Curl response: {response}");

                if (!string.IsNullOrEmpty(response))
                {
                    var userDto = JsonSerializer.Deserialize<UserDto>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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
                _logger.LogInformation($"Getting user profile for id: {userId}");

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "curl.exe",
                        Arguments = $"-X GET http://127.0.0.1:5002/api/users/{userId}",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                var response = await process.StandardOutput.ReadToEndAsync();

                if (!string.IsNullOrEmpty(response))
                {
                    var userDto = JsonSerializer.Deserialize<UserDto>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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