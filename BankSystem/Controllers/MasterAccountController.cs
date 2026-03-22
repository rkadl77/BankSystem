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

        [HttpGet("id")]
        public async Task<ActionResult<Guid>> GetId()
        {
            if (!await IsAdminAsync())
                return Forbid();

            var id = await _masterAccountService.GetMasterAccountIdAsync();
            return Ok(id);
        }

        [HttpGet("balance")]
        public async Task<ActionResult<decimal>> GetBalance()
        {
            Console.WriteLine("=== GetBalance called ===");

            if (!await IsAdminAsync())
                return Forbid();

            var balance = await _masterAccountService.GetBalanceAsync();
            return Ok(balance);
        }

        [HttpGet("has-funds")]
        public async Task<ActionResult<bool>> HasFunds([FromQuery] decimal amount)
        {
            if (!await IsAdminAsync())
                return Forbid();

            var result = await _masterAccountService.HasSufficientFundsAsync(amount);
            return Ok(result);
        }
    }
}