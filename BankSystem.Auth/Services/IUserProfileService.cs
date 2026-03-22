using System;
using System.Threading.Tasks;

namespace BankSystem.Auth.Services
{
    public interface IUserProfileService
    {
        Task<UserProfile?> GetUserProfileAsync(string email);
        Task<UserProfile?> GetUserProfileByIdAsync(Guid userId);
        Task<bool> UserExistsAsync(string email);
    }

    public class UserProfile
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}