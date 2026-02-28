using System;

namespace BankSystem.Credit.DTOs
{
    public class CreditPaymentDto
    {
        public Guid Id { get; set; }
        public Guid CreditId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class RepayCreditRequest
    {
        public Guid CreditId { get; set; }
        public decimal Amount { get; set; }
    }
}