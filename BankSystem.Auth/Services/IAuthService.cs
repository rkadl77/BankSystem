using System.Threading.Tasks;

namespace BankSystem.Auth.Services
{
    public interface IAuthService
    {
        Task<AuthResult> ValidateCredentialsAsync(string email, string password);
    }

    public class AuthResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public Guid? UserId { get; set; }
        public string? Role { get; set; }
        public string? FullName { get; set; }
    }
}