using Microsoft.EntityFrameworkCore;
using EHRPlatform.Common.Data;
using EHRPlatform.Common.Events;
using EHRPlatform.Services.Analytics.Domain.Entities;

namespace EHRPlatform.Services.Analytics.Data;

/// <summary>
/// DbContext for Analytics Service.
/// Manages metrics, dashboards, reports.
/// </summary>
public class AnalyticsContext : BaseDbContext
{
    public AnalyticsContext(DbContextOptions<AnalyticsContext> options) : base(options) { }

    public DbSet<AnalyticsMetric> Metrics { get; set; } = null!;
    public DbSet<Dashboard> Dashboards { get; set; } = null!;
    public DbSet<DashboardWidget> DashboardWidgets { get; set; } = null!;
    public DbSet<Report> Reports { get; set; } = null!;
    public DbSet<ReportExecution> ReportExecutions { get; set; } = null!;
    public DbSet<EventMetric> EventMetrics { get; set; } = null!;
    
    // ✓ Outbox Event Pattern - Ensures consistency across stores
    public DbSet<OutboxEvent> OutboxEvents { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AnalyticsContext).Assembly);
    }
}
