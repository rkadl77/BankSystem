using Microsoft.AspNetCore.Mvc;
using BankSystem.Settings.DTOs;
using BankSystem.Settings.Services;

namespace BankSystem.Settings.Controllers
{
    [ApiController]
    [Route("api/settings")]
    public class SettingsController : ControllerBase
    {
        private readonly ISettingsService _settingsService;

        public SettingsController(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<UserSettingsDto>> GetSettings(Guid userId)
        {
            var settings = await _settingsService.GetSettingsAsync(userId);
            if (settings == null)
                return NotFound();
            return Ok(settings);
        }

        [HttpPost("{userId}")]
        public async Task<ActionResult<UserSettingsDto>> CreateOrUpdateSettings(Guid userId, CreateSettingsRequest request)
        {
            try
            {
                var settings = await _settingsService.CreateOrUpdateSettingsAsync(userId, request);
                return Ok(settings);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{userId}/theme")]
        public async Task<ActionResult<UserSettingsDto>> UpdateTheme(Guid userId, UpdateThemeRequest request)
        {
            try
            {
                var settings = await _settingsService.UpdateThemeAsync(userId, request);
                return Ok(settings);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{userId}/hidden-accounts")]
        public async Task<ActionResult<UserSettingsDto>> UpdateHiddenAccounts(Guid userId, UpdateHiddenAccountsRequest request)
        {
            try
            {
                var settings = await _settingsService.UpdateHiddenAccountsAsync(userId, request);
                return Ok(settings);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteSettings(Guid userId)
        {
            var result = await _settingsService.DeleteSettingsAsync(userId);
            if (!result)
                return NotFound();
            return Ok(new { message = "Settings deleted successfully" });
        }
    }
}
