using System;
using System.Collections.Generic;

namespace BankSystem.Credit.Models
{
    public class Credit
    {
        public Guid Id { get; set; }
        public Guid ClientId { get; set; }
        public Guid AccountId { get; set; }
        public Guid TariffId { get; set; }
        public decimal Amount { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal InterestRate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public CreditTariff? Tariff { get; set; }
        public ICollection<CreditPayment>? Payments { get; set; }
    }
}