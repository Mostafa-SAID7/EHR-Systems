namespace EHRPlatform.Services.Analytics.Persistence;

using EHRPlatform.Services.Analytics.Domain.Entities;
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

        // AnalyticsMetric
        modelBuilder.Entity<AnalyticsMetric>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.MetricName);
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => new { e.MetricDate, e.MetricName });
            entity.HasIndex(e => e.Timestamp);
            entity.Property(e => e.MetricName).HasMaxLength(100);
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.Unit).HasMaxLength(20);
        });

        // Dashboard
        modelBuilder.Entity<Dashboard>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.HasMany(e => e.Widgets).WithOne(w => w.Dashboard).HasForeignKey(w => w.DashboardId).OnDelete(DeleteBehavior.Cascade);
        });

        // DashboardWidget
        modelBuilder.Entity<DashboardWidget>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DashboardId);
            entity.Property(e => e.WidgetType).HasMaxLength(50);
            entity.Property(e => e.MetricName).HasMaxLength(100);
        });

        // Report
        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.CreatedBy);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.ReportType).HasMaxLength(50);
            entity.HasMany(e => e.Executions).WithOne(re => re.Report).HasForeignKey(re => re.ReportId).OnDelete(DeleteBehavior.Cascade);
        });

        // ReportExecution
        modelBuilder.Entity<ReportExecution>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ReportId);
            entity.HasIndex(e => e.ExecutedAt);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.ContentType).HasMaxLength(50);
        });

        // KPISummary
        modelBuilder.Entity<KPISummary>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.SummaryDate).IsUnique();
            entity.Property(e => e.SummaryDate).HasColumnType("date");
        });
    }
}
