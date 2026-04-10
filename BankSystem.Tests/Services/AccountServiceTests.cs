using System;
using System.Threading.Tasks;
using BankSystem.Clients;
using BankSystem.Data;
using BankSystem.DTOs;
using BankSystem.Models;
using BankSystem.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace BankSystem.Tests.Services
{
    public class AccountServiceTests
    {
        private BankSystemContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<BankSystemContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new BankSystemContext(options);
        }

        private AccountService GetService(BankSystemContext context)
        {
            var mockUserClient = new Mock<IUserServiceClient>();
            mockUserClient.Setup(c => c.UserExistsAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            return new AccountService(context, mockUserClient.Object);
        }

        #region GetMasterAccountAsync Tests

        [Fact]
        public async Task GetMasterAccountAsync_WhenMasterAccountExists_ReturnsMasterAccount()
        {
            // Arrange
            var context = GetInMemoryContext();
            var masterAccount = new Account
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                AccountNumber = "MASTER-001",
                ClientId = null,
                Balance = 100000.00m,
                Currency = "USD",
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                IsMasterAccount = true
            };
            context.Accounts.Add(masterAccount);
            await context.SaveChangesAsync();

            var service = GetService(context);

            // Act
            var result = await service.GetMasterAccountAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsMasterAccount);
            Assert.Equal("MASTER-001", result.AccountNumber);
            Assert.Equal(100000.00m, result.Balance);
        }

        [Fact]
        public async Task GetMasterAccountAsync_WhenMasterAccountDoesNotExist_ThrowsInvalidOperationException()
        {
            // Arrange
            var context = GetInMemoryContext();
            var service = GetService(context);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.GetMasterAccountAsync());
            Assert.Equal("Master account not found", exception.Message);
        }

        #endregion

        #region CloseAccountAsync Tests

        [Fact]
        public async Task CloseAccountAsync_WhenClosingMasterAccount_ThrowsInvalidOperationException()
        {
            // Arrange
            var context = GetInMemoryContext();
            var masterAccount = new Account
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                AccountNumber = "MASTER-001",
                ClientId = null,
                Balance = 0,
                Currency = "USD",
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                IsMasterAccount = true
            };
            context.Accounts.Add(masterAccount);
            await context.SaveChangesAsync();

            var service = GetService(context);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CloseAccountAsync(masterAccount.Id));
            Assert.Equal("Cannot close the master account", exception.Message);
        }

        #endregion

        #region CreateAccountAsync Tests

        [Fact]
        public async Task CreateAccountAsync_WhenMasterAccountExists_CreatesAccountSuccessfully()
        {
            // Arrange
            var context = GetInMemoryContext();
            var masterAccount = new Account
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                AccountNumber = "MASTER-001",
                ClientId = null,
                Balance = 100000.00m,
                Currency = "USD",
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                IsMasterAccount = true
            };
            context.Accounts.Add(masterAccount);
            await context.SaveChangesAsync();

            var service = GetService(context);
            var clientId = Guid.NewGuid();
            var request = new CreateAccountRequest
            {
                ClientId = clientId,
                Currency = "USD"
            };

            // Act
            var result = await service.CreateAccountAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.NotNull(result.AccountNumber);
            Assert.Equal("USD", result.Currency);
            Assert.Equal(0, result.Balance);
            Assert.True(result.IsActive);
        }

        [Fact]
        public async Task CreateAccountAsync_WhenNoMasterAccountExists_ThrowsInvalidOperationException()
        {
            // Arrange
            var context = GetInMemoryContext();
            // Don't add a master account

            var service = GetService(context);
            var request = new CreateAccountRequest
            {
                ClientId = Guid.NewGuid(),
                Currency = "USD"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateAccountAsync(request));
            Assert.Equal("Cannot create account: Master account (bank) does not exist yet", exception.Message);
        }

        #endregion
    }
}
