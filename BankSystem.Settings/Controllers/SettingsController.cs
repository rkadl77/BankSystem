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

        /// <summary>Gets settings for a specific user.</summary>
        /// <param name="userId">The user's unique identifier.</param>
        /// <returns>The user's settings.</returns>
        /// <response code="200">Returns the user's settings.</response>
        /// <response code="404">If the settings are not found.</response>
        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(UserSettingsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserSettingsDto>> GetSettings(Guid userId)
        {
            var settings = await _settingsService.GetSettingsAsync(userId);
            if (settings == null)
                return NotFound();
            return Ok(settings);
        }

        /// <summary>Creates or updates settings for a specific user.</summary>
        /// <param name="userId">The user's unique identifier.</param>
        /// <param name="request">The settings details.</param>
        /// <returns>The created or updated settings.</returns>
        /// <response code="200">Returns the settings.</response>
        /// <response code="400">If the request is invalid.</response>
        [HttpPost("{userId}")]
        [ProducesResponseType(typeof(UserSettingsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

        /// <summary>Updates the theme preference for a specific user.</summary>
        /// <param name="userId">The user's unique identifier.</param>
        /// <param name="request">The theme update details.</param>
        /// <returns>The updated settings.</returns>
        /// <response code="200">Returns the updated settings.</response>
        /// <response code="400">If the request is invalid.</response>
        [HttpPatch("{userId}/theme")]
        [ProducesResponseType(typeof(UserSettingsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

        /// <summary>Updates the hidden accounts for a specific user.</summary>
        /// <param name="userId">The user's unique identifier.</param>
        /// <param name="request">The hidden accounts update details.</param>
        /// <returns>The updated settings.</returns>
        /// <response code="200">Returns the updated settings.</response>
        /// <response code="400">If the request is invalid.</response>
        [HttpPatch("{userId}/hidden-accounts")]
        [ProducesResponseType(typeof(UserSettingsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

        /// <summary>Deletes settings for a specific user.</summary>
        /// <param name="userId">The user's unique identifier.</param>
        /// <returns>A success response.</returns>
        /// <response code="200">If the settings were successfully deleted.</response>
        /// <response code="404">If the settings are not found.</response>
        [HttpDelete("{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteSettings(Guid userId)
        {
            var result = await _settingsService.DeleteSettingsAsync(userId);
            if (!result)
                return NotFound();
            return Ok(new { message = "Settings deleted successfully" });
        }
    }
}
