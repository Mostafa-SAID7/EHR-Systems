using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Contracts.DTOs;
using EHRPlatform.Services.Audit.Application.Audit.Responses;

namespace EHRPlatform.Services.Audit.Features.Audit.Queries;

/// <summary>
/// Get paginated list of access logs query.
/// Cached for performance (600s TTL).
/// </summary>
public record GetAccessLogsQuery : ICachedQuery<PagedResult<AccessLogResponse>>
{
    public Guid? UserId { get; init; }
    public string? ResourceType { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 50;

    public string CacheKey => $"access_logs_{UserId}_{ResourceType}_{StartDate:yyyyMMdd}_{EndDate:yyyyMMdd}_{PageNumber}_{PageSize}";
    public int CacheDurationSeconds => 600;
}


