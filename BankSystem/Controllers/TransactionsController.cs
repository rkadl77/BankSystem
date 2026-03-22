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

        /// <summary>Processes a deposit transaction.</summary>
        /// <param name="request">The deposit details.</param>
        /// <returns>The transaction details.</returns>
        /// <response code="200">Returns the successfully processed transaction.</response>
        /// <response code="400">If the request is invalid.</response>
        /// <response code="401">Unauthorized.</response>
        [HttpPost("deposit")]
        [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

        /// <summary>Processes a withdraw transaction.</summary>
        /// <param name="request">The withdraw details.</param>
        /// <returns>The transaction details.</returns>
        /// <response code="200">Returns the successfully processed transaction.</response>
        /// <response code="400">If the request is invalid, e.g. insufficient funds.</response>
        /// <response code="401">Unauthorized.</response>
        [HttpPost("withdraw")]
        [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

        /// <summary>Processes a transfer between two accounts.</summary>
        /// <param name="request">The transfer details.</param>
        /// <returns>A success response.</returns>
        /// <response code="200">If the transfer was successful.</response>
        /// <response code="400">If the request is invalid, e.g. insufficient funds.</response>
        /// <response code="401">Unauthorized.</response>
        [HttpPost("transfer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

        /// <summary>Gets all transactions for a specific account.</summary>
        /// <param name="accountId">The unique identifier of the account.</param>
        /// <returns>A list of transactions.</returns>
        /// <response code="200">Returns the list of transactions.</response>
        /// <response code="401">Unauthorized.</response>
        [HttpGet("account/{accountId}")]
        [ProducesResponseType(typeof(IEnumerable<TransactionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<TransactionDto>>> GetAccountTransactions(Guid accountId)
        {
            var transactions = await _transactionService.GetTransactionsByAccountIdAsync(accountId);
            return Ok(transactions);
        }
    }
}