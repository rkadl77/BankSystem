using Microsoft.AspNetCore.Mvc;
using BankSystem.Auth.DTOs;
using BankSystem.Auth.Services;

namespace BankSystem.Auth.Controllers
{
    [ApiController]
    [Route("api/internal/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IAuthStorageService _authStorageService;

        public UsersController(IAuthStorageService authStorageService)
        {
            _authStorageService = authStorageService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAuthUser([FromBody] CreateAuthUserRequest request)
        {
            try
            {
                await _authStorageService.CreateAuthUserAsync(
                    request.UserId,
                    request.Email,
                    request.Password);
                return Ok(new { message = "Auth user created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}