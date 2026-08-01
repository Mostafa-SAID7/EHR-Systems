using EHRPlatform.BuildingBlocks.Common.Application.CQRS;

namespace EHRPlatform.Services.Audit.Features.Audit.Queries;

/// <summary>
/// Get audit trail for resource.
/// </summary>
public record GetResourceAuditTrailQuery : ICachedQuery<AuditTrailResponseDto>
{
    public string ResourceType { get; init; } = string.Empty;
    public Guid ResourceId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 50;

    public string CacheKey => $"audit_trail_{ResourceType}_{ResourceId}_{PageNumber}_{PageSize}";
    public int CacheDurationSeconds => 1800; // 30 minutes
}

/// <summary>
/// Get user audit activity.
/// </summary>
public record GetUserAuditActivityQuery : ICachedQuery<AccessLogDto>
{
    public Guid UserId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 50;

    public string CacheKey => $"audit_user_{UserId}_{FromDate:yyyyMMdd}_{ToDate:yyyyMMdd}_{PageNumber}_{PageSize}";
    public int CacheDurationSeconds => 1800;
}

/// <summary>
/// Get compliance reports.
/// </summary>
public record GetComplianceReportsQuery : ICachedQuery<List<ComplianceReportDto>>
{
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }

    public string CacheKey => $"compliance_reports_{FromDate:yyyyMMdd}_{ToDate:yyyyMMdd}";
    public int CacheDurationSeconds => 3600;
}


