using Microsoft.EntityFrameworkCore;
using EHRPlatform.Services.Analytics.Domain.Entities;

namespace EHRPlatform.Services.Analytics.Data.Seeds;

/// <summary>
/// Seed data for Analytics (Metrics, Dashboards, Reports).
/// </summary>
public static class AnalyticsSeed
{
    public static void SeedAnalytics(this ModelBuilder modelBuilder)
    {
        var dashboardId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var userId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var metricId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        modelBuilder.Entity<Dashboard>().HasData(
            new Dashboard
            {
                Id = dashboardId,
                Name = "Executive Dashboard",
                UserId = userId,
                Description = "Default executive analytics dashboard",
                CreatedAt = DateTime.UtcNow
            }
        );

        modelBuilder.Entity<AnalyticsMetric>().HasData(
            new AnalyticsMetric
            {
                Id = metricId,
                MetricName = "Patient Encounters",
                Category = "Clinical",
                Value = 1250,
                PeriodStart = DateTime.UtcNow.AddDays(-30),
                PeriodEnd = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            }
        );
    }
}
