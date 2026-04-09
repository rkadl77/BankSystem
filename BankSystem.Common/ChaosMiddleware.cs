using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BankSystem.Common;

/// <summary>
/// Middleware that simulates chaos engineering by randomly returning 500 errors
/// 30% errors on odd minutes, 70% errors on even minutes
/// </summary>
public class ChaosMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ChaosMiddleware> _logger;

    // Paths to skip chaos testing
    private static readonly string[] SkipPaths = new[] { "/swagger", "/health", "/connect" };

    public ChaosMiddleware(RequestDelegate next, ILogger<ChaosMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip chaos for specific paths
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
        if (SkipPaths.Any(skipPath => path.StartsWith(skipPath)))
        {
            await _next(context);
            return;
        }

        // Determine error threshold based on minute
        var currentMinute = DateTime.UtcNow.Minute;
        var isEvenMinute = currentMinute % 2 == 0;
        var threshold = isEvenMinute ? 0.7 : 0.3;

        // Check if we should trigger chaos
        var randomValue = Random.Shared.NextDouble();
        
        if (randomValue < threshold)
        {
            // Get trace ID from context (set by TracingMiddleware)
            var traceId = context.Items["TraceId"]?.ToString() 
                          ?? context.Request.Headers["X-Trace-Id"].FirstOrDefault() 
                          ?? "unknown";

            _logger.LogWarning(
                "CHAOS: Simulating failure ({Threshold}% error rate) - TraceId={TraceId}, Minute={Minute} (Even={IsEven})",
                threshold * 100, traceId, currentMinute, isEvenMinute);

            // Return 500 error
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";

            var errorResponse = new
            {
                error = "Service temporarily unavailable",
                traceId = traceId
            };

            var jsonResponse = JsonSerializer.Serialize(errorResponse);
            await context.Response.WriteAsync(jsonResponse);
            return;
        }

        // Normal flow
        await _next(context);
    }
}
