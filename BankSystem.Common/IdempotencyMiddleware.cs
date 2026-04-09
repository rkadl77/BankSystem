using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace BankSystem.Common;

/// <summary>
/// Middleware that ensures idempotent requests return the same response
/// for duplicate requests with the same Idempotency-Key header
/// </summary>
public class IdempotencyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private const string IdempotencyKeyHeader = "Idempotency-Key";
    private const string IdempotentReplayedHeader = "Idempotent-Replayed";
    private const string CacheKeyPrefix = "idempotency_";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(24);

    public IdempotencyMiddleware(RequestDelegate next, IMemoryCache cache)
    {
        _next = next;
        _cache = cache;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only intercept POST and PUT requests
        var method = context.Request.Method.ToUpperInvariant();
        if (method != "POST" && method != "PUT")
        {
            await _next(context);
            return;
        }

        // Read Idempotency-Key header
        var idempotencyKey = context.Request.Headers[IdempotencyKeyHeader].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            await _next(context);
            return;
        }

        var cacheKey = $"{CacheKeyPrefix}{idempotencyKey}";

        // Check if we have a cached response
        if (_cache.TryGetValue(cacheKey, out CachedResponse? cachedResponse))
        {
            // Return cached response
            context.Response.ContentType = cachedResponse!.ContentType;
            context.Response.StatusCode = cachedResponse.StatusCode;
            context.Response.Headers[IdempotentReplayedHeader] = "true";

            await context.Response.Body.WriteAsync(
                cachedResponse.Body, 
                0, 
                cachedResponse.Body.Length);
            
            return;
        }

        // Buffer the response body
        var originalResponseBody = context.Response.Body;
        using var responseBodyBuffer = new MemoryStream();
        context.Response.Body = responseBodyBuffer;

        try
        {
            // Execute next middleware
            await _next(context);

            // Read the response body
            responseBodyBuffer.Position = 0;
            var responseBody = new byte[responseBodyBuffer.Length];
            await responseBodyBuffer.ReadAsync(responseBody, 0, responseBody.Length);

            // Cache the response
            var cachedResponseToStore = new CachedResponse
            {
                StatusCode = context.Response.StatusCode,
                Body = responseBody,
                ContentType = context.Response.ContentType ?? "application/json"
            };

            _cache.Set(cacheKey, cachedResponseToStore, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheTtl
            });

            // Write response to original stream
            responseBodyBuffer.Position = 0;
            await responseBodyBuffer.CopyToAsync(originalResponseBody);
        }
        finally
        {
            context.Response.Body = originalResponseBody;
        }
    }
}

/// <summary>
/// Cached response data for idempotency
/// </summary>
public class CachedResponse
{
    public int StatusCode { get; set; }
    public byte[] Body { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = "application/json";
}
