using EHRPlatform.Common.CQRS;
using EHRPlatform.Common.DTOs;
using EHRPlatform.Services.Analytics.Features.Analytics.Queries;
using EHRPlatform.Services.Analytics.Application.Analytics.Responses;
using EHRPlatform.Services.Analytics.Application.Analytics.Mappers;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Analytics.Features.Analytics.Handlers;

/// <summary>
/// Handler for GetDashboardsQuery.
/// Retrieves paginated dashboards filtered by user.
/// </summary>
public class GetDashboardsQueryHandler : IQueryHandler<GetDashboardsQuery, PagedResult<DashboardResponse>>
{
    private readonly AnalyticsMapper _mapper;
    private readonly ILogger<GetDashboardsQueryHandler> _logger;

    public GetDashboardsQueryHandler(
        AnalyticsMapper mapper,
        ILogger<GetDashboardsQueryHandler> logger)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger;
    }

    public async Task<PagedResult<DashboardResponse>> Handle(GetDashboardsQuery query, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Retrieving dashboards for user {UserId}, page {PageNumber}", query.UserId, query.PageNumber);

        // TODO: Implement repository query
        var dashboards = new List<Domain.Entities.Dashboard>();
        var total = 0;

        return _mapper.MapToDashboardPagedResult(dashboards, total, query.PageNumber, query.PageSize);
    }
}
