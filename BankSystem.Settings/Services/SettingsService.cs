using BankSystem.Settings.Data;
using BankSystem.Settings.DTOs;
using BankSystem.Settings.Models;
using Microsoft.EntityFrameworkCore;

namespace BankSystem.Settings.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly SettingsContext _context;
        private static readonly string[] ValidThemes = { "Light", "Dark" };

        public SettingsService(SettingsContext context)
        {
            _context = context;
        }

        public async Task<UserSettingsDto?> GetSettingsAsync(Guid userId)
        {
            var settings = await _context.UserSettings
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (settings == null)
            {
                // Create default settings
                settings = new UserSettings
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Theme = "Light",
                    HiddenAccountIds = "[]",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.UserSettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            return MapToDto(settings);
        }

        public async Task<UserSettingsDto> CreateOrUpdateSettingsAsync(Guid userId, CreateSettingsRequest request)
        {
            var settings = await _context.UserSettings
                .FirstOrDefaultAsync(s => s.UserId == userId);

            var now = DateTime.UtcNow;

            if (settings == null)
            {
                settings = new UserSettings
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Theme = request.Theme ?? "Light",
                    HiddenAccountIds = request.HiddenAccountIds != null
                        ? System.Text.Json.JsonSerializer.Serialize(request.HiddenAccountIds)
                        : "[]",
                    CreatedAt = now,
                    UpdatedAt = now
                };
                _context.UserSettings.Add(settings);
            }
            else
            {
                if (request.Theme != null)
                    settings.Theme = request.Theme;
                if (request.HiddenAccountIds != null)
                    settings.SetHiddenAccounts(request.HiddenAccountIds);
                settings.UpdatedAt = now;
            }

            await _context.SaveChangesAsync();
            return MapToDto(settings);
        }

        public async Task<UserSettingsDto> UpdateThemeAsync(Guid userId, UpdateThemeRequest request)
        {
            if (!ValidThemes.Contains(request.Theme))
                throw new InvalidOperationException($"Invalid theme '{request.Theme}'. Valid themes are: {string.Join(", ", ValidThemes)}");

            var settings = await _context.UserSettings
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (settings == null)
            {
                settings = new UserSettings
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Theme = request.Theme,
                    HiddenAccountIds = "[]",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.UserSettings.Add(settings);
            }
            else
            {
                settings.Theme = request.Theme;
                settings.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return MapToDto(settings);
        }

        public async Task<UserSettingsDto> UpdateHiddenAccountsAsync(Guid userId, UpdateHiddenAccountsRequest request)
        {
            var settings = await _context.UserSettings
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (settings == null)
            {
                settings = new UserSettings
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Theme = "Light",
                    HiddenAccountIds = System.Text.Json.JsonSerializer.Serialize(request.AccountIds),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.UserSettings.Add(settings);
            }
            else
            {
                settings.SetHiddenAccounts(request.AccountIds);
                settings.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return MapToDto(settings);
        }

        public async Task<bool> DeleteSettingsAsync(Guid userId)
        {
            var settings = await _context.UserSettings
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (settings == null)
                return false;

            _context.UserSettings.Remove(settings);
            await _context.SaveChangesAsync();
            return true;
        }

        private UserSettingsDto MapToDto(UserSettings settings)
        {
            return new UserSettingsDto
            {
                Id = settings.Id,
                UserId = settings.UserId,
                Theme = settings.Theme,
                HiddenAccountIds = settings.GetHiddenAccountsList(),
                UpdatedAt = settings.UpdatedAt
            };
        }
    }
}
