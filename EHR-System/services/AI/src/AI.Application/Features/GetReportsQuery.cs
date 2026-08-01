using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Contracts.DTOs;
using EHRPlatform.Services.Analytics.Application.Analytics.Responses;

namespace EHRPlatform.Services.Analytics.Features.Analytics.Queries;

/// <summary>
/// Get paginated list of reports query.
/// Cached for performance (3600s TTL - longer for reports).
/// </summary>
public record GetReportsQuery : ICachedQuery<PagedResult<ReportResponse>>
{
    public Guid? UserId { get; init; }
    public string? Schedule { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 50;

    public string CacheKey => $"reports_{UserId}_{Schedule}_{PageNumber}_{PageSize}";
    public int CacheDurationSeconds => 3600;
}


