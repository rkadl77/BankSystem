using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BankSystem.Credit.DTOs;
using BankSystem.Credit.Services;

namespace BankSystem.Credit.Controllers
{
    [Authorize(Roles = "admin,employee")]
    [ApiController]
    [Route("api/[controller]")]
    public class CreditTariffsController : ControllerBase
    {
        private readonly ICreditTariffService _tariffService;

        public CreditTariffsController(ICreditTariffService tariffService)
        {
            _tariffService = tariffService;
        }

        /// <summary>Gets all credit tariffs.</summary>
        /// <returns>A list of credit tariffs.</returns>
        /// <response code="200">Returns the list of tariffs.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="403">Forbidden.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CreditTariffDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<CreditTariffDto>>> GetAllTariffs()
        {
            var tariffs = await _tariffService.GetAllTariffsAsync();
            return Ok(tariffs);
        }

        /// <summary>Gets all active credit tariffs.</summary>
        /// <returns>A list of active credit tariffs.</returns>
        /// <response code="200">Returns the list of active tariffs.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="403">Forbidden.</response>
        [HttpGet("active")]
        [ProducesResponseType(typeof(IEnumerable<CreditTariffDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<CreditTariffDto>>> GetActiveTariffs()
        {
            var tariffs = await _tariffService.GetActiveTariffsAsync();
            return Ok(tariffs);
        }

        /// <summary>Gets a credit tariff by its unique identifier.</summary>
        /// <param name="id">The tariff's unique identifier.</param>
        /// <returns>The tariff details.</returns>
        /// <response code="200">Returns the requested tariff.</response>
        /// <response code="404">If the tariff is not found.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="403">Forbidden.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CreditTariffDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<CreditTariffDto>> GetTariffById(Guid id)
        {
            var tariff = await _tariffService.GetTariffByIdAsync(id);
            if (tariff == null)
                return NotFound();
            return Ok(tariff);
        }

        /// <summary>Creates a new credit tariff.</summary>
        /// <param name="request">The tariff creation details.</param>
        /// <returns>The newly created tariff.</returns>
        /// <response code="201">Returns the newly created tariff.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="403">Forbidden.</response>
        [HttpPost]
        [ProducesResponseType(typeof(CreditTariffDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<CreditTariffDto>> CreateTariff(CreateCreditTariffRequest request)
        {
            var tariff = await _tariffService.CreateTariffAsync(request);
            return CreatedAtAction(nameof(GetTariffById), new { id = tariff.Id }, tariff);
        }

        /// <summary>Updates an existing credit tariff.</summary>
        /// <param name="id">The tariff's unique identifier.</param>
        /// <param name="request">The tariff update details.</param>
        /// <returns>The updated tariff.</returns>
        /// <response code="200">Returns the updated tariff.</response>
        /// <response code="404">If the tariff is not found.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="403">Forbidden.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(CreditTariffDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<CreditTariffDto>> UpdateTariff(Guid id, UpdateCreditTariffRequest request)
        {
            var tariff = await _tariffService.UpdateTariffAsync(id, request);
            if (tariff == null)
                return NotFound();
            return Ok(tariff);
        }

        /// <summary>Deletes a credit tariff.</summary>
        /// <param name="id">The tariff's unique identifier.</param>
        /// <returns>No content on success.</returns>
        /// <response code="204">If the tariff was successfully deleted.</response>
        /// <response code="404">If the tariff is not found.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="403">Forbidden.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteTariff(Guid id)
        {
            var result = await _tariffService.DeleteTariffAsync(id);
            if (!result)
                return NotFound();
            return NoContent();
        }
    }
}