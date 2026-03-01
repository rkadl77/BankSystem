using System;

namespace BankSystem.Users.DTOs
{
    public class CreditDto
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal InterestRate { get; set; }
        public DateTime StartDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}