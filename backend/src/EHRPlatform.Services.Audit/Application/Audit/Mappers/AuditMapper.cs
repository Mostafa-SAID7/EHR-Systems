using Mapster;
using EHRPlatform.Common.Mapping;
using EHRPlatform.Services.Audit.Domain.Entities;
using EHRPlatform.Services.Audit.Application.Audit.Responses;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Audit.Application.Audit.Mappers;

/// <summary>
/// Audit Mapper
/// Single Responsibility: Convert between Audit domain models and DTOs.
/// </summary>
public class AuditMapper : MappingServiceBase<AuditEntry, AuditEntryResponse>
{
    public AuditMapper(ILogger<AuditMapper> logger) : base(logger)
    {
    }

    /// <summary>
    /// Map single audit entry to response DTO.
    /// </summary>
    public AuditEntryResponse MapToResponseDto(AuditEntry auditEntry)
    {
        return MapSingleToDto(auditEntry);
    }

    /// <summary>
    /// Map collection of audit entries to paginated DTO.
    /// </summary>
    public AuditListDto MapToListDto(
        ICollection<AuditEntry> auditEntries,
        int total,
        int pageNumber,
        int pageSize)
    {
        Logger.LogDebug("Mapping {Count} audit entries to paginated list DTO", auditEntries.Count);

        return new AuditListDto
        {
            Items = auditEntries.Adapt<List<AuditEntryResponse>>(),
            Total = total,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Map collection of access logs to paginated DTO.
    /// </summary>
    public AccessLogListDto MapToAccessLogListDto(
        ICollection<AccessLog> accessLogs,
        int total,
        int pageNumber,
        int pageSize)
    {
        Logger.LogDebug("Mapping {Count} access logs to paginated list DTO", accessLogs.Count);

        return new AccessLogListDto
        {
            Items = accessLogs.Adapt<List<AccessLogResponse>>(),
            Total = total,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
