namespace EHRPlatform.Services.Analytics.Domain.Specifications;

using EHRPlatform.Services.Analytics.Domain.Entities;
using EHRPlatform.Services.Analytics.Domain.ValueObjects;

/// <summary>
/// Specifications for AnalyticsMetric queries
/// </summary>
public static class MetricSpecifications
{
    /// <summary>
    /// Gets metrics for tenant
    /// </summary>
    public static IQueryable<AnalyticsMetric> ForTenant(IQueryable<AnalyticsMetric> query, long tenantId)
    {
        return query.Where(m => m.TenantId == tenantId);
    }

    /// <summary>
    /// Gets metrics by name
    /// </summary>
    public static IQueryable<AnalyticsMetric> ByName(IQueryable<AnalyticsMetric> query, string metricName)
    {
        return query.Where(m => m.MetricName == metricName);
    }

    /// <summary>
    /// Gets metrics by category
    /// </summary>
    public static IQueryable<AnalyticsMetric> ByCategory(IQueryable<AnalyticsMetric> query, string category)
    {
        return query.Where(m => m.Category == category);
    }

    /// <summary>
    /// Gets metrics within date range
    /// </summary>
    public static IQueryable<AnalyticsMetric> InDateRange(IQueryable<AnalyticsMetric> query, DateRange dateRange)
    {
        return query.Where(m => m.Timestamp >= dateRange.StartDate && m.Timestamp <= dateRange.EndDate);
    }

    /// <summary>
    /// Gets recent metrics (last N hours)
    /// </summary>
    public static IQueryable<AnalyticsMetric> RecentMetrics(IQueryable<AnalyticsMetric> query, int lastHours)
    {
        var cutoffTime = DateTime.UtcNow.AddHours(-lastHours);
        return query.Where(m => m.Timestamp >= cutoffTime);
    }

    /// <summary>
    /// Gets metrics by dimension filter
    /// </summary>
    public static IQueryable<AnalyticsMetric> ByDimension1(IQueryable<AnalyticsMetric> query, string? dimension1)
    {
        if (string.IsNullOrEmpty(dimension1))
            return query;
        return query.Where(m => m.Dimension1 == dimension1);
    }

    /// <summary>
    /// Gets metrics above value threshold
    /// </summary>
    public static IQueryable<AnalyticsMetric> WithValueAbove(IQueryable<AnalyticsMetric> query, decimal threshold)
    {
        return query.Where(m => m.Value > threshold);
    }

    /// <summary>
    /// Gets metrics below value threshold
    /// </summary>
    public static IQueryable<AnalyticsMetric> WithValueBelow(IQueryable<AnalyticsMetric> query, decimal threshold)
    {
        return query.Where(m => m.Value < threshold);
    }

    /// <summary>
    /// Gets metrics with all three dimensions populated
    /// </summary>
    public static IQueryable<AnalyticsMetric> FullyDimensioned(IQueryable<AnalyticsMetric> query)
    {
        return query.Where(m => 
            !string.IsNullOrEmpty(m.Dimension1) && 
            !string.IsNullOrEmpty(m.Dimension2) && 
            !string.IsNullOrEmpty(m.Dimension3));
    }

    /// <summary>
    /// Orders metrics by timestamp (newest first)
    /// </summary>
    public static IOrderedQueryable<AnalyticsMetric> OrderByNewest(IQueryable<AnalyticsMetric> query)
    {
        return query.OrderByDescending(m => m.Timestamp);
    }

    /// <summary>
    /// Orders metrics by value (highest first)
    /// </summary>
    public static IOrderedQueryable<AnalyticsMetric> OrderByValueDesc(IQueryable<AnalyticsMetric> query)
    {
        return query.OrderByDescending(m => m.Value);
    }
}
