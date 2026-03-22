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
        public string Status { get; set; } = "Active";
        public DateTime PaymentDueDate { get; set; }
        public DateTime? LastPaymentDate { get; set; }
        public int TermMonths { get; set; }

        public int DaysOverdue => Status == "Overdue" && PaymentDueDate < DateTime.UtcNow
            ? (DateTime.UtcNow - PaymentDueDate).Days
            : 0;

        public CreditTariff? Tariff { get; set; }
        public ICollection<CreditPayment>? Payments { get; set; }
    }
}