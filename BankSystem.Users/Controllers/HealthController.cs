using BankSystem.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankSystem.Users.Controllers;

[ApiController]
[Route("health")]
[AllowAnonymous]
public class HealthController : ControllerBase
{
    private readonly ErrorRateTracker _errorRateTracker;

    public HealthController(ErrorRateTracker errorRateTracker)
    {
        _errorRateTracker = errorRateTracker;
    }

    /// <summary>
    /// Get current error rate based on last 100 requests
    /// </summary>
    [HttpGet("error-rate")]
    public IActionResult GetErrorRate()
    {
        var (errorRate, requestCount) = _errorRateTracker.GetErrorRate();
        
        return Ok(new
        {
            ErrorRate = errorRate,
            RequestCount = requestCount
        });
    }
}
