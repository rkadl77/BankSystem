using System;

namespace BankSystem.DTOs
{
    public class TransactionDto
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? Description { get; set; }
        public Guid? RelatedTransactionId { get; set; }
        public decimal? ConversionRate { get; set; }
        public string? OriginalCurrency { get; set; }
        public string? TargetCurrency { get; set; }
    }

    public class CreateTransactionRequest
    {
        public Guid AccountId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class TransferRequest
    {
        public Guid FromAccountId { get; set; }
        public Guid ToAccountId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class TransferBetweenAccountsRequest
    {
        public Guid FromAccountId { get; set; }
        public Guid ToAccountId { get; set; }
        public decimal Amount { get; set; }
    }
}