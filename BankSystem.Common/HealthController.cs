using Microsoft.AspNetCore.Mvc;

namespace BankSystem.Common;

/// <summary>
/// Base controller for health check endpoints
/// Copy this to each service or inherit from it
/// </summary>
[ApiController]
[Route("health")]
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
