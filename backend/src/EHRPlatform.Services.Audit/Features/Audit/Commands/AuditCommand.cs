using EHRPlatform.BuildingBlocks.Common.Application.CQRS;

namespace EHRPlatform.Services.Audit.Features.Audit.Commands;

/// <summary>
/// Record audit entry command.
/// Called by all services via Kafka or direct API.
/// </summary>
public record RecordAuditEntryCommand : ICommand
{
    public Guid UserId { get; init; }
    public string UserEmail { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string ResourceType { get; init; } = string.Empty;
    public Guid ResourceId { get; init; }
    public bool Success { get; init; } = true;
    public string? FailureReason { get; init; }
    public string IpAddress { get; init; } = string.Empty;
    public string UserAgent { get; init; } = string.Empty;
    public string? PiiIndicators { get; init; }
    public int AccessLevel { get; init; } = 1;
    public string? ChangeDetails { get; init; }
}

/// <summary>
/// Record data change command.
/// </summary>
public record RecordDataChangeCommand : ICommand
{
    public Guid UserId { get; init; }
    public string ResourceType { get; init; } = string.Empty;
    public Guid ResourceId { get; init; }
    public string FieldName { get; init; } = string.Empty;
    public string? OldValue { get; init; }
    public string? NewValue { get; init; }
    public string? Reason { get; init; }
}

/// <summary>
/// Generate compliance report command.
/// </summary>
public record GenerateComplianceReportCommand : ICommand<ComplianceReportResponseDto>
{
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
}

/// <summary>
/// Export audit logs command.
/// </summary>
public record ExportAuditLogsCommand : ICommand<AuditExportResponseDto>
{
    public Guid ExportedBy { get; init; }
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
    public string Format { get; init; } = "JSON"; // PDF, CSV, JSON
    public bool EncryptFile { get; init; }
}


