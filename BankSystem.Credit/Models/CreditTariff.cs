using System;
using System.Collections.Generic;

namespace BankSystem.Credit.Models
{
    public class CreditTariff
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal InterestRate { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public ICollection<Credit>? Credits { get; set; }
    }
}