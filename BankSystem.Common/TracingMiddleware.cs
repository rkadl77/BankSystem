using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BankSystem.Common;

/// <summary>
/// Middleware that traces all requests and sends logs to the Monitoring service
/// </summary>
public class TracingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TracingMiddleware> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _serviceName;
    private readonly string _monitoringUrl;

    public TracingMiddleware(
        RequestDelegate next,
        ILogger<TracingMiddleware> logger,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _serviceName = configuration["ServiceName"] ?? "Unknown";
        _monitoringUrl = configuration["Monitoring:Url"] ?? "http://localhost:5200/api/logs";
    }

    public async Task InvokeAsync(HttpContext context, ErrorRateTracker errorRateTracker)
    {
        // 1. Read or generate TraceId
        var traceId = context.Request.Headers["X-Trace-Id"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(traceId))
        {
            traceId = Guid.NewGuid().ToString();
        }

        // 2. Add to response headers
        context.Response.Headers["X-Trace-Id"] = traceId;

        // 3. Add to HttpContext.Items
        context.Items["TraceId"] = traceId;

        // 4. Start stopwatch
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // 5. Execute next middleware
            await _next(context);
        }
        finally
        {
            // 6. Stop stopwatch
            stopwatch.Stop();

            // 7. Build log entry
            var statusCode = context.Response.StatusCode;
            var isError = statusCode >= 500;

            var logEntry = new
            {
                TraceId = traceId,
                Service = _serviceName,
                Method = context.Request.Method,
                Path = context.Request.Path.ToString(),
                StatusCode = statusCode,
                DurationMs = stopwatch.ElapsedMilliseconds,
                IsError = isError,
                Timestamp = DateTime.UtcNow
            };

            // Log structured entry locally
            _logger.LogInformation(
                "TraceId={TraceId} Service={Service} Method={Method} Path={Path} StatusCode={StatusCode} DurationMs={DurationMs} IsError={IsError}",
                traceId, _serviceName, logEntry.Method, logEntry.Path, logEntry.StatusCode, logEntry.DurationMs, isError);

            // Record in error rate tracker
            errorRateTracker.RecordRequest(isError);

            // 8. Send to Monitoring service (fire-and-forget)
            _ = Task.Run(async () =>
            {
                try
                {
                    using var httpClient = _httpClientFactory.CreateClient("Monitoring");
                    using var content = new StringContent(
                        JsonSerializer.Serialize(logEntry),
                        System.Text.Encoding.UTF8,
                        "application/json");
                    
                    await httpClient.PostAsync(_monitoringUrl, content);
                }
                catch (Exception ex)
                {
                    // Ignore failures - fire-and-forget
                    _logger.LogDebug(ex, "Failed to send log to Monitoring service");
                }
            });
        }
    }
}
