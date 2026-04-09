namespace BankSystem.Auth.DTOs
{
    public class CreateAuthUserRequest
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}