using EHRPlatform.Common.Application.CQRS;
using EHRPlatform.Common.Data.Abstractions;
using EHRPlatform.Common.Data.Implementations;
using EHRPlatform.Services.Analytics.Domain.Entities;
using EHRPlatform.Services.Analytics.Features.Analytics.Dtos.Responses;

namespace EHRPlatform.Services.Analytics.Features.Analytics.Queries;

/// <summary>
/// Get KPI summary handler.
/// Single Responsibility: Compute KPI summary (patient volume, utilization, revenue) for a period.
/// </summary>
public class GetKPISummaryQueryHandler : IQueryHandler<GetKPISummaryQuery, AnalyticsMetricListDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetKPISummaryQueryHandler> _logger;

    public GetKPISummaryQueryHandler(IUnitOfWork unitOfWork, ILogger<GetKPISummaryQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<AnalyticsMetricListDto> Handle(GetKPISummaryQuery request, CancellationToken ct)
    {
        var periodStart = request.PeriodStart ?? DateTime.UtcNow.AddDays(-30);
        var periodEnd   = request.PeriodEnd   ?? DateTime.UtcNow;

        var repo = _unitOfWork.Repository<AnalyticsMetric>();
        var metrics = await repo.ToListAsync(
            q => q.Where(m => m.PeriodStart >= periodStart && m.PeriodEnd <= periodEnd), ct);

        return new AnalyticsMetricListDto
        {
            PatientVolume          = metrics.Where(m => m.MetricName.Contains("patient")).Sum(m => m.Value),
            AppointmentUtilization = metrics.Where(m => m.MetricName.Contains("appointment")).Sum(m => m.Value) / 100m,
            RevenueTotal           = metrics.Where(m => m.Unit == "USD").Sum(m => m.Value),
            Trends                 = new List<TrendItemDto>()
        };
    }
}


