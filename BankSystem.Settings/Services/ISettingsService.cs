using BankSystem.Settings.DTOs;

namespace BankSystem.Settings.Services
{
    public interface ISettingsService
    {
        Task<UserSettingsDto?> GetSettingsAsync(Guid userId);
        Task<UserSettingsDto> CreateOrUpdateSettingsAsync(Guid userId, CreateSettingsRequest request);
        Task<UserSettingsDto> UpdateThemeAsync(Guid userId, UpdateThemeRequest request);
        Task<UserSettingsDto> UpdateHiddenAccountsAsync(Guid userId, UpdateHiddenAccountsRequest request);
        Task<bool> DeleteSettingsAsync(Guid userId);
    }
}
