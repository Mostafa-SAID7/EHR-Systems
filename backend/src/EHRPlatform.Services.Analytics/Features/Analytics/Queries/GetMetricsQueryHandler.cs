using EHRPlatform.Common.CQRS;
using EHRPlatform.Common.Data;
using EHRPlatform.Services.Analytics.Domain.Entities;
using EHRPlatform.Services.Analytics.Features.Analytics.Dtos.Responses;

namespace EHRPlatform.Services.Analytics.Features.Analytics.Queries;

/// <summary>
/// Get analytics metrics for a category and period handler.
/// Single Responsibility: Retrieve and aggregate metric entries for a specific category.
/// </summary>
public class GetMetricsQueryHandler : IQueryHandler<GetMetricsQuery, AnalyticsMetricResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetMetricsQueryHandler> _logger;

    public GetMetricsQueryHandler(IUnitOfWork unitOfWork, ILogger<GetMetricsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<AnalyticsMetricResponseDto> Handle(GetMetricsQuery request, CancellationToken ct)
    {
        _logger.LogInformation("Fetching metrics for {Category}", request.Category);
        var repo = _unitOfWork.Repository<AnalyticsMetric>();
        var metrics = await repo.ToListAsync(
            q => q.Where(m => m.Category == request.Category
                && m.PeriodStart >= request.PeriodStart
                && m.PeriodEnd <= request.PeriodEnd), ct);

        return new AnalyticsMetricResponseDto
        {
            Category = request.Category,
            PeriodStart = request.PeriodStart,
            PeriodEnd = request.PeriodEnd,
            Metrics = metrics.Select(m => new MetricItemDto { Name = m.MetricName, Value = m.Value, Unit = m.Unit }).ToList()
        };
    }
}
