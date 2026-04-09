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

        /// <summary>Gets all users.</summary>
        /// <returns>A list of all users.</returns>
        /// <response code="200">Returns the list of users.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="403">Forbidden.</response>
        [Authorize(Roles = "admin,employee")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        /// <summary>Gets a user by their unique identifier.</summary>
        /// <param name="id">The user's unique identifier.</param>
        /// <returns>The user details.</returns>
        /// <response code="200">Returns the requested user.</response>
        /// <response code="404">If the user is not found.</response>
        /// <response code="401">Unauthorized.</response>
        [Authorize]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserDto>> GetUserById(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        /// <summary>[AllowAnonymous] Internal use: Gets a user by their unique identifier.</summary>
        /// <param name="id">The user's unique identifier.</param>
        /// <returns>The user details.</returns>
        [AllowAnonymous]
        [HttpGet("internal/{id}")]
        public async Task<ActionResult<UserDto>> GetUserByIdInternal(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        /// <summary>[AllowAnonymous] Gets a user by their email address.</summary>
        /// <param name="email">The email address to search for.</param>
        /// <returns>The user details.</returns>
        /// <response code="200">Returns the requested user.</response>
        /// <response code="404">If the user is not found.</response>
        [AllowAnonymous]
        [HttpGet("email/{email}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserDto>> GetUserByEmail(string email)
        {
            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        /// <summary>[AllowAnonymous] Checks if a user exists by their ID.</summary>
        /// <param name="id">The user's unique identifier.</param>
        /// <returns>True if the user exists, false otherwise.</returns>
        /// <response code="200">Returns the existence status.</response>
        [AllowAnonymous]
        [HttpGet("{id}/exists")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<ActionResult<bool>> UserExists(Guid id)
        {
            var exists = await _userService.UserExistsAsync(id);
            return Ok(exists);
        }

        /// <summary>Gets comprehensive client details including accounts and credits.</summary>
        /// <param name="id">The client's unique identifier.</param>
        /// <returns>The client details.</returns>
        /// <response code="200">Returns the client's comprehensive details.</response>
        /// <response code="404">If the user is not found.</response>
        /// <response code="401">Unauthorized.</response>
        [Authorize]
        [HttpGet("{id}/details")]
        [ProducesResponseType(typeof(ClientDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

        /// <summary>Creates a new user.</summary>
        /// <param name="request">The user creation details.</param>
        /// <returns>The newly created user.</returns>
        /// <response code="201">Returns the newly created user.</response>
        /// <response code="400">If the request is invalid.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="403">Forbidden.</response>
        [Authorize(Roles = "admin")]
        [HttpPost]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

        /// <summary>[AllowAnonymous] Registers a new user.</summary>
        /// <param name="request">The user registration details.</param>
        /// <returns>The newly registered user.</returns>
        /// <response code="201">Returns the newly registered user.</response>
        /// <response code="400">If the registration is invalid.</response>
        [AllowAnonymous]
        [HttpPost("register")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

        /// <summary>Updates an existing user.</summary>
        /// <param name="id">The user's unique identifier.</param>
        /// <param name="request">The user update details.</param>
        /// <returns>The updated user.</returns>
        /// <response code="200">Returns the updated user.</response>
        /// <response code="404">If the user is not found.</response>
        /// <response code="401">Unauthorized.</response>
        [Authorize]
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserDto>> UpdateUser(Guid id, UpdateUserRequest request)
        {
            var user = await _userService.UpdateUserAsync(id, request);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        /// <summary>Deletes a user.</summary>
        /// <param name="id">The user's unique identifier.</param>
        /// <returns>No content on success.</returns>
        /// <response code="204">If the user was successfully deleted.</response>
        /// <response code="404">If the user is not found.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="403">Forbidden.</response>
        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result)
                return NotFound();
            return NoContent();
        }
    }
}