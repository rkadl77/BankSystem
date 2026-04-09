using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;

namespace BankSystem.Common;

/// <summary>
/// Shared Polly resilience policies for HTTP clients
/// </summary>
public static class PollyPolicies
{
    /// <summary>
    /// Creates a retry policy with exponential backoff
    /// - 3 retries
    /// - Backoff: 1s, 2s, 4s
    /// - Retries on: HttpRequestException, 500, 502, 503, 504
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(string serviceName)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError() // HttpRequestException, 500, 502, 503, 504
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt - 1)), // 1s, 2s, 4s
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    var traceId = context.TryGetValue("TraceId", out var tid) ? tid?.ToString() : "unknown";
                    var statusCode = outcome.Result?.StatusCode;
                    
                    // Log via console since we don't have access to logger here
                    Console.WriteLine($"[RETRY] Service={serviceName} Attempt={retryAttempt} Delay={timespan.TotalSeconds}s TraceId={traceId} StatusCode={statusCode}");
                });
    }

    /// <summary>
    /// Creates a circuit breaker policy
    /// - Opens if >70% failures in last 10 requests
    /// - Break duration: 30 seconds
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(string serviceName)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 7, // 7 out of 10 = 70%
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (outcome, breakDelay) =>
                {
                    Console.WriteLine($"[CIRCUIT OPEN] Service={serviceName} for {breakDelay.TotalSeconds}s - Too many failures");
                },
                onReset: () =>
                {
                    Console.WriteLine($"[CIRCUIT CLOSED] Service={serviceName} - Recovered");
                },
                onHalfOpen: () =>
                {
                    Console.WriteLine($"[CIRCUIT HALF-OPEN] Service={serviceName} - Testing...");
                });
    }

    /// <summary>
    /// Creates a fallback policy for when circuit is open
    /// Returns 503 with retry-after message
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetFallbackPolicy()
    {
        return Policy<HttpResponseMessage>
            .Handle<Polly.CircuitBreaker.BrokenCircuitException>()
            .FallbackAsync(
                fallbackAction: async (cancellationToken) =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                    {
                        Content = new StringContent(
                            System.Text.Json.JsonSerializer.Serialize(new
                            {
                                error = "Service temporarily unavailable (circuit open)",
                                retryAfter = 30
                            }),
                            System.Text.Encoding.UTF8,
                            "application/json")
                    };
                    
                    return await Task.FromResult(response);
                });
    }

    /// <summary>
    /// Combines all policies: Fallback → Circuit Breaker → Retry
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy(string serviceName)
    {
        var fallback = GetFallbackPolicy();
        var circuitBreaker = GetCircuitBreakerPolicy(serviceName);
        var retry = GetRetryPolicy(serviceName);

        return fallback.WrapAsync(circuitBreaker).WrapAsync(retry);
    }
}
