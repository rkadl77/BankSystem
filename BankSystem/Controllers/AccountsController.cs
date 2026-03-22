using Microsoft.AspNetCore.Mvc;
using BankSystem.DTOs;
using BankSystem.Services;
using Microsoft.AspNetCore.Authorization;

namespace BankSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly ITransactionService _transactionService;

        public AccountsController(IAccountService accountService, ITransactionService transactionService)
        {
            _accountService = accountService;
            _transactionService = transactionService;
        }

        /// <summary>Gets an account by its unique identifier.</summary>
        /// <param name="id">The unique identifier of the account.</param>
        /// <returns>The account details.</returns>
        /// <response code="200">Returns the requested account.</response>
        /// <response code="404">If the account is not found.</response>
        /// <response code="401">Unauthorized.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AccountDto>> GetAccountById(Guid id)
        {
            var account = await _accountService.GetAccountByIdAsync(id);
            if (account == null)
                return NotFound();
            return Ok(account);
        }

        /// <summary>Gets all accounts for a specific client.</summary>
        /// <param name="clientId">The client's unique identifier.</param>
        /// <returns>A list of the client's accounts.</returns>
        /// <response code="200">Returns the list of accounts.</response>
        /// <response code="401">Unauthorized.</response>
        [HttpGet("client/{clientId}")]
        [ProducesResponseType(typeof(IEnumerable<AccountDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<AccountDto>>> GetAccountsByClientId(Guid clientId)
        {
            var accounts = await _accountService.GetAccountsByClientIdAsync(clientId);
            return Ok(accounts);
        }

        /// <summary>Creates a new account.</summary>
        /// <param name="request">The account creation details.</param>
        /// <returns>The newly created account.</returns>
        /// <response code="201">Returns the newly created account.</response>
        /// <response code="400">If the request is invalid.</response>
        /// <response code="401">Unauthorized.</response>
        [HttpPost]
        [ProducesResponseType(typeof(AccountDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AccountDto>> CreateAccount(CreateAccountRequest request)
        {
            try
            {
                var account = await _accountService.CreateAccountAsync(request);
                return CreatedAtAction(nameof(GetAccountById), new { id = account.Id }, account);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>Closes an existing account.</summary>
        /// <param name="id">The unique identifier of the account to close.</param>
        /// <returns>No content on success.</returns>
        /// <response code="204">If the account was successfully closed.</response>
        /// <response code="400">If the account cannot be closed (e.g. non-zero balance).</response>
        /// <response code="401">Unauthorized.</response>
        [HttpPut("{id}/close")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CloseAccount(Guid id)
        {
            var result = await _accountService.CloseAccountAsync(id);
            if (!result)
                return BadRequest("Cannot close account. Check if it exists, is active, and has zero balance.");
            return NoContent();
        }

        /// <summary>Gets the current balance of an account.</summary>
        /// <param name="id">The unique identifier of the account.</param>
        /// <returns>The current balance.</returns>
        /// <response code="200">Returns the account balance.</response>
        /// <response code="401">Unauthorized.</response>
        [HttpGet("{id}/balance")]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<decimal>> GetBalance(Guid id)
        {
            var balance = await _accountService.GetBalanceAsync(id);
            return Ok(balance);
        }

        /// <summary>Gets all transactions for a specific account.</summary>
        /// <param name="id">The unique identifier of the account.</param>
        /// <returns>A list of transactions.</returns>
        /// <response code="200">Returns the list of transactions.</response>
        /// <response code="401">Unauthorized.</response>
        [HttpGet("{id}/transactions")]
        [ProducesResponseType(typeof(IEnumerable<TransactionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<TransactionDto>>> GetAccountTransactions(Guid id)
        {
            var transactions = await _transactionService.GetTransactionsByAccountIdAsync(id);
            return Ok(transactions);
        }
    }
}