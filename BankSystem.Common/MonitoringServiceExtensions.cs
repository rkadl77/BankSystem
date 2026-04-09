using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BankSystem.Common;

/// <summary>
/// Extension methods for registering tracing and monitoring services
/// </summary>
public static class MonitoringServiceExtensions
{
    /// <summary>
    /// Register tracing and monitoring services
    /// </summary>
    public static IServiceCollection AddBankSystemMonitoring(this IServiceCollection services)
    {
        // Register ErrorRateTracker as singleton
        services.AddSingleton<ErrorRateTracker>();
        
        // Add HttpClient for Monitoring service
        services.AddHttpClient("Monitoring");
        
        return services;
    }

    /// <summary>
    /// Use the TracingMiddleware (should be called FIRST in the pipeline)
    /// </summary>
    public static IApplicationBuilder UseBankSystemTracing(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TracingMiddleware>();
    }

    /// <summary>
    /// Use the ChaosMiddleware (simulates random failures)
    /// Should be called after authentication but before MapControllers
    /// </summary>
    public static IApplicationBuilder UseChaosEngineering(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ChaosMiddleware>();
    }
}
