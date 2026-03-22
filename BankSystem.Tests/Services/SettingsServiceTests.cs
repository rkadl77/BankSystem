using System;
using System.Linq;
using System.Threading.Tasks;
using BankSystem.Settings.Data;
using BankSystem.Settings.DTOs;
using BankSystem.Settings.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BankSystem.Tests.Services
{
    public class SettingsServiceTests
    {
        private SettingsContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<SettingsContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new SettingsContext(options);
        }

        private SettingsService GetService(SettingsContext context)
        {
            return new SettingsService(context);
        }

        #region GetSettingsAsync Tests

        [Fact]
        public async Task GetSettingsAsync_SettingsExist_ReturnsSettings()
        {
            // Arrange
            var context = GetInMemoryContext();
            var userId = Guid.NewGuid();
            var service = GetService(context);

            // Create settings first
            await service.CreateOrUpdateSettingsAsync(userId, new CreateSettingsRequest { Theme = "Dark" });

            // Act
            var result = await service.GetSettingsAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
            Assert.Equal("Dark", result.Theme);
        }

        [Fact]
        public async Task GetSettingsAsync_SettingsMissing_CreatesDefaults()
        {
            // Arrange
            var context = GetInMemoryContext();
            var userId = Guid.NewGuid();
            var service = GetService(context);

            // Act
            var result = await service.GetSettingsAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
            Assert.Equal("Light", result.Theme);
            Assert.Empty(result.HiddenAccountIds);
        }

        #endregion

        #region CreateOrUpdateSettingsAsync Tests

        [Fact]
        public async Task CreateOrUpdateSettingsAsync_NewSettings_CreatesSettings()
        {
            // Arrange
            var context = GetInMemoryContext();
            var userId = Guid.NewGuid();
            var service = GetService(context);
            var request = new CreateSettingsRequest
            {
                Theme = "Dark",
                HiddenAccountIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }
            };

            // Act
            var result = await service.CreateOrUpdateSettingsAsync(userId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
            Assert.Equal("Dark", result.Theme);
            Assert.Equal(2, result.HiddenAccountIds.Count);
        }

        [Fact]
        public async Task CreateOrUpdateSettingsAsync_ExistingSettings_UpdatesSettings()
        {
            // Arrange
            var context = GetInMemoryContext();
            var userId = Guid.NewGuid();
            var service = GetService(context);

            // Create initial settings
            await service.CreateOrUpdateSettingsAsync(userId, new CreateSettingsRequest { Theme = "Light" });

            var updateRequest = new CreateSettingsRequest
            {
                Theme = "Dark",
                HiddenAccountIds = new List<Guid> { Guid.NewGuid() }
            };

            // Act
            var result = await service.CreateOrUpdateSettingsAsync(userId, updateRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Dark", result.Theme);
            Assert.Single(result.HiddenAccountIds);
        }

        [Fact]
        public async Task CreateOrUpdateSettingsAsync_PartialUpdate_UpdatesOnlyProvidedFields()
        {
            // Arrange
            var context = GetInMemoryContext();
            var userId = Guid.NewGuid();
            var service = GetService(context);

            // Create initial settings
            var accountId = Guid.NewGuid();
            await service.CreateOrUpdateSettingsAsync(userId, new CreateSettingsRequest
            {
                Theme = "Light",
                HiddenAccountIds = new List<Guid> { accountId }
            });

            // Update only theme
            var updateRequest = new CreateSettingsRequest { Theme = "Dark" };

            // Act
            var result = await service.CreateOrUpdateSettingsAsync(userId, updateRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Dark", result.Theme);
            // Hidden accounts should remain unchanged
            Assert.Single(result.HiddenAccountIds);
            Assert.Equal(accountId, result.HiddenAccountIds[0]);
        }

        #endregion

        #region UpdateThemeAsync Tests

        [Fact]
        public async Task UpdateThemeAsync_ValidTheme_UpdatesTheme()
        {
            // Arrange
            var context = GetInMemoryContext();
            var userId = Guid.NewGuid();
            var service = GetService(context);

            // Act
            var result = await service.UpdateThemeAsync(userId, new UpdateThemeRequest { Theme = "Dark" });

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Dark", result.Theme);
        }

        [Theory]
        [InlineData("Light")]
        [InlineData("Dark")]
        public async Task UpdateThemeAsync_ValidThemes_AcceptsTheme(string theme)
        {
            // Arrange
            var context = GetInMemoryContext();
            var userId = Guid.NewGuid();
            var service = GetService(context);

            // Act
            var result = await service.UpdateThemeAsync(userId, new UpdateThemeRequest { Theme = theme });

            // Assert
            Assert.NotNull(result);
            Assert.Equal(theme, result.Theme);
        }

        [Fact]
        public async Task UpdateThemeAsync_InvalidTheme_ThrowsInvalidOperationException()
        {
            // Arrange
            var context = GetInMemoryContext();
            var userId = Guid.NewGuid();
            var service = GetService(context);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.UpdateThemeAsync(userId, new UpdateThemeRequest { Theme = "Blue" }));
            Assert.Contains("Invalid theme", exception.Message);
        }

        #endregion

        #region UpdateHiddenAccountsAsync Tests

        [Fact]
        public async Task UpdateHiddenAccountsAsync_NewAccounts_SetsHiddenAccounts()
        {
            // Arrange
            var context = GetInMemoryContext();
            var userId = Guid.NewGuid();
            var service = GetService(context);
            var accountIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

            // Act
            var result = await service.UpdateHiddenAccountsAsync(userId, new UpdateHiddenAccountsRequest { AccountIds = accountIds });

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.HiddenAccountIds.Count);
        }

        [Fact]
        public async Task UpdateHiddenAccountsAsync_ExistingSettings_UpdatesHiddenAccounts()
        {
            // Arrange
            var context = GetInMemoryContext();
            var userId = Guid.NewGuid();
            var service = GetService(context);

            // Create initial settings
            await service.UpdateHiddenAccountsAsync(userId, new UpdateHiddenAccountsRequest
            {
                AccountIds = new List<Guid> { Guid.NewGuid() }
            });

            var newAccountIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            // Act
            var result = await service.UpdateHiddenAccountsAsync(userId, new UpdateHiddenAccountsRequest { AccountIds = newAccountIds });

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.HiddenAccountIds.Count);
        }

        [Fact]
        public async Task UpdateHiddenAccountsAsync_EmptyList_ClearsHiddenAccounts()
        {
            // Arrange
            var context = GetInMemoryContext();
            var userId = Guid.NewGuid();
            var service = GetService(context);

            // Create initial settings with hidden accounts
            await service.UpdateHiddenAccountsAsync(userId, new UpdateHiddenAccountsRequest
            {
                AccountIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }
            });

            // Act
            var result = await service.UpdateHiddenAccountsAsync(userId, new UpdateHiddenAccountsRequest { AccountIds = new List<Guid>() });

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.HiddenAccountIds);
        }

        #endregion

        #region DeleteSettingsAsync Tests

        [Fact]
        public async Task DeleteSettingsAsync_ExistingSettings_DeletesAndReturnsTrue()
        {
            // Arrange
            var context = GetInMemoryContext();
            var userId = Guid.NewGuid();
            var service = GetService(context);

            // Create settings first
            await service.CreateOrUpdateSettingsAsync(userId, new CreateSettingsRequest { Theme = "Dark" });

            // Act
            var result = await service.DeleteSettingsAsync(userId);

            // Assert
            Assert.True(result);
            var settings = await context.UserSettings.FirstOrDefaultAsync(s => s.UserId == userId);
            Assert.Null(settings);
        }

        [Fact]
        public async Task DeleteSettingsAsync_NonExistingSettings_ReturnsFalse()
        {
            // Arrange
            var context = GetInMemoryContext();
            var userId = Guid.NewGuid();
            var service = GetService(context);

            // Act
            var result = await service.DeleteSettingsAsync(userId);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region JSON Round-Trip Tests

        [Fact]
        public async Task HiddenAccounts_JSONRoundTrip_PreservesData()
        {
            // Arrange
            var context = GetInMemoryContext();
            var userId = Guid.NewGuid();
            var service = GetService(context);
            var accountIds = new List<Guid>
            {
                Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Guid.Parse("00000000-0000-0000-0000-000000000002"),
                Guid.Parse("00000000-0000-0000-0000-000000000003")
            };

            // Act
            var result = await service.UpdateHiddenAccountsAsync(userId, new UpdateHiddenAccountsRequest { AccountIds = accountIds });

            // Re-fetch from database to verify JSON serialization/deserialization
            var settings = await context.UserSettings.FirstAsync(s => s.UserId == userId);
            var deserializedList = settings.GetHiddenAccountsList();

            // Assert
            Assert.Equal(3, deserializedList.Count);
            Assert.Contains(Guid.Parse("00000000-0000-0000-0000-000000000001"), deserializedList);
            Assert.Contains(Guid.Parse("00000000-0000-0000-0000-000000000002"), deserializedList);
            Assert.Contains(Guid.Parse("00000000-0000-0000-0000-000000000003"), deserializedList);
        }

        [Fact]
        public async Task HiddenAccounts_InvalidJSON_ReturnsEmptyList()
        {
            // Arrange
            var context = GetInMemoryContext();
            var userId = Guid.NewGuid();

            // Manually insert invalid JSON
            context.UserSettings.Add(new BankSystem.Settings.Models.UserSettings
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Theme = "Light",
                HiddenAccountIds = "invalid json",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();

            // Act
            var settings = await context.UserSettings.FirstAsync(s => s.UserId == userId);
            var result = settings.GetHiddenAccountsList();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion
    }
}
