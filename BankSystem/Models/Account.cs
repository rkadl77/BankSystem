using System;

namespace BankSystem.Models
{
    public class Account
    {
        public Guid Id { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public Guid? ClientId { get; set; }
        public decimal Balance { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ClosedAt { get; set; }
        public bool IsMasterAccount { get; set; } = false;
    }
}