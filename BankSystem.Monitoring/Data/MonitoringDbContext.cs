using BankSystem.Monitoring.Models;
using Microsoft.EntityFrameworkCore;

namespace BankSystem.Monitoring.Data;

public class MonitoringDbContext : DbContext
{
    public MonitoringDbContext(DbContextOptions<MonitoringDbContext> options) : base(options)
    {
    }

    public DbSet<RequestLog> RequestLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RequestLog>(entity =>
        {
            entity.HasIndex(e => e.TraceId);
            entity.HasIndex(e => e.Service);
            entity.HasIndex(e => e.Timestamp);
        });
    }
}
