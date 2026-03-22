using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BankSystem.Users.DTOs;
using BankSystem.Users.Services;
using BankSystem.Users.Clients;

namespace BankSystem.Users.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly CoreServiceClient _coreClient;
        private readonly CreditServiceClient _creditClient;

        public UsersController(
            IUserService userService,
            CoreServiceClient coreClient,
            CreditServiceClient creditClient)
        {
            _userService = userService;
            _coreClient = coreClient;
            _creditClient = creditClient;
        }

        [Authorize(Roles = "admin,employee")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUserById(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        [AllowAnonymous]
        [HttpGet("email/{email}")]
        public async Task<ActionResult<UserDto>> GetUserByEmail(string email)
        {
            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        [AllowAnonymous]
        [HttpGet("{id}/exists")]
        public async Task<ActionResult<bool>> UserExists(Guid id)
        {
            var exists = await _userService.UserExistsAsync(id);
            return Ok(exists);
        }

        [Authorize]
        [HttpGet("{id}/details")]
        public async Task<ActionResult<ClientDetailsDto>> GetClientDetails(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();

            var accounts = await _coreClient.GetAccountsByClientIdAsync(id);
            var credits = await _creditClient.GetCreditsByClientIdAsync(id);

            var details = new ClientDetailsDto
            {
                User = user,
                Accounts = accounts ?? new List<AccountDto>(),
                Credits = credits ?? new List<CreditDto>()
            };

            return Ok(details);
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser(CreateUserRequest request)
        {
            try
            {
                var user = await _userService.CreateUserAsync(request);
                return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> RegisterUser(RegisterUserRequest request)
        {
            try
            {
                var createRequest = new CreateUserRequest
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    Phone = request.Phone,
                    Role = request.Role,
                    Password = request.Password
                };

                var user = await _userService.CreateUserAsync(createRequest);
                return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult<UserDto>> UpdateUser(Guid id, UpdateUserRequest request)
        {
            var user = await _userService.UpdateUserAsync(id, request);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result)
                return NotFound();
            return NoContent();
        }
    }
}