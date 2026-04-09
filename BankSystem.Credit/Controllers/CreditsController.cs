using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BankSystem.Credit.DTOs;
using BankSystem.Credit.Services;

namespace BankSystem.Credit.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CreditsController : ControllerBase
    {
        private readonly ICreditService _creditService;

        public CreditsController(ICreditService creditService)
        {
            _creditService = creditService;
        }

        [Authorize(Roles = "admin,employee")]
        [HttpGet("client/{clientId}")]
        public async Task<ActionResult<IEnumerable<CreditDto>>> GetCreditsByClientId(Guid clientId)
        {
            var credits = await _creditService.GetCreditsByClientIdAsync(clientId);
            return Ok(credits);
        }

        [Authorize(Roles = "admin,employee")]
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<CreditDto>>> GetActiveCredits()
        {
            var credits = await _creditService.GetActiveCreditsAsync();
            return Ok(credits);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<CreditDetailsDto>> GetCreditById(Guid id)
        {
            var credit = await _creditService.GetCreditByIdAsync(id);
            if (credit == null)
                return NotFound();
            return Ok(credit);
        }

        [Authorize(Roles = "admin,employee")]
        [HttpGet("client/{clientId}/total-debt")]
        public async Task<ActionResult<decimal>> GetTotalDebtByClientId(Guid clientId)
        {
            var debt = await _creditService.GetTotalDebtByClientIdAsync(clientId);
            return Ok(debt);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<CreditDto>> CreateCredit(CreateCreditRequest request)
        {
            try
            {
                var credit = await _creditService.CreateCreditAsync(request);
                return CreatedAtAction(nameof(GetCreditById), new { id = credit.Id }, credit);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("repay")]
        public async Task<IActionResult> RepayCredit(RepayCreditRequest request)
        {
            var result = await _creditService.RepayCreditAsync(request);
            if (!result)
                return BadRequest("Repayment failed");
            return Ok(new { message = "Repayment successful" });
        }

        [HttpGet("client/{clientId}/overdue")]
        public async Task<ActionResult<IEnumerable<CreditDto>>> GetOverdueCredits(Guid clientId)
        {
            var credits = await _creditService.GetOverdueCreditsAsync(clientId);
            return Ok(credits);
        }

        [HttpGet("client/{clientId}/rating")]
        public async Task<ActionResult<CreditRatingDto>> GetCreditRating(Guid clientId)
        {
            var rating = await _creditService.CalculateCreditRatingAsync(clientId);
            return Ok(rating);
        }

        [HttpGet("{id}/details")]
        public async Task<ActionResult<CreditDetailDto>> GetCreditDetails(Guid id)
        {
            var credit = await _creditService.GetCreditDetailsAsync(id);
            if (credit == null)
                return NotFound();
            return Ok(credit);
        }

        [HttpPost("{id}/update-status")]
        public async Task<IActionResult> UpdateCreditStatus(Guid id)
        {
            var result = await _creditService.UpdateCreditStatusAsync(id);
            if (!result)
                return NotFound();
            return Ok(new { message = "Status updated successfully" });
        }
    }
}