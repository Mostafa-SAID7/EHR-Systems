using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EHRPlatform.Services.Analytics.Domain.Entities;

namespace EHRPlatform.Services.Analytics.Domain.Repositories;

/// <summary>
/// Repository interface for AnalyticsMetric entity
/// </summary>
public interface IMetricRepository
{
    /// <summary>
    /// Get metric by ID
    /// </summary>
    Task<AnalyticsMetric?> GetByIdAsync(Guid id);

    /// <summary>
    /// Get metrics by name
    /// </summary>
    Task<IEnumerable<AnalyticsMetric>> GetByNameAsync(string metricName, long tenantId);

    /// <summary>
    /// Get metrics by category
    /// </summary>
    Task<IEnumerable<AnalyticsMetric>> GetByCategoryAsync(string category, long tenantId);

    /// <summary>
    /// Get metrics within time range
    /// </summary>
    Task<IEnumerable<AnalyticsMetric>> GetByTimeRangeAsync(DateTime startTime, DateTime endTime, long tenantId);

    /// <summary>
    /// Get all metrics for tenant
    /// </summary>
    Task<IEnumerable<AnalyticsMetric>> GetAllAsync(long tenantId);

    /// <summary>
    /// Get recent metrics (last N hours)
    /// </summary>
    Task<IEnumerable<AnalyticsMetric>> GetRecentAsync(long tenantId, int lastHours = 24);

    /// <summary>
    /// Add new metric
    /// </summary>
    Task<AnalyticsMetric> AddAsync(AnalyticsMetric metric);

    /// <summary>
    /// Add multiple metrics (batch)
    /// </summary>
    Task AddBatchAsync(IEnumerable<AnalyticsMetric> metrics);

    /// <summary>
    /// Update existing metric
    /// </summary>
    Task<AnalyticsMetric> UpdateAsync(AnalyticsMetric metric);

    /// <summary>
    /// Delete metric
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Archive metrics older than specified days
    /// </summary>
    Task ArchiveOldMetricsAsync(int olderThanDays, long tenantId);

    /// <summary>
    /// Get metric count for time period
    /// </summary>
    Task<long> GetCountAsync(string metricName, DateTime startTime, DateTime endTime, long tenantId);
}
