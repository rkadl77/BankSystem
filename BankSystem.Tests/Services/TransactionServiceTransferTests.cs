using System;
using System.Linq;
using System.Threading.Tasks;
using BankSystem.Data;
using BankSystem.DTOs;
using BankSystem.Models;
using BankSystem.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BankSystem.Tests.Services
{
    public class TransactionServiceTransferTests
    {
        private BankSystemContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<BankSystemContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            return new BankSystemContext(options);
        }

        private TransactionService GetService(BankSystemContext context, ICurrencyExchangeService? currencyService = null)
        {
            currencyService ??= Mock.Of<ICurrencyExchangeService>();
            var wsLogger = Mock.Of<ILogger<WebSocketHandler>>();
            var wsHandler = new WebSocketHandler(wsLogger);
            return new TransactionService(context, wsHandler, currencyService);
        }

        #region TransferBetweenAccountsAsync Tests

        [Fact]
        public async Task TransferBetweenAccountsAsync_SameCurrency_CreatesTwoTransactions()
        {
            // Arrange
            var context = GetInMemoryContext();
            var fromAccount = new Account
            {
                Id = Guid.NewGuid(),
                AccountNumber = "ACC-001",
                ClientId = Guid.NewGuid(),
                Balance = 1000m,
                Currency = "USD",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            var toAccount = new Account
            {
                Id = Guid.NewGuid(),
                AccountNumber = "ACC-002",
                ClientId = Guid.NewGuid(),
                Balance = 500m,
                Currency = "USD",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            context.Accounts.AddRange(fromAccount, toAccount);
            await context.SaveChangesAsync();

            var service = GetService(context);
            var request = new TransferBetweenAccountsRequest
            {
                FromAccountId = fromAccount.Id,
                ToAccountId = toAccount.Id,
                Amount = 200m
            };

            // Act
            var result = await service.TransferBetweenAccountsAsync(request);

            // Assert
            var transactions = result.ToList();
            Assert.Equal(2, transactions.Count);

            // Refresh accounts from DB
            await context.Entry(fromAccount).ReloadAsync();
            await context.Entry(toAccount).ReloadAsync();

            Assert.Equal(800m, fromAccount.Balance);
            Assert.Equal(700m, toAccount.Balance);

            var debitTx = transactions.First(t => t.Type == "transfer_out");
            var creditTx = transactions.First(t => t.Type == "transfer_in");

            Assert.Equal(-200m, debitTx.Amount);
            Assert.Equal(200m, creditTx.Amount);
            Assert.Equal("USD", debitTx.Currency);
            Assert.Equal("USD", creditTx.Currency);
            Assert.Null(debitTx.ConversionRate);
            Assert.Null(creditTx.ConversionRate);
        }

        [Fact]
        public async Task TransferBetweenAccountsAsync_DifferentCurrencies_ConvertsAndCreatesTransactions()
        {
            // Arrange
            var context = GetInMemoryContext();
            var fromAccount = new Account
            {
                Id = Guid.NewGuid(),
                AccountNumber = "ACC-001",
                ClientId = Guid.NewGuid(),
                Balance = 1000m,
                Currency = "USD",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            var toAccount = new Account
            {
                Id = Guid.NewGuid(),
                AccountNumber = "ACC-002",
                ClientId = Guid.NewGuid(),
                Balance = 500m,
                Currency = "EUR",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            context.Accounts.AddRange(fromAccount, toAccount);
            await context.SaveChangesAsync();

            var mockCurrencyService = new Mock<ICurrencyExchangeService>();
            mockCurrencyService.Setup(x => x.GetExchangeRateAsync("USD", "EUR")).ReturnsAsync(0.85m);
            mockCurrencyService.Setup(x => x.ConvertAsync(200m, "USD", "EUR")).ReturnsAsync(170m);

            var service = GetService(context, mockCurrencyService.Object);
            var request = new TransferBetweenAccountsRequest
            {
                FromAccountId = fromAccount.Id,
                ToAccountId = toAccount.Id,
                Amount = 200m
            };

            // Act
            var result = await service.TransferBetweenAccountsAsync(request);

            // Assert
            var transactions = result.ToList();
            Assert.Equal(2, transactions.Count);

            // Refresh accounts from DB
            await context.Entry(fromAccount).ReloadAsync();
            await context.Entry(toAccount).ReloadAsync();

            Assert.Equal(800m, fromAccount.Balance);
            Assert.Equal(670m, toAccount.Balance); // 500 + 170

            var debitTx = transactions.First(t => t.Type == "transfer_out");
            var creditTx = transactions.First(t => t.Type == "transfer_in");

            Assert.Equal(-200m, debitTx.Amount);
            Assert.Equal(170m, creditTx.Amount);
            Assert.Equal("USD", debitTx.Currency);
            Assert.Equal("EUR", creditTx.Currency);
            Assert.Equal(0.85m, debitTx.ConversionRate);
            Assert.Equal(0.85m, creditTx.ConversionRate);
            Assert.Equal("USD", debitTx.OriginalCurrency);
            Assert.Equal("EUR", debitTx.TargetCurrency);
        }

        [Fact]
        public async Task TransferBetweenAccountsAsync_InsufficientFunds_ThrowsInvalidOperationException()
        {
            // Arrange
            var context = GetInMemoryContext();
            var fromAccount = new Account
            {
                Id = Guid.NewGuid(),
                AccountNumber = "ACC-001",
                ClientId = Guid.NewGuid(),
                Balance = 100m,
                Currency = "USD",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            var toAccount = new Account
            {
                Id = Guid.NewGuid(),
                AccountNumber = "ACC-002",
                ClientId = Guid.NewGuid(),
                Balance = 500m,
                Currency = "USD",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            context.Accounts.AddRange(fromAccount, toAccount);
            await context.SaveChangesAsync();

            var service = GetService(context);
            var request = new TransferBetweenAccountsRequest
            {
                FromAccountId = fromAccount.Id,
                ToAccountId = toAccount.Id,
                Amount = 200m
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.TransferBetweenAccountsAsync(request));
            Assert.Equal("Insufficient funds", exception.Message);

            // Verify balances unchanged
            await context.Entry(fromAccount).ReloadAsync();
            await context.Entry(toAccount).ReloadAsync();
            Assert.Equal(100m, fromAccount.Balance);
            Assert.Equal(500m, toAccount.Balance);
        }

        [Fact]
        public async Task TransferBetweenAccountsAsync_AccountNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var context = GetInMemoryContext();
            var service = GetService(context);
            var request = new TransferBetweenAccountsRequest
            {
                FromAccountId = Guid.NewGuid(),
                ToAccountId = Guid.NewGuid(),
                Amount = 200m
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.TransferBetweenAccountsAsync(request));
            Assert.Equal("One or both accounts not found", exception.Message);
        }

        [Fact]
        public async Task TransferBetweenAccountsAsync_InactiveAccount_ThrowsInvalidOperationException()
        {
            // Arrange
            var context = GetInMemoryContext();
            var fromAccount = new Account
            {
                Id = Guid.NewGuid(),
                AccountNumber = "ACC-001",
                ClientId = Guid.NewGuid(),
                Balance = 1000m,
                Currency = "USD",
                CreatedAt = DateTime.UtcNow,
                IsActive = false // Inactive
            };
            var toAccount = new Account
            {
                Id = Guid.NewGuid(),
                AccountNumber = "ACC-002",
                ClientId = Guid.NewGuid(),
                Balance = 500m,
                Currency = "USD",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            context.Accounts.AddRange(fromAccount, toAccount);
            await context.SaveChangesAsync();

            var service = GetService(context);
            var request = new TransferBetweenAccountsRequest
            {
                FromAccountId = fromAccount.Id,
                ToAccountId = toAccount.Id,
                Amount = 200m
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.TransferBetweenAccountsAsync(request));
            Assert.Equal("One or both accounts are inactive", exception.Message);
        }

        #endregion
    }
}
