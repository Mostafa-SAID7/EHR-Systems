namespace EHRPlatform.Services.Analytics.Persistence;

using EHRPlatform.Services.Analytics.Domain.Entities;
using EHRPlatform.Services.Analytics.Persistence.EntityConfigurations;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Database context for Analytics microservice.
/// </summary>
public interface IAnalyticsDbContext
{
    DbSet<AnalyticsMetric> AnalyticsMetrics { get; }
    DbSet<Dashboard> Dashboards { get; }
    DbSet<DashboardWidget> DashboardWidgets { get; }
    DbSet<Report> Reports { get; }
    DbSet<ReportExecution> ReportExecutions { get; }
    DbSet<KPISummary> KPISummaries { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public class AnalyticsDbContext : DbContext, IAnalyticsDbContext
{
    public DbSet<AnalyticsMetric> AnalyticsMetrics { get; set; } = null!;
    public DbSet<Dashboard> Dashboards { get; set; } = null!;
    public DbSet<DashboardWidget> DashboardWidgets { get; set; } = null!;
    public DbSet<Report> Reports { get; set; } = null!;
    public DbSet<ReportExecution> ReportExecutions { get; set; } = null!;
    public DbSet<KPISummary> KPISummaries { get; set; } = null!;

    public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        modelBuilder.ApplyConfiguration(new AnalyticsMetricConfiguration());
        modelBuilder.ApplyConfiguration(new DashboardConfiguration());
        modelBuilder.ApplyConfiguration(new DashboardWidgetConfiguration());
        modelBuilder.ApplyConfiguration(new ReportConfiguration());
        modelBuilder.ApplyConfiguration(new ReportExecutionConfiguration());
        modelBuilder.ApplyConfiguration(new KPISummaryConfiguration());
    }
}
