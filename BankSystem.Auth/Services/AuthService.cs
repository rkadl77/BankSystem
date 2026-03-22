using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BankSystem.Auth.Data;
using BankSystem.Auth.Models;

namespace BankSystem.Auth.Services
{
    public class AuthService : IAuthService
    {
        private readonly AuthDbContext _context;
        private readonly IUserProfileService _userProfileService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            AuthDbContext context,
            IUserProfileService userProfileService,
            ILogger<AuthService> logger)
        {
            _context = context;
            _userProfileService = userProfileService;
            _logger = logger;
        }

        public async Task<AuthResult> ValidateCredentialsAsync(string email, string password)
        {
            try
            {
                var authUser = await _context.AuthUsers
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (authUser == null)
                {
                    return new AuthResult { Success = false, ErrorMessage = "User not found" };
                }

                if (!VerifyPassword(password, authUser.PasswordHash))
                {
                    return new AuthResult { Success = false, ErrorMessage = "Invalid password" };
                }

                var profile = await _userProfileService.GetUserProfileAsync(email);
                if (profile == null)
                {
                    return new AuthResult { Success = false, ErrorMessage = "User profile not found" };
                }

                if (!profile.IsActive)
                {
                    return new AuthResult { Success = false, ErrorMessage = "User is inactive" };
                }

                return new AuthResult
                {
                    Success = true,
                    UserId = profile.Id,
                    Role = profile.Role,
                    FullName = $"{profile.FirstName} {profile.LastName}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating credentials for {Email}", email);
                return new AuthResult { Success = false, ErrorMessage = "Internal error" };
            }
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }
    }
}