using System;

namespace BankSystem.Credit.Models
{
    public class CreditPayment
    {
        public Guid Id { get; set; }
        public Guid CreditId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public Credit? Credit { get; set; }
    }
}