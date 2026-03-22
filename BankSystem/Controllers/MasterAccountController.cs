using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BankSystem.Services;
using System.Security.Claims;

namespace BankSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MasterAccountController : ControllerBase
    {
        private readonly IMasterAccountService _masterAccountService;
        private readonly UserRoleService _userRoleService;

        public MasterAccountController(IMasterAccountService masterAccountService, UserRoleService userRoleService)
        {
            _masterAccountService = masterAccountService;
            _userRoleService = userRoleService;
        }

        private async Task<bool> IsAdminAsync()
        {
            Console.WriteLine("=== IsAdminAsync called ===");

            var userIdClaim = User.FindFirst("sub")?.Value
                ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            Console.WriteLine($"UserId claim: {userIdClaim}");

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                Console.WriteLine("UserId claim is null or invalid");
                return false;
            }

            Console.WriteLine($"Getting role for user: {userId}");
            var role = await _userRoleService.GetUserRoleAsync(userId);
            Console.WriteLine($"Role: {role}");

            return role == "admin";
        }

        /// <summary>Gets the master account's unique identifier.</summary>
        /// <returns>The master account identifier.</returns>
        /// <response code="200">Returns the master account ID.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="401">Unauthorized.</response>
        [HttpGet("id")]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<Guid>> GetId()
        {
            if (!await IsAdminAsync())
                return Forbid();

            var id = await _masterAccountService.GetMasterAccountIdAsync();
            return Ok(id);
        }

        /// <summary>Gets the master account balance.</summary>
        /// <returns>The current balance of the master account.</returns>
        /// <response code="200">Returns the balance.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="401">Unauthorized.</response>
        [HttpGet("balance")]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<decimal>> GetBalance()
        {
            Console.WriteLine("=== GetBalance called ===");

            if (!await IsAdminAsync())
                return Forbid();

            var balance = await _masterAccountService.GetBalanceAsync();
            return Ok(balance);
        }

        /// <summary>Checks if the master account has sufficient funds for a transaction.</summary>
        /// <param name="amount">The required amount.</param>
        /// <returns>True if there are sufficient funds, false otherwise.</returns>
        /// <response code="200">Returns the availability of funds.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="401">Unauthorized.</response>
        [HttpGet("has-funds")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<bool>> HasFunds([FromQuery] decimal amount)
        {
            if (!await IsAdminAsync())
                return Forbid();

            var result = await _masterAccountService.HasSufficientFundsAsync(amount);
            return Ok(result);
        }
    }
}