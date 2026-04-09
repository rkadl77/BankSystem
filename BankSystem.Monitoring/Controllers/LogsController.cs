using BankSystem.Monitoring.Data;
using BankSystem.Monitoring.DTOs;
using BankSystem.Monitoring.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankSystem.Monitoring.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class LogsController : ControllerBase
{
    private readonly MonitoringDbContext _context;

    public LogsController(MonitoringDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Save a new request log
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> CreateLog([FromBody] RequestLog log)
    {
        log.Id = Guid.NewGuid();
        if (log.Timestamp == default)
        {
            log.Timestamp = DateTime.UtcNow;
        }

        _context.RequestLogs.Add(log);
        await _context.SaveChangesAsync();

        return Ok(new { log.Id });
    }

    /// <summary>
    /// Get last 100 logs with optional filters
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<RequestLog>>> GetLogs(
        [FromQuery] string? service,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var query = _context.RequestLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(service))
        {
            query = query.Where(l => l.Service == service);
        }

        if (from.HasValue)
        {
            query = query.Where(l => l.Timestamp >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(l => l.Timestamp <= to.Value);
        }

        var logs = await query
            .OrderByDescending(l => l.Timestamp)
            .Take(100)
            .ToListAsync();

        return Ok(logs);
    }

    /// <summary>
    /// Get aggregated statistics
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<LogsStatsResponse>> GetStats()
    {
        var allLogs = await _context.RequestLogs.ToListAsync();

        var totalRequests = allLogs.Count;
        if (totalRequests == 0)
        {
            return Ok(new LogsStatsResponse
            {
                TotalRequests = 0,
                ErrorRate = 0,
                AvgDurationMs = 0,
                PerService = new List<ServiceStats>()
            });
        }

        var errorCount = allLogs.Count(l => l.IsError);
        var errorRate = (double)errorCount / totalRequests * 100;
        var avgDuration = allLogs.Average(l => (double)l.DurationMs);

        var perService = allLogs
            .GroupBy(l => l.Service)
            .Select(g => new ServiceStats
            {
                Service = g.Key,
                Requests = g.Count(),
                Errors = g.Count(l => l.IsError),
                AvgDuration = g.Average(l => (double)l.DurationMs)
            })
            .OrderByDescending(s => s.Requests)
            .ToList();

        return Ok(new LogsStatsResponse
        {
            TotalRequests = totalRequests,
            ErrorRate = Math.Round(errorRate, 2),
            AvgDurationMs = Math.Round(avgDuration, 2),
            PerService = perService
        });
    }

    /// <summary>
    /// Get all logs for a specific trace ID
    /// </summary>
    [HttpGet("trace/{traceId}")]
    public async Task<ActionResult<List<RequestLog>>> GetTraceLogs(string traceId)
    {
        var logs = await _context.RequestLogs
            .Where(l => l.TraceId == traceId)
            .OrderBy(l => l.Timestamp)
            .ToListAsync();

        return Ok(logs);
    }
}
