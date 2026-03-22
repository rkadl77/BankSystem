using Microsoft.AspNetCore.Mvc;
using BankSystem.DTOs;
using BankSystem.Services;
using Microsoft.AspNetCore.Authorization;

namespace BankSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionsController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpPost("deposit")]
        public async Task<ActionResult<TransactionDto>> Deposit(CreateTransactionRequest request)
        {
            try
            {
                var transaction = await _transactionService.DepositAsync(request);
                return Ok(transaction);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("withdraw")]
        public async Task<ActionResult<TransactionDto>> Withdraw(CreateTransactionRequest request)
        {
            try
            {
                var transaction = await _transactionService.WithdrawAsync(request);
                return Ok(transaction);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("transfer")]
        public async Task<IActionResult> Transfer(TransferRequest request)
        {
            try
            {
                var result = await _transactionService.TransferAsync(request);
                return Ok(new { message = "Transfer completed successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("account/{accountId}")]
        public async Task<ActionResult<IEnumerable<TransactionDto>>> GetAccountTransactions(Guid accountId)
        {
            var transactions = await _transactionService.GetTransactionsByAccountIdAsync(accountId);
            return Ok(transactions);
        }
    }
}