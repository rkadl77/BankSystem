using System;

namespace BankSystem.Users.DTOs
{
    public class AccountDto
    {
        public Guid Id { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string Currency { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}