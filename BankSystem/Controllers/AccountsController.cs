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

        [HttpGet("{id}")]
        public async Task<ActionResult<AccountDto>> GetAccountById(Guid id)
        {
            var account = await _accountService.GetAccountByIdAsync(id);
            if (account == null)
                return NotFound();
            return Ok(account);
        }

        [HttpGet("client/{clientId}")]
        public async Task<ActionResult<IEnumerable<AccountDto>>> GetAccountsByClientId(Guid clientId)
        {
            var accounts = await _accountService.GetAccountsByClientIdAsync(clientId);
            return Ok(accounts);
        }

        [HttpPost]
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

        [HttpPut("{id}/close")]
        public async Task<IActionResult> CloseAccount(Guid id)
        {
            var result = await _accountService.CloseAccountAsync(id);
            if (!result)
                return BadRequest("Cannot close account. Check if it exists, is active, and has zero balance.");
            return NoContent();
        }

        [HttpGet("{id}/balance")]
        public async Task<ActionResult<decimal>> GetBalance(Guid id)
        {
            var balance = await _accountService.GetBalanceAsync(id);
            return Ok(balance);
        }

        [HttpGet("{id}/transactions")]
        public async Task<ActionResult<IEnumerable<TransactionDto>>> GetAccountTransactions(Guid id)
        {
            var transactions = await _transactionService.GetTransactionsByAccountIdAsync(id);
            return Ok(transactions);
        }
    }
}