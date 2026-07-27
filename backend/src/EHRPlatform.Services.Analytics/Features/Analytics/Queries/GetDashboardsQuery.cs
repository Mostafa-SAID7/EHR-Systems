using EHRPlatform.Common.CQRS;
using EHRPlatform.Common.DTOs;
using EHRPlatform.Services.Analytics.Application.Analytics.Responses;

namespace EHRPlatform.Services.Analytics.Features.Analytics.Queries;

/// <summary>
/// Get paginated list of dashboards query.
/// Cached for performance (600s TTL).
/// </summary>
public record GetDashboardsQuery : ICachedQuery<PagedResult<DashboardResponse>>
{
    public Guid? UserId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 50;

    public string CacheKey => $"dashboards_{UserId}_{PageNumber}_{PageSize}";
    public int CacheDurationSeconds => 600;
}
