namespace EHRPlatform.Services.Analytics.Domain.Specifications;

using EHRPlatform.Services.Analytics.Domain.Entities;

/// <summary>
/// Specifications for KPISummary queries
/// </summary>
public static class KPISpecifications
{
    /// <summary>
    /// Gets KPIs for tenant
    /// </summary>
    public static IQueryable<KPISummary> ForTenant(IQueryable<KPISummary> query, long tenantId)
    {
        return query.Where(k => k.TenantId == tenantId);
    }

    /// <summary>
    /// Gets KPI for specific date
    /// </summary>
    public static IQueryable<KPISummary> ForDate(IQueryable<KPISummary> query, DateTime date)
    {
        return query.Where(k => k.SummaryDate.Date == date.Date);
    }

    /// <summary>
    /// Gets KPIs within date range
    /// </summary>
    public static IQueryable<KPISummary> InDateRange(IQueryable<KPISummary> query, DateTime startDate, DateTime endDate)
    {
        return query.Where(k => k.SummaryDate >= startDate && k.SummaryDate <= endDate);
    }

    /// <summary>
    /// Gets recent KPIs (last N days)
    /// </summary>
    public static IQueryable<KPISummary> RecentDays(IQueryable<KPISummary> query, int lastDays)
    {
        var cutoffDate = DateTime.UtcNow.Date.AddDays(-lastDays);
        return query.Where(k => k.SummaryDate >= cutoffDate);
    }

    /// <summary>
    /// Gets KPIs where total patients exceeds threshold
    /// </summary>
    public static IQueryable<KPISummary> WithPatientsAbove(IQueryable<KPISummary> query, int threshold)
    {
        return query.Where(k => k.TotalPatients > threshold);
    }

    /// <summary>
    /// Gets KPIs where revenue exceeds threshold
    /// </summary>
    public static IQueryable<KPISummary> WithRevenueAbove(IQueryable<KPISummary> query, decimal threshold)
    {
        return query.Where(k => k.RevenueInvoiced > threshold);
    }

    /// <summary>
    /// Gets KPIs where system uptime is below threshold
    /// </summary>
    public static IQueryable<KPISummary> WithDowntime(IQueryable<KPISummary> query, double uptimeThreshold)
    {
        return query.Where(k => k.SystemUptime < uptimeThreshold);
    }

    /// <summary>
    /// Orders KPIs by date (newest first)
    /// </summary>
    public static IOrderedQueryable<KPISummary> OrderByNewest(IQueryable<KPISummary> query)
    {
        return query.OrderByDescending(k => k.SummaryDate);
    }

    /// <summary>
    /// Orders KPIs by patient count (highest first)
    /// </summary>
    public static IOrderedQueryable<KPISummary> OrderByPatientCount(IQueryable<KPISummary> query)
    {
        return query.OrderByDescending(k => k.TotalPatients);
    }

    /// <summary>
    /// Orders KPIs by revenue (highest first)
    /// </summary>
    public static IOrderedQueryable<KPISummary> OrderByRevenue(IQueryable<KPISummary> query)
    {
        return query.OrderByDescending(k => k.RevenueInvoiced);
    }
}
