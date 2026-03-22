using System;

namespace BankSystem.Credit.DTOs
{
    public class CreditTariffDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal InterestRate { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateCreditTariffRequest
    {
        public string Name { get; set; } = string.Empty;
        public decimal InterestRate { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class UpdateCreditTariffRequest
    {
        public string Name { get; set; } = string.Empty;
        public decimal InterestRate { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}