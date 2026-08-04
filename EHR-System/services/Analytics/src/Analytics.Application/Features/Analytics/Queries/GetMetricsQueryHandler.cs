namespace EHRPlatform.Services.Analytics.Application.Features.Analytics.Queries;

using MediatR;
using Microsoft.Extensions.Logging;
using EHRPlatform.Services.Analytics.Domain.Repositories;
using EHRPlatform.BuildingBlocks.Caching;
using EHRPlatform.BuildingBlocks.Security.MultiTenancy;

/// <summary>
/// Handler for getting metrics
/// </summary>
public class GetMetricsQueryHandler : IRequestHandler<GetMetricsQuery, GetMetricsResponse>
{
    private readonly IMetricRepository _metricRepository;
    private readonly ICacheService _cacheService;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetMetricsQueryHandler> _logger;

    public GetMetricsQueryHandler(
        IMetricRepository metricRepository,
        ICacheService cacheService,
        ITenantContext tenantContext,
        ILogger<GetMetricsQueryHandler> logger)
    {
        _metricRepository = metricRepository;
        _cacheService = cacheService;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<GetMetricsResponse> Handle(
        GetMetricsQuery query,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving metrics from {FromDate} to {ToDate}", query.FromDate, query.ToDate);

        try
        {
            var tenantId = _tenantContext.TenantId;
            if (tenantId == 0)
            {
                return new GetMetricsResponse(
                    Success: false,
                    Message: "Tenant context not available",
                    Metrics: Enumerable.Empty<MetricDataDto>(),
                    TotalCount: 0,
                    PageNumber: query.PageNumber,
                    PageSize: query.PageSize);
            }

            // Generate cache key
            var cacheKey = $"metrics:{tenantId}:{query.FromDate:yyyyMMdd}:{query.ToDate:yyyyMMdd}:{query.MetricType}:{query.PageNumber}";
            
            // Check cache (10 minutes)
            var cachedMetrics = await _cacheService.GetAsync<(List<MetricDataDto>, int)>(cacheKey);
            if (cachedMetrics.HasValue)
            {
                _logger.LogInformation("Retrieved metrics from cache");
                return new GetMetricsResponse(
                    Success: true,
                    Message: "Metrics retrieved successfully (from cache)",
                    Metrics: cachedMetrics.Value.Item1,
                    TotalCount: cachedMetrics.Value.Item2,
                    PageNumber: query.PageNumber,
                    PageSize: query.PageSize);
            }

            // Query metrics from repository by date range
            var metrics = await _metricRepository.GetByTimeRangeAsync(
                query.FromDate, query.ToDate, tenantId);

            // Filter by type if provided
            if (!string.IsNullOrWhiteSpace(query.MetricType))
            {
                metrics = metrics.Where(m => m.MetricName == query.MetricType);
            }

            var totalCount = metrics.Count();

            // Apply pagination
            var pagedMetrics = metrics
                .OrderByDescending(m => m.Timestamp)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(m => new MetricDataDto(
                    MetricName: m.MetricName,
                    Value: m.Value,
                    Timestamp: m.Timestamp,
                    Unit: m.Unit))
                .ToList();

            // Cache results for 10 minutes
            await _cacheService.SetAsync(cacheKey, (pagedMetrics, totalCount), TimeSpan.FromMinutes(10));

            _logger.LogInformation("Retrieved {Count} metrics from repository", pagedMetrics.Count);

            return new GetMetricsResponse(
                Success: true,
                Message: "Metrics retrieved successfully",
                Metrics: pagedMetrics,
                TotalCount: totalCount,
                PageNumber: query.PageNumber,
                PageSize: query.PageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving metrics");
            return new GetMetricsResponse(
                Success: false,
                Message: $"Failed to retrieve metrics: {ex.Message}",
                Metrics: Enumerable.Empty<MetricDataDto>(),
                TotalCount: 0,
                PageNumber: query.PageNumber,
                PageSize: query.PageSize);
        }
    }
}
