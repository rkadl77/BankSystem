namespace BankSystem.Monitoring.DTOs;

public class ServiceStats
{
    public string Service { get; set; } = string.Empty;
    public int Requests { get; set; }
    public int Errors { get; set; }
    public double AvgDuration { get; set; }
}

public class LogsStatsResponse
{
    public int TotalRequests { get; set; }
    public double ErrorRate { get; set; }
    public double AvgDurationMs { get; set; }
    public List<ServiceStats> PerService { get; set; } = new();
}
