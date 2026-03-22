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
        public DateTime PaymentDueDate { get; set; }
        public DateTime? LastPaymentDate { get; set; }
        public int TermMonths { get; set; }
        public int DaysOverdue { get; set; }
        public List<CreditPaymentDto> Payments { get; set; } = new();
    }

    public class CreditDetailDto
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
        public DateTime PaymentDueDate { get; set; }
        public DateTime? LastPaymentDate { get; set; }
        public int TermMonths { get; set; }
        public int DaysOverdue { get; set; }
        public string RatingDescription { get; set; } = string.Empty;
        public List<CreditPaymentDto> Payments { get; set; } = new();
    }

    public class CreditRatingDto
    {
        public Guid ClientId { get; set; }
        public int Rating { get; set; }
        public string Description { get; set; } = string.Empty;
        public int OverdueCount { get; set; }
        public int TotalCount { get; set; }
        public decimal OnTimePercentage { get; set; }
    }
}