using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BankSystem.Auth.Data;
using BankSystem.Auth.Models;

namespace BankSystem.Auth.Services
{
    public class AuthStorageService : IAuthStorageService
    {
        private readonly AuthDbContext _context;

        public AuthStorageService(AuthDbContext context)
        {
            _context = context;
        }

        public async Task CreateAuthUserAsync(Guid userId, string email, string password)
        {
            try
            {
                Console.WriteLine($"Saving to DB: UserId={userId}, Email={email}");

                var authUser = new AuthUser
                {
                    Id = userId,
                    Email = email,
                    PasswordHash = HashPassword(password),
                    CreatedAt = DateTime.UtcNow
                };

                _context.AuthUsers.Add(authUser);
                await _context.SaveChangesAsync();

                Console.WriteLine("Saved successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving: {ex.Message}");
                throw;
            }
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}