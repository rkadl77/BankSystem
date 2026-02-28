using System;

namespace BankSystem.Credit.DTOs
{
    public class CreditDto
    {
        public Guid Id { get; set; }
        public Guid ClientId { get; set; }
        public Guid AccountId { get; set; }
        public string TariffName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal InterestRate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class CreateCreditRequest
    {
        public Guid ClientId { get; set; }
        public Guid AccountId { get; set; }
        public Guid TariffId { get; set; }
        public decimal Amount { get; set; }
    }

    public class CreditDetailsDto
    {
        public Guid Id { get; set; }
        public Guid ClientId { get; set; }
        public Guid AccountId { get; set; }
        public string TariffName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal InterestRate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<CreditPaymentDto> Payments { get; set; } = new();
    }
}