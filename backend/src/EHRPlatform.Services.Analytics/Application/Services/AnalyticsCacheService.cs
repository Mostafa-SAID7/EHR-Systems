using EHRPlatform.BuildingBlocks.Observability.Caching;

namespace EHRPlatform.Services.Analytics.Application.Services;

/// <summary>
/// Analytics-specific cache key generator.
/// Uses CacheKeyGenerator pattern from Common for consistency.
/// </summary>
public static class AnalyticsCacheKeys
{
    // Dashboard cache keys
    public static string DashboardKey(Guid id) => $"analytics:dashboard:{id}";
    public static string DashboardsByUserKey(Guid userId) => $"analytics:dashboards:user:{userId}";
    public static string DashboardPatternKey => "analytics:dashboard:*";

    // Report cache keys
    public static string ReportKey(Guid id) => $"analytics:report:{id}";
    public static string ReportsByUserKey(Guid userId) => $"analytics:reports:user:{userId}";
    public static string ReportPatternKey => "analytics:report:*";

    // Metric cache keys
    public static string MetricKey(Guid id) => $"analytics:metric:{id}";
    public static string MetricsListKey => "analytics:metrics:list";
    public static string MetricPatternKey => "analytics:metric:*";

    // Aggregated analytics cache keys
    public static string AggregationKey(string aggregationType, Guid entityId) =>
        $"analytics:agg:{aggregationType}:{entityId}";
    public static string AggregationPatternKey => "analytics:agg:*";
}

/// <summary>
/// Analytics service wrapper around ICacheService.
/// Provides type-safe, domain-specific caching operations.
/// Reuses the production-grade ICacheService from Common.
/// </summary>
public interface IAnalyticsCacheService
{
    /// <summary>
    /// Get or load dashboard (prevents thundering herd).
    /// </summary>
    Task<T?> GetDashboardAsync<T>(Guid dashboardId, Func<Task<T>>? loader = null)
        where T : class;

    /// <summary>
    /// Get or load report.
    /// </summary>
    Task<T?> GetReportAsync<T>(Guid reportId, Func<Task<T>>? loader = null)
        where T : class;

    /// <summary>
    /// Get or load metric.
    /// </summary>
    Task<T?> GetMetricAsync<T>(Guid metricId, Func<Task<T>>? loader = null)
        where T : class;

    /// <summary>
    /// Invalidate dashboard cache (user-initiated or system).
    /// </summary>
    Task InvalidateDashboardAsync(Guid dashboardId);

    /// <summary>
    /// Invalidate all user's dashboards.
    /// </summary>
    Task InvalidateUserDashboardsAsync(Guid userId);

    /// <summary>
    /// Invalidate report cache.
    /// </summary>
    Task InvalidateReportAsync(Guid reportId);

    /// <summary>
    /// Invalidate all user's reports.
    /// </summary>
    Task InvalidateUserReportsAsync(Guid userId);

    /// <summary>
    /// Invalidate all analytics cache (rare, use sparingly).
    /// </summary>
    Task InvalidateAllAsync();
}

public class AnalyticsCacheService : IAnalyticsCacheService
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<AnalyticsCacheService> _logger;
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromHours(1);
    private readonly TimeSpan _shortExpiration = TimeSpan.FromMinutes(15);

    public AnalyticsCacheService(ICacheService cacheService, ILogger<AnalyticsCacheService> logger)
    {
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger;
    }

    public async Task<T?> GetDashboardAsync<T>(Guid dashboardId, Func<Task<T>>? loader = null)
        where T : class
    {
        var cacheKey = AnalyticsCacheKeys.DashboardKey(dashboardId);

        if (loader == null)
        {
            // Simple get
            return await _cacheService.GetAsync<T>(cacheKey);
        }

        // Get or set (prevents thundering herd)
        return await _cacheService.GetOrSetAsync(
            cacheKey,
            async _ => await loader(),
            _defaultExpiration);
    }

    public async Task<T?> GetReportAsync<T>(Guid reportId, Func<Task<T>>? loader = null)
        where T : class
    {
        var cacheKey = AnalyticsCacheKeys.ReportKey(reportId);

        if (loader == null)
        {
            return await _cacheService.GetAsync<T>(cacheKey);
        }

        return await _cacheService.GetOrSetAsync(
            cacheKey,
            async _ => await loader(),
            _shortExpiration); // Reports cached shorter than dashboards
    }

    public async Task<T?> GetMetricAsync<T>(Guid metricId, Func<Task<T>>? loader = null)
        where T : class
    {
        var cacheKey = AnalyticsCacheKeys.MetricKey(metricId);

        if (loader == null)
        {
            return await _cacheService.GetAsync<T>(cacheKey);
        }

        return await _cacheService.GetOrSetAsync(
            cacheKey,
            async _ => await loader(),
            _shortExpiration); // Metrics often change, shorter TTL
    }

    public async Task InvalidateDashboardAsync(Guid dashboardId)
    {
        try
        {
            await _cacheService.RemoveAsync(AnalyticsCacheKeys.DashboardKey(dashboardId));
            _logger.LogInformation("Invalidated dashboard cache: {DashboardId}", dashboardId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to invalidate dashboard cache: {DashboardId}", dashboardId);
        }
    }

    public async Task InvalidateUserDashboardsAsync(Guid userId)
    {
        try
        {
            await _cacheService.RemoveByPatternAsync(
                AnalyticsCacheKeys.DashboardsByUserKey(userId));
            _logger.LogInformation("Invalidated user dashboards cache: {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to invalidate user dashboards: {UserId}", userId);
        }
    }

    public async Task InvalidateReportAsync(Guid reportId)
    {
        try
        {
            await _cacheService.RemoveAsync(AnalyticsCacheKeys.ReportKey(reportId));
            _logger.LogInformation("Invalidated report cache: {ReportId}", reportId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to invalidate report cache: {ReportId}", reportId);
        }
    }

    public async Task InvalidateUserReportsAsync(Guid userId)
    {
        try
        {
            await _cacheService.RemoveByPatternAsync(
                AnalyticsCacheKeys.ReportsByUserKey(userId));
            _logger.LogInformation("Invalidated user reports cache: {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to invalidate user reports: {UserId}", userId);
        }
    }

    public async Task InvalidateAllAsync()
    {
        try
        {
            await _cacheService.RemoveByPatternAsync(AnalyticsCacheKeys.DashboardPatternKey);
            await _cacheService.RemoveByPatternAsync(AnalyticsCacheKeys.ReportPatternKey);
            await _cacheService.RemoveByPatternAsync(AnalyticsCacheKeys.MetricPatternKey);
            _logger.LogWarning("Invalidated ALL analytics cache");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to invalidate all analytics cache");
        }
    }
}


