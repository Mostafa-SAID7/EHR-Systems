namespace EHRPlatform.Services.Analytics.Domain.Specifications;

using EHRPlatform.Services.Analytics.Domain.Entities;

/// <summary>
/// Specifications for Dashboard queries
/// </summary>
public static class DashboardSpecifications
{
    /// <summary>
    /// Gets all dashboards for tenant
    /// </summary>
    public static IQueryable<Dashboard> ForTenant(IQueryable<Dashboard> query, long tenantId)
    {
        return query.Where(d => d.TenantId == tenantId);
    }

    /// <summary>
    /// Gets public dashboards only
    /// </summary>
    public static IQueryable<Dashboard> PublicOnly(IQueryable<Dashboard> query)
    {
        return query.Where(d => d.IsPublic);
    }

    /// <summary>
    /// Gets dashboards by owner
    /// </summary>
    public static IQueryable<Dashboard> ByOwner(IQueryable<Dashboard> query, Guid userId)
    {
        return query.Where(d => d.CreatedBy == userId);
    }

    /// <summary>
    /// Gets dashboard by name (exact match)
    /// </summary>
    public static IQueryable<Dashboard> ByName(IQueryable<Dashboard> query, string name)
    {
        return query.Where(d => d.Name == name);
    }

    /// <summary>
    /// Gets dashboards matching name pattern
    /// </summary>
    public static IQueryable<Dashboard> ByNameContains(IQueryable<Dashboard> query, string namePattern)
    {
        return query.Where(d => d.Name.Contains(namePattern));
    }

    /// <summary>
    /// Orders dashboards by creation date (newest first)
    /// </summary>
    public static IOrderedQueryable<Dashboard> OrderByNewest(IQueryable<Dashboard> query)
    {
        return query.OrderByDescending(d => d.CreatedAt);
    }

    /// <summary>
    /// Orders dashboards by display order
    /// </summary>
    public static IOrderedQueryable<Dashboard> OrderByDisplayOrder(IQueryable<Dashboard> query)
    {
        return query.OrderBy(d => d.DisplayOrder);
    }

    /// <summary>
    /// Gets dashboards created within date range
    /// </summary>
    public static IQueryable<Dashboard> CreatedBetween(IQueryable<Dashboard> query, DateTime startDate, DateTime endDate)
    {
        return query.Where(d => d.CreatedAt >= startDate && d.CreatedAt <= endDate);
    }

    /// <summary>
    /// Gets dashboards with widgets
    /// </summary>
    public static IQueryable<Dashboard> WithWidgets(IQueryable<Dashboard> query)
    {
        return query.Where(d => d.Widgets.Any());
    }

    /// <summary>
    /// Gets dashboards updated recently (last N days)
    /// </summary>
    public static IQueryable<Dashboard> UpdatedRecently(IQueryable<Dashboard> query, int lastDays)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-lastDays);
        return query.Where(d => d.UpdatedAt.HasValue && d.UpdatedAt >= cutoffDate);
    }
}
