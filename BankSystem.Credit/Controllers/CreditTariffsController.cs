using Microsoft.AspNetCore.Mvc;
using BankSystem.Credit.DTOs;
using BankSystem.Credit.Services;

namespace BankSystem.Credit.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CreditTariffsController : ControllerBase
    {
        private readonly ICreditTariffService _tariffService;

        public CreditTariffsController(ICreditTariffService tariffService)
        {
            _tariffService = tariffService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CreditTariffDto>>> GetAllTariffs()
        {
            var tariffs = await _tariffService.GetAllTariffsAsync();
            return Ok(tariffs);
        }

        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<CreditTariffDto>>> GetActiveTariffs()
        {
            var tariffs = await _tariffService.GetActiveTariffsAsync();
            return Ok(tariffs);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CreditTariffDto>> GetTariffById(Guid id)
        {
            var tariff = await _tariffService.GetTariffByIdAsync(id);
            if (tariff == null)
                return NotFound();
            return Ok(tariff);
        }

        [HttpPost]
        public async Task<ActionResult<CreditTariffDto>> CreateTariff(CreateCreditTariffRequest request)
        {
            var tariff = await _tariffService.CreateTariffAsync(request);
            return CreatedAtAction(nameof(GetTariffById), new { id = tariff.Id }, tariff);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CreditTariffDto>> UpdateTariff(Guid id, UpdateCreditTariffRequest request)
        {
            var tariff = await _tariffService.UpdateTariffAsync(id, request);
            if (tariff == null)
                return NotFound();
            return Ok(tariff);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTariff(Guid id)
        {
            var result = await _tariffService.DeleteTariffAsync(id);
            if (!result)
                return NotFound();
            return NoContent();
        }
    }
}