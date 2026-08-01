using Mapster;
using EHRPlatform.BuildingBlocks.Common.Application.Mapping;
using EHRPlatform.BuildingBlocks.Contracts.DTOs;
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
    /// Map collection of audit entries to paginated result.
    /// </summary>
    public PagedResult<AuditEntryResponse> MapToPagedResult(
        ICollection<AuditEntry> auditEntries,
        int total,
        int pageNumber,
        int pageSize)
    {
        Logger.LogDebug("Mapping {Count} audit entries to paged result", auditEntries.Count);

        return PagedResult<AuditEntryResponse>.Create(
            auditEntries.Adapt<List<AuditEntryResponse>>(),
            total,
            pageNumber,
            pageSize);
    }

    /// <summary>
    /// Map collection of access logs to paginated result.
    /// </summary>
    public PagedResult<AccessLogResponse> MapToAccessLogPagedResult(
        ICollection<AccessLog> accessLogs,
        int total,
        int pageNumber,
        int pageSize)
    {
        Logger.LogDebug("Mapping {Count} access logs to paged result", accessLogs.Count);

        return PagedResult<AccessLogResponse>.Create(
            accessLogs.Adapt<List<AccessLogResponse>>(),
            total,
            pageNumber,
            pageSize);
    }
}


