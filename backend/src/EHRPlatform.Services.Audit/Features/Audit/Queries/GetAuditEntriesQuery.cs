using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Contracts.DTOs;
using EHRPlatform.Services.Audit.Application.Audit.Responses;

namespace EHRPlatform.Services.Audit.Features.Audit.Queries;

/// <summary>
/// Get paginated list of audit entries query.
/// Cached for performance (600s TTL).
/// </summary>
public record GetAuditEntriesQuery : ICachedQuery<PagedResult<AuditEntryResponse>>
{
    public Guid? UserId { get; init; }
    public string? ResourceType { get; init; }
    public string? Action { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 50;

    public string CacheKey => $"audit_entries_{UserId}_{ResourceType}_{Action}_{StartDate:yyyyMMdd}_{EndDate:yyyyMMdd}_{PageNumber}_{PageSize}";
    public int CacheDurationSeconds => 600;
}


