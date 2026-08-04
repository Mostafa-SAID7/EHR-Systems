namespace EHRPlatform.Services.Analytics.Domain.Specifications;

using EHRPlatform.Services.Analytics.Domain.Entities;
using EHRPlatform.Services.Analytics.Domain.Enums;

/// <summary>
/// Specifications for Report queries
/// </summary>
public static class ReportSpecifications
{
    /// <summary>
    /// Gets reports for tenant
    /// </summary>
    public static IQueryable<Report> ForTenant(IQueryable<Report> query, long tenantId)
    {
        return query.Where(r => r.TenantId == tenantId);
    }

    /// <summary>
    /// Gets active reports only
    /// </summary>
    public static IQueryable<Report> ActiveOnly(IQueryable<Report> query)
    {
        return query.Where(r => r.Status == ReportStatus.Active);
    }

    /// <summary>
    /// Gets scheduled reports only
    /// </summary>
    public static IQueryable<Report> ScheduledOnly(IQueryable<Report> query)
    {
        return query.Where(r => r.IsScheduled);
    }

    /// <summary>
    /// Gets on-demand reports only
    /// </summary>
    public static IQueryable<Report> OnDemandOnly(IQueryable<Report> query)
    {
        return query.Where(r => !r.IsScheduled);
    }

    /// <summary>
    /// Gets reports by owner
    /// </summary>
    public static IQueryable<Report> ByOwner(IQueryable<Report> query, Guid userId)
    {
        return query.Where(r => r.CreatedBy == userId);
    }

    /// <summary>
    /// Gets reports by name
    /// </summary>
    public static IQueryable<Report> ByName(IQueryable<Report> query, string name)
    {
        return query.Where(r => r.Name == name);
    }

    /// <summary>
    /// Gets reports matching name pattern
    /// </summary>
    public static IQueryable<Report> ByNameContains(IQueryable<Report> query, string namePattern)
    {
        return query.Where(r => r.Name.Contains(namePattern));
    }

    /// <summary>
    /// Gets reports by type
    /// </summary>
    public static IQueryable<Report> ByType(IQueryable<Report> query, string reportType)
    {
        return query.Where(r => r.ReportType == reportType);
    }

    /// <summary>
    /// Gets reports with execution history
    /// </summary>
    public static IQueryable<Report> WithExecutions(IQueryable<Report> query)
    {
        return query.Where(r => r.Executions.Any());
    }

    /// <summary>
    /// Gets reports created within date range
    /// </summary>
    public static IQueryable<Report> CreatedBetween(IQueryable<Report> query, DateTime startDate, DateTime endDate)
    {
        return query.Where(r => r.CreatedAt >= startDate && r.CreatedAt <= endDate);
    }

    /// <summary>
    /// Orders reports by creation date (newest first)
    /// </summary>
    public static IOrderedQueryable<Report> OrderByNewest(IQueryable<Report> query)
    {
        return query.OrderByDescending(r => r.CreatedAt);
    }

    /// <summary>
    /// Orders reports by last execution
    /// </summary>
    public static IOrderedQueryable<Report> OrderByLastExecution(IQueryable<Report> query)
    {
        return query.OrderByDescending(r => r.Executions.OrderByDescending(e => e.ExecutedAt).FirstOrDefault()!.ExecutedAt);
    }
}
