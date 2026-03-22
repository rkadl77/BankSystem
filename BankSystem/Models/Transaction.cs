using System;

namespace BankSystem.Models
{
    public class Transaction
    {
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = "completed";
        public DateTime Timestamp { get; set; }
        public string? Description { get; set; }
        public Guid? RelatedTransactionId { get; set; }
        public decimal? ConversionRate { get; set; }
        public string? OriginalCurrency { get; set; }
        public string? TargetCurrency { get; set; }
    }
}