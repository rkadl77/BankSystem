using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BankSystem.Data;
using BankSystem.Models;
using BankSystem.DTOs;
using System.Text.Json;

namespace BankSystem.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly BankSystemContext _context;
        private readonly WebSocketHandler _webSocketHandler;
        private readonly ICurrencyExchangeService _currencyExchangeService;

        public TransactionService(BankSystemContext context, WebSocketHandler webSocketHandler, ICurrencyExchangeService currencyExchangeService)
        {
            _context = context;
            _webSocketHandler = webSocketHandler;
            _currencyExchangeService = currencyExchangeService;
        }

        public async Task<IEnumerable<TransactionDto>> GetTransactionsByAccountIdAsync(Guid accountId)
        {
            var transactions = await _context.Transactions
                .Where(t => t.AccountId == accountId)
                .OrderByDescending(t => t.Timestamp)
                .ToListAsync();

            return transactions.Select(t => new TransactionDto
            {
                Id = t.Id,
                Amount = t.Amount,
                Currency = t.Currency,
                Type = t.Type,
                Status = t.Status,
                Timestamp = t.Timestamp,
                Description = t.Description,
                RelatedTransactionId = t.RelatedTransactionId
            });
        }

        public async Task<TransactionDto> DepositAsync(CreateTransactionRequest request)
        {
            var account = await _context.Accounts.FindAsync(request.AccountId);
            if (account == null || !account.IsActive)
                throw new InvalidOperationException("Account not found or inactive");

            if (account.Currency != request.Currency)
                throw new InvalidOperationException($"Currency mismatch. Account currency: {account.Currency}");

            account.Balance += request.Amount;

            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = request.AccountId,
                Amount = request.Amount,
                Currency = account.Currency,
                Type = "deposit",
                Status = "completed",
                Timestamp = DateTime.UtcNow,
                Description = request.Description
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            var notification = JsonSerializer.Serialize(new
            {
                type = "transaction",
                accountId = request.AccountId,
                transactionId = transaction.Id,
                amount = transaction.Amount,
                timestamp = transaction.Timestamp,
                description = transaction.Description
            });
            await _webSocketHandler.NotifyAccountUpdateAsync(request.AccountId, notification);

            return new TransactionDto
            {
                Id = transaction.Id,
                Amount = transaction.Amount,
                Currency = transaction.Currency,
                Type = transaction.Type,
                Status = transaction.Status,
                Timestamp = transaction.Timestamp,
                Description = transaction.Description,
                RelatedTransactionId = transaction.RelatedTransactionId
            };
        }

        public async Task<TransactionDto> WithdrawAsync(CreateTransactionRequest request)
        {
            var account = await _context.Accounts.FindAsync(request.AccountId);
            if (account == null || !account.IsActive)
                throw new InvalidOperationException("Account not found or inactive");

            if (account.Currency != request.Currency)
                throw new InvalidOperationException($"Currency mismatch. Account currency: {account.Currency}");

            if (account.Balance < request.Amount)
                throw new InvalidOperationException("Insufficient funds");

            account.Balance -= request.Amount;

            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = request.AccountId,
                Amount = -request.Amount,
                Currency = account.Currency,
                Type = "withdrawal",
                Status = "completed",
                Timestamp = DateTime.UtcNow,
                Description = request.Description
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            var notification = JsonSerializer.Serialize(new
            {
                type = "transaction",
                accountId = request.AccountId,
                transactionId = transaction.Id,
                amount = transaction.Amount,
                timestamp = transaction.Timestamp,
                description = transaction.Description
            });
            await _webSocketHandler.NotifyAccountUpdateAsync(request.AccountId, notification);

            return new TransactionDto
            {
                Id = transaction.Id,
                Amount = transaction.Amount,
                Currency = transaction.Currency,
                Type = transaction.Type,
                Status = transaction.Status,
                Timestamp = transaction.Timestamp,
                Description = transaction.Description,
                RelatedTransactionId = transaction.RelatedTransactionId
            };
        }

        public async Task<bool> TransferAsync(TransferRequest request)
        {
            var fromAccount = await _context.Accounts.FindAsync(request.FromAccountId);
            var toAccount = await _context.Accounts.FindAsync(request.ToAccountId);

            if (fromAccount == null || toAccount == null || !fromAccount.IsActive || !toAccount.IsActive)
                throw new InvalidOperationException("Accounts not found or inactive");

            if (fromAccount.Currency != request.Currency || toAccount.Currency != request.Currency)
                throw new InvalidOperationException("Currency mismatch");

            if (fromAccount.Balance < request.Amount)
                throw new InvalidOperationException("Insufficient funds");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                fromAccount.Balance -= request.Amount;
                toAccount.Balance += request.Amount;

                var transactionId = Guid.NewGuid();

                var fromTransaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    AccountId = request.FromAccountId,
                    Amount = -request.Amount,
                    Currency = fromAccount.Currency,
                    Type = "transfer_out",
                    Status = "completed",
                    Timestamp = DateTime.UtcNow,
                    Description = $"Transfer to {toAccount.AccountNumber}: {request.Description}",
                    RelatedTransactionId = transactionId
                };

                var toTransaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    AccountId = request.ToAccountId,
                    Amount = request.Amount,
                    Currency = toAccount.Currency,
                    Type = "transfer_in",
                    Status = "completed",
                    Timestamp = DateTime.UtcNow,
                    Description = $"Transfer from {fromAccount.AccountNumber}: {request.Description}",
                    RelatedTransactionId = transactionId
                };

                _context.Transactions.AddRange(fromTransaction, toTransaction);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var fromNotification = JsonSerializer.Serialize(new
                {
                    type = "transaction",
                    accountId = request.FromAccountId,
                    transactionId = fromTransaction.Id,
                    amount = fromTransaction.Amount,
                    timestamp = fromTransaction.Timestamp,
                    description = fromTransaction.Description
                });
                await _webSocketHandler.NotifyAccountUpdateAsync(request.FromAccountId, fromNotification);

                var toNotification = JsonSerializer.Serialize(new
                {
                    type = "transaction",
                    accountId = request.ToAccountId,
                    transactionId = toTransaction.Id,
                    amount = toTransaction.Amount,
                    timestamp = toTransaction.Timestamp,
                    description = toTransaction.Description
                });
                await _webSocketHandler.NotifyAccountUpdateAsync(request.ToAccountId, toNotification);

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<TransactionDto>> TransferBetweenAccountsAsync(TransferBetweenAccountsRequest request)
        {
            var fromAccount = await _context.Accounts.FindAsync(request.FromAccountId);
            var toAccount = await _context.Accounts.FindAsync(request.ToAccountId);

            if (fromAccount == null || toAccount == null)
                throw new InvalidOperationException("One or both accounts not found");

            if (!fromAccount.IsActive || !toAccount.IsActive)
                throw new InvalidOperationException("One or both accounts are inactive");

            if (fromAccount.Balance < request.Amount)
                throw new InvalidOperationException("Insufficient funds");

            var transactionId = Guid.NewGuid();
            var transactions = new List<Transaction>();
            var transactionDtos = new List<TransactionDto>();

            using var dbTransaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (fromAccount.Currency == toAccount.Currency)
                {
                    // Same currency - single transaction pair
                    fromAccount.Balance -= request.Amount;
                    toAccount.Balance += request.Amount;

                    var fromTransaction = new Transaction
                    {
                        Id = Guid.NewGuid(),
                        AccountId = request.FromAccountId,
                        Amount = -request.Amount,
                        Currency = fromAccount.Currency,
                        Type = "transfer_out",
                        Status = "completed",
                        Timestamp = DateTime.UtcNow,
                        Description = request.Description ?? $"Transfer to {toAccount.AccountNumber}",
                        RelatedTransactionId = transactionId
                    };

                    var toTransaction = new Transaction
                    {
                        Id = Guid.NewGuid(),
                        AccountId = request.ToAccountId,
                        Amount = request.Amount,
                        Currency = toAccount.Currency,
                        Type = "transfer_in",
                        Status = "completed",
                        Timestamp = DateTime.UtcNow,
                        Description = request.Description ?? $"Transfer from {fromAccount.AccountNumber}",
                        RelatedTransactionId = transactionId
                    };

                    transactions.Add(fromTransaction);
                    transactions.Add(toTransaction);

                    transactionDtos.Add(MapToDto(fromTransaction));
                    transactionDtos.Add(MapToDto(toTransaction));
                }
                else
                {
                    // Different currencies - convert and create 2 transaction pairs
                    var conversionRate = await _currencyExchangeService.GetExchangeRateAsync(fromAccount.Currency, toAccount.Currency);
                    var convertedAmount = await _currencyExchangeService.ConvertAsync(request.Amount, fromAccount.Currency, toAccount.Currency);

                    fromAccount.Balance -= request.Amount;
                    toAccount.Balance += convertedAmount;

                    // Debit transaction (from account)
                    var debitTransaction = new Transaction
                    {
                        Id = Guid.NewGuid(),
                        AccountId = request.FromAccountId,
                        Amount = -request.Amount,
                        Currency = fromAccount.Currency,
                        Type = "transfer_out",
                        Status = "completed",
                        Timestamp = DateTime.UtcNow,
                        Description = request.Description ?? $"Transfer to {toAccount.AccountNumber}",
                        RelatedTransactionId = transactionId,
                        ConversionRate = conversionRate,
                        OriginalCurrency = fromAccount.Currency,
                        TargetCurrency = toAccount.Currency
                    };

                    // Credit transaction (to account)
                    var creditTransaction = new Transaction
                    {
                        Id = Guid.NewGuid(),
                        AccountId = request.ToAccountId,
                        Amount = convertedAmount,
                        Currency = toAccount.Currency,
                        Type = "transfer_in",
                        Status = "completed",
                        Timestamp = DateTime.UtcNow,
                        Description = request.Description ?? $"Transfer from {fromAccount.AccountNumber}",
                        RelatedTransactionId = transactionId,
                        ConversionRate = conversionRate,
                        OriginalCurrency = fromAccount.Currency,
                        TargetCurrency = toAccount.Currency
                    };

                    transactions.Add(debitTransaction);
                    transactions.Add(creditTransaction);

                    transactionDtos.Add(MapToDto(debitTransaction));
                    transactionDtos.Add(MapToDto(creditTransaction));
                }

                _context.Transactions.AddRange(transactions);
                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                return transactionDtos;
            }
            catch
            {
                await dbTransaction.RollbackAsync();
                throw;
            }
        }

        private TransactionDto MapToDto(Transaction t)
        {
            return new TransactionDto
            {
                Id = t.Id,
                Amount = t.Amount,
                Currency = t.Currency,
                Type = t.Type,
                Status = t.Status,
                Timestamp = t.Timestamp,
                Description = t.Description,
                RelatedTransactionId = t.RelatedTransactionId,
                ConversionRate = t.ConversionRate,
                OriginalCurrency = t.OriginalCurrency,
                TargetCurrency = t.TargetCurrency
            };
        }
    }
}