namespace EHRPlatform.Services.Analytics.Application.Features.Analytics.Queries;

using MediatR;
using EHRPlatform.Services.Analytics.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handler for GetKPISummaryQuery - Retrieves KPI summary.
/// </summary>
public class GetKPISummaryQueryHandler : IRequestHandler<GetKPISummaryQuery, GetKPISummaryResponse>
{
    private readonly IAnalyticsDbContext _context;
    private readonly ILogger<GetKPISummaryQueryHandler> _logger;

    public GetKPISummaryQueryHandler(
        IAnalyticsDbContext context,
        ILogger<GetKPISummaryQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<GetKPISummaryResponse> Handle(GetKPISummaryQuery request, CancellationToken cancellationToken)
    {
        var queryDate = request.ForDate?.Date ?? DateTime.UtcNow.Date;
        _logger.LogInformation("Getting KPI summary for {Date}", queryDate);

        try
        {
            var summary = await _context.KPISummaries
                .FirstOrDefaultAsync(k => k.SummaryDate.Date == queryDate, cancellationToken);

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
