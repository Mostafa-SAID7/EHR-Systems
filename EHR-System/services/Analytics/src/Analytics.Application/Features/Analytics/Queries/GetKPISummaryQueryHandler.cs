namespace EHRPlatform.Services.Analytics.Application.Features.Analytics.Queries;

using MediatR;
using EHRPlatform.Services.Analytics.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using EHRPlatform.BuildingBlocks.Caching;
using EHRPlatform.BuildingBlocks.Security.MultiTenancy;

/// <summary>
/// Handler for GetKPISummaryQuery - Retrieves KPI summary with 15-minute cache.
/// </summary>
public class GetKPISummaryQueryHandler : IRequestHandler<GetKPISummaryQuery, GetKPISummaryResponse>
{
    private readonly IAnalyticsDbContext _context;
    private readonly ICacheService _cacheService;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetKPISummaryQueryHandler> _logger;

    public GetKPISummaryQueryHandler(
        IAnalyticsDbContext context,
        ICacheService cacheService,
        ITenantContext tenantContext,
        ILogger<GetKPISummaryQueryHandler> logger)
    {
        _context = context;
        _cacheService = cacheService;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<GetKPISummaryResponse> Handle(GetKPISummaryQuery request, CancellationToken cancellationToken)
    {
        var queryDate = request.ForDate?.Date ?? DateTime.UtcNow.Date;
        _logger.LogInformation("Getting KPI summary for {Date}", queryDate);

        try
        {
            var tenantId = _tenantContext.TenantId;
            if (tenantId == 0)
            {
                return new GetKPISummaryResponse
                {
                    Success = false,
                    Message = "Tenant context not available"
                };
            }

            // Generate cache key
            var cacheKey = $"kpi:summary:{tenantId}:{queryDate:yyyyMMdd}";

            // Check cache (15 minutes)
            var cachedSummary = await _cacheService.GetAsync<KPISummaryDto>(cacheKey);
            if (cachedSummary != null)
            {
                _logger.LogInformation("Retrieved KPI summary from cache");
                return new GetKPISummaryResponse
                {
                    Success = true,
                    Summary = cachedSummary
                };
            }

            // Get from database
            var summary = await _context.KPISummaries
                .Where(k => k.SummaryDate.Date == queryDate && k.TenantId == tenantId)
                .FirstOrDefaultAsync(cancellationToken);

            if (summary == null)
            {
                _logger.LogInformation("No KPI summary found for {Date}", queryDate);
                return new GetKPISummaryResponse
                {
                    Success = false,
                    Message = "No data available for this date"
                };
            }

            var dto = new KPISummaryDto
            {
                SummaryDate = summary.SummaryDate,
                TotalPatients = summary.TotalPatients,
                NewPatients = summary.NewPatients,
                AppointmentsScheduled = summary.AppointmentsScheduled,
                AppointmentsCompleted = summary.AppointmentsCompleted,
                AppointmentsCancelled = summary.AppointmentsCancelled,
                AverageAppointmentDurationMinutes = summary.AverageAppointmentDurationMinutes,
                ClinicalNotesCreated = summary.ClinicalNotesCreated,
                RevenueInvoiced = summary.RevenueInvoiced,
                RevenuePaid = summary.RevenuePaid,
                OutstandingBalance = summary.OutstandingBalance,
                SystemUptime = summary.SystemUptime,
                ApiCallCount = summary.ApiCallCount,
                AverageResponseTimeMs = summary.AverageResponseTimeMs
            };

            // Cache for 15 minutes
            await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(15));

            return new GetKPISummaryResponse
            {
                Success = true,
                Summary = dto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting KPI summary");
            return new GetKPISummaryResponse
            {
                Success = false,
                Message = "An error occurred while retrieving the KPI summary"
            };
        }
    }
}
