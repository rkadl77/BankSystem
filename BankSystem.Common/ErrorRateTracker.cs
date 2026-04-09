using System.Collections.Concurrent;

namespace BankSystem.Common;

/// <summary>
/// Tracks the error rate of the last 100 requests
/// </summary>
public class ErrorRateTracker
{
    private readonly ConcurrentQueue<bool> _requestResults = new();
    private const int MaxRequests = 100;
    private readonly object _lock = new();

    /// <summary>
    /// Record a request result
    /// </summary>
    /// <param name="isError">True if the request resulted in an error (status >= 500)</param>
    public void RecordRequest(bool isError)
    {
        lock (_lock)
        {
            _requestResults.Enqueue(isError);
            
            // Keep only the last 100 requests
            while (_requestResults.Count > MaxRequests)
            {
                _requestResults.TryDequeue(out _);
            }
        }
    }

    /// <summary>
    /// Get the current error rate percentage
    /// </summary>
    /// <returns>Tuple of (errorRate percentage, requestCount)</returns>
    public (double ErrorRate, int RequestCount) GetErrorRate()
    {
        var requests = _requestResults.ToArray();
        var count = requests.Length;
        
        if (count == 0)
        {
            return (0.0, 0);
        }

        var errorCount = requests.Count(x => x);
        var errorRate = (double)errorCount / count * 100;
        
        return (Math.Round(errorRate, 2), count);
    }
}
