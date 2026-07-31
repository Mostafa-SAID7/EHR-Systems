using EHRPlatform.Common.Application.Common.CQRS;
using EHRPlatform.Common.Shared.DTOs;
using EHRPlatform.Services.Analytics.Features.Analytics.Queries;
using EHRPlatform.Services.Analytics.Application.Analytics.Responses;
using EHRPlatform.Services.Analytics.Application.Analytics.Mappers;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Analytics.Features.Analytics.Handlers;

/// <summary>
/// Handler for GetReportsQuery.
/// Retrieves paginated reports filtered by user and schedule.
/// </summary>
public class GetReportsQueryHandler : IQueryHandler<GetReportsQuery, PagedResult<ReportResponse>>
{
    private readonly AnalyticsMapper _mapper;
    private readonly ILogger<GetReportsQueryHandler> _logger;

    public GetReportsQueryHandler(
        AnalyticsMapper mapper,
        ILogger<GetReportsQueryHandler> logger)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger;
    }

    public async Task<PagedResult<ReportResponse>> Handle(GetReportsQuery query, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Retrieving reports for user {UserId}, schedule {Schedule}, page {PageNumber}", 
            query.UserId, query.Schedule, query.PageNumber);

        // TODO: Implement repository query
        var reports = new List<Domain.Entities.Report>();
        var total = 0;

        return _mapper.MapToReportPagedResult(reports, total, query.PageNumber, query.PageSize);
    }
}

