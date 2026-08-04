using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EHRPlatform.Services.Analytics.Domain.Entities;
using EHRPlatform.Services.Analytics.Domain.Repositories;

namespace EHRPlatform.Services.Analytics.Persistence.Repositories;

/// <summary>
/// Repository implementation for AnalyticsMetric entity
/// </summary>
public class AnalyticsMetricRepository : IMetricRepository
{
    private readonly IAnalyticsDbContext _context;

    public AnalyticsMetricRepository(IAnalyticsDbContext context)
    {
        _context = context;
    }

    public async Task<AnalyticsMetric?> GetByIdAsync(Guid id)
    {
        return await _context.AnalyticsMetrics
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<IEnumerable<AnalyticsMetric>> GetByNameAsync(string metricName, long tenantId)
    {
        return await _context.AnalyticsMetrics
            .Where(m => m.MetricName == metricName)
            .ToListAsync();
    }

    public async Task<IEnumerable<AnalyticsMetric>> GetByCategoryAsync(string category, long tenantId)
    {
        return await _context.AnalyticsMetrics
            .Where(m => m.Category == category)
            .ToListAsync();
    }

    public async Task<IEnumerable<AnalyticsMetric>> GetByTimeRangeAsync(DateTime startTime, DateTime endTime, long tenantId)
    {
        return await _context.AnalyticsMetrics
            .Where(m => m.Timestamp >= startTime && m.Timestamp <= endTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<AnalyticsMetric>> GetAllAsync(long tenantId)
    {
        return await _context.AnalyticsMetrics
            .ToListAsync();
    }

    public async Task<IEnumerable<AnalyticsMetric>> GetRecentAsync(long tenantId, int lastHours = 24)
    {
        var cutoffTime = DateTime.UtcNow.AddHours(-lastHours);
        return await _context.AnalyticsMetrics
            .Where(m => m.Timestamp >= cutoffTime)
            .ToListAsync();
    }

    public async Task<AnalyticsMetric> AddAsync(AnalyticsMetric metric)
    {
        _context.AnalyticsMetrics.Add(metric);
        await _context.SaveChangesAsync();
        return metric;
    }

    public async Task AddBatchAsync(IEnumerable<AnalyticsMetric> metrics)
    {
        _context.AnalyticsMetrics.AddRange(metrics);
        await _context.SaveChangesAsync();
    }

    public async Task<AnalyticsMetric> UpdateAsync(AnalyticsMetric metric)
    {
        _context.AnalyticsMetrics.Update(metric);
        await _context.SaveChangesAsync();
        return metric;
    }

    public async Task DeleteAsync(Guid id)
    {
        var metric = await GetByIdAsync(id);
        if (metric != null)
        {
            _context.AnalyticsMetrics.Remove(metric);
            await _context.SaveChangesAsync();
        }
    }

    public async Task ArchiveOldMetricsAsync(int olderThanDays, long tenantId)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-olderThanDays);
        var oldMetrics = await _context.AnalyticsMetrics
            .Where(m => m.Timestamp < cutoffDate)
            .ToListAsync();

        foreach (var metric in oldMetrics)
        {
            _context.AnalyticsMetrics.Remove(metric);
        }
        
        await _context.SaveChangesAsync();
    }

    public async Task<long> GetCountAsync(string metricName, DateTime startTime, DateTime endTime, long tenantId)
    {
        return await _context.AnalyticsMetrics
            .Where(m => m.MetricName == metricName && m.Timestamp >= startTime && m.Timestamp <= endTime)
            .CountAsync();
    }
}
