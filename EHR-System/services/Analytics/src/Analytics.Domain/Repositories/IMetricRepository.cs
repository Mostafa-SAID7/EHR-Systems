using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EHRPlatform.Services.Analytics.Domain.Entities;

namespace EHRPlatform.Services.Analytics.Domain.Repositories;

/// <summary>
/// Repository interface for Metric entity
/// </summary>
public interface IMetricRepository
{
    /// <summary>
    /// Get metric by ID
    /// </summary>
    Task<Metric?> GetByIdAsync(Guid id);

    /// <summary>
    /// Get metrics by name
    /// </summary>
    Task<IEnumerable<Metric>> GetByNameAsync(string metricName, long tenantId);

    /// <summary>
    /// Get metrics by category
    /// </summary>
    Task<IEnumerable<Metric>> GetByCategoryAsync(string category, long tenantId);

    /// <summary>
    /// Get metrics within time range
    /// </summary>
    Task<IEnumerable<Metric>> GetByTimeRangeAsync(DateTime startTime, DateTime endTime, long tenantId);

    /// <summary>
    /// Get all metrics for tenant
    /// </summary>
    Task<IEnumerable<Metric>> GetAllAsync(long tenantId);

    /// <summary>
    /// Get recent metrics (last N hours)
    /// </summary>
    Task<IEnumerable<Metric>> GetRecentAsync(long tenantId, int lastHours = 24);

    /// <summary>
    /// Add new metric
    /// </summary>
    Task<Metric> AddAsync(Metric metric);

    /// <summary>
    /// Add multiple metrics (batch)
    /// </summary>
    Task AddBatchAsync(IEnumerable<Metric> metrics);

    /// <summary>
    /// Update existing metric
    /// </summary>
    Task<Metric> UpdateAsync(Metric metric);

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
