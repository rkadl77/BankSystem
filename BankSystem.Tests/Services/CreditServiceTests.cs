using System;
using System.Linq;
using System.Threading.Tasks;
using BankSystem.Credit.Clients;
using BankSystem.Credit.Data;
using BankSystem.Credit.DTOs;
using BankSystem.Credit.Models;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using CreditModel = BankSystem.Credit.Models.Credit;
using CreditService = BankSystem.Credit.Services.CreditService;

namespace BankSystem.Tests.Services
{
    public class CreditServiceTests
    {
        private CreditDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<CreditDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new CreditDbContext(options);
        }

        private CreditService GetService(CreditDbContext context)
        {
            var mockCoreClient = new Mock<CoreServiceClient>(new System.Net.Http.HttpClient(), Mock.Of<Microsoft.Extensions.Logging.ILogger<CoreServiceClient>>());
            return new CreditService(context, mockCoreClient.Object);
        }

        #region GetOverdueCreditsAsync Tests

        [Fact]
        public async Task GetOverdueCreditsAsync_WithOverdueStatus_ReturnsOverdueCredits()
        {
            // Arrange
            var context = GetInMemoryContext();
            var clientId = Guid.NewGuid();
            var tariff = new CreditTariff { Id = Guid.NewGuid(), Name = "Test Tariff", InterestRate = 5.5m };
            context.CreditTariffs.Add(tariff);

            var overdueCredit = new CreditModel
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                AccountId = Guid.NewGuid(),
                TariffId = tariff.Id,
                Amount = 10000m,
                RemainingAmount = 5000m,
                InterestRate = 5.5m,
                StartDate = DateTime.UtcNow.AddMonths(-6),
                Status = "Overdue",
                PaymentDueDate = DateTime.UtcNow.AddDays(-10),
                TermMonths = 12
            };

            var activeCredit = new CreditModel
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                AccountId = Guid.NewGuid(),
                TariffId = tariff.Id,
                Amount = 5000m,
                RemainingAmount = 3000m,
                InterestRate = 5.5m,
                StartDate = DateTime.UtcNow.AddMonths(-3),
                Status = "Active",
                PaymentDueDate = DateTime.UtcNow.AddDays(10),
                TermMonths = 12
            };

            context.Credits.AddRange(overdueCredit, activeCredit);
            await context.SaveChangesAsync();

            var service = GetService(context);

            // Act
            var result = await service.GetOverdueCreditsAsync(clientId);

            // Assert
            var credits = result.ToList();
            Assert.Single(credits);
            Assert.Equal("Overdue", credits[0].Status);
        }

        [Fact]
        public async Task GetOverdueCreditsAsync_WithPastDueDate_ReturnsOverdueCredits()
        {
            // Arrange
            var context = GetInMemoryContext();
            var clientId = Guid.NewGuid();
            var tariff = new CreditTariff { Id = Guid.NewGuid(), Name = "Test Tariff", InterestRate = 5.5m };
            context.CreditTariffs.Add(tariff);

            var pastDueCredit = new CreditModel
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                AccountId = Guid.NewGuid(),
                TariffId = tariff.Id,
                Amount = 10000m,
                RemainingAmount = 5000m,
                InterestRate = 5.5m,
                StartDate = DateTime.UtcNow.AddMonths(-6),
                Status = "Active", // Not explicitly marked as Overdue
                PaymentDueDate = DateTime.UtcNow.AddDays(-5), // But past due date
                TermMonths = 12
            };

            context.Credits.Add(pastDueCredit);
            await context.SaveChangesAsync();

            var service = GetService(context);

            // Act
            var result = await service.GetOverdueCreditsAsync(clientId);

            // Assert
            var credits = result.ToList();
            Assert.Single(credits);
        }

        #endregion

        #region UpdateCreditStatusAsync Tests

        [Fact]
        public async Task UpdateCreditStatusAsync_PastDueDate_SetsOverdue()
        {
            // Arrange
            var context = GetInMemoryContext();
            var credit = new CreditModel
            {
                Id = Guid.NewGuid(),
                ClientId = Guid.NewGuid(),
                AccountId = Guid.NewGuid(),
                TariffId = Guid.NewGuid(),
                Amount = 10000m,
                RemainingAmount = 5000m,
                InterestRate = 5.5m,
                StartDate = DateTime.UtcNow.AddMonths(-6),
                Status = "Active",
                PaymentDueDate = DateTime.UtcNow.AddDays(-5),
                TermMonths = 12
            };
            context.Credits.Add(credit);
            await context.SaveChangesAsync();

            var service = GetService(context);

            // Act
            var result = await service.UpdateCreditStatusAsync(credit.Id);

            // Assert
            Assert.True(result);
            var updatedCredit = await context.Credits.FindAsync(credit.Id);
            Assert.Equal("Overdue", updatedCredit?.Status);
        }

        [Fact]
        public async Task UpdateCreditStatusAsync_PaidAfterDueDate_SetsPaid()
        {
            // Arrange
            var context = GetInMemoryContext();
            var credit = new CreditModel
            {
                Id = Guid.NewGuid(),
                ClientId = Guid.NewGuid(),
                AccountId = Guid.NewGuid(),
                TariffId = Guid.NewGuid(),
                Amount = 10000m,
                RemainingAmount = 0m,
                InterestRate = 5.5m,
                StartDate = DateTime.UtcNow.AddMonths(-6),
                Status = "Overdue",
                PaymentDueDate = DateTime.UtcNow.AddDays(-10),
                LastPaymentDate = DateTime.UtcNow.AddDays(-5), // Paid after due date
                TermMonths = 12
            };
            context.Credits.Add(credit);
            await context.SaveChangesAsync();

            var service = GetService(context);

            // Act
            var result = await service.UpdateCreditStatusAsync(credit.Id);

            // Assert
            Assert.True(result);
            var updatedCredit = await context.Credits.FindAsync(credit.Id);
            Assert.Equal("Paid", updatedCredit?.Status);
        }

        [Fact]
        public async Task UpdateCreditStatusAsync_NotFound_ReturnsFalse()
        {
            // Arrange
            var context = GetInMemoryContext();
            var service = GetService(context);

            // Act
            var result = await service.UpdateCreditStatusAsync(Guid.NewGuid());

            // Assert
            Assert.False(result);
        }

        #endregion

        #region CalculateCreditRatingAsync Tests

        [Fact]
        public async Task CalculateCreditRatingAsync_NoCredits_ReturnsPerfectRating()
        {
            // Arrange
            var context = GetInMemoryContext();
            var clientId = Guid.NewGuid();
            var service = GetService(context);

            // Act
            var result = await service.CalculateCreditRatingAsync(clientId);

            // Assert
            Assert.Equal(100, result.Rating);
            Assert.Equal("Excellent", result.Description);
            Assert.Equal(0, result.TotalCount);
            Assert.Equal(100m, result.OnTimePercentage);
        }

        [Fact]
        public async Task CalculateCreditRatingAsync_WithOverdueCredits_ReducesRating()
        {
            // Arrange
            var context = GetInMemoryContext();
            var clientId = Guid.NewGuid();
            var tariff = new CreditTariff { Id = Guid.NewGuid(), Name = "Test Tariff", InterestRate = 5.5m };
            context.CreditTariffs.Add(tariff);

            var overdueCredit = new CreditModel
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                AccountId = Guid.NewGuid(),
                TariffId = tariff.Id,
                Amount = 10000m,
                RemainingAmount = 5000m,
                InterestRate = 5.5m,
                StartDate = DateTime.UtcNow.AddMonths(-6),
                Status = "Overdue",
                PaymentDueDate = DateTime.UtcNow.AddDays(-10),
                TermMonths = 12
            };

            context.Credits.Add(overdueCredit);
            await context.SaveChangesAsync();

            var service = GetService(context);

            // Act
            var result = await service.CalculateCreditRatingAsync(clientId);

            // Assert
            Assert.Equal(1, result.TotalCount);
            Assert.Equal(1, result.OverdueCount);
            Assert.True(result.Rating < 100);
        }

        [Fact]
        public async Task CalculateCreditRatingAsync_WithDefaultedCredits_ReducesRatingMore()
        {
            // Arrange
            var context = GetInMemoryContext();
            var clientId = Guid.NewGuid();
            var tariff = new CreditTariff { Id = Guid.NewGuid(), Name = "Test Tariff", InterestRate = 5.5m };
            context.CreditTariffs.Add(tariff);

            var defaultedCredit = new CreditModel
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                AccountId = Guid.NewGuid(),
                TariffId = tariff.Id,
                Amount = 10000m,
                RemainingAmount = 5000m,
                InterestRate = 5.5m,
                StartDate = DateTime.UtcNow.AddMonths(-12),
                Status = "Defaulted",
                PaymentDueDate = DateTime.UtcNow.AddDays(-100),
                TermMonths = 12
            };

            context.Credits.Add(defaultedCredit);
            await context.SaveChangesAsync();

            var service = GetService(context);

            // Act
            var result = await service.CalculateCreditRatingAsync(clientId);

            // Assert
            Assert.Equal(1, result.TotalCount);
            Assert.Equal(0, result.OverdueCount); // Defaulted is not Overdue
            Assert.Equal(85, result.Rating); // 100 - 15 for default = 85
        }

        #endregion

        #region GetCreditDetailsAsync Tests

        [Fact]
        public async Task GetCreditDetailsAsync_ExistingCredit_ReturnsDetailsWithRating()
        {
            // Arrange
            var context = GetInMemoryContext();
            var clientId = Guid.NewGuid();
            var tariff = new CreditTariff { Id = Guid.NewGuid(), Name = "Test Tariff", InterestRate = 5.5m };
            context.CreditTariffs.Add(tariff);

            var credit = new CreditModel
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                AccountId = Guid.NewGuid(),
                TariffId = tariff.Id,
                Amount = 10000m,
                RemainingAmount = 5000m,
                InterestRate = 5.5m,
                StartDate = DateTime.UtcNow.AddMonths(-6),
                Status = "Active",
                PaymentDueDate = DateTime.UtcNow.AddDays(10),
                TermMonths = 12
            };

            context.Credits.Add(credit);
            await context.SaveChangesAsync();

            var service = GetService(context);

            // Act
            var result = await service.GetCreditDetailsAsync(credit.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(credit.Id, result.Id);
            Assert.Equal("Test Tariff", result.TariffName);
            Assert.NotNull(result.RatingDescription);
        }

        [Fact]
        public async Task GetCreditDetailsAsync_NonExistingCredit_ReturnsNull()
        {
            // Arrange
            var context = GetInMemoryContext();
            var service = GetService(context);

            // Act
            var result = await service.GetCreditDetailsAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        #endregion
    }
}
