namespace EHRPlatform.Services.Audit.Features.Audit.Dtos.Responses;

// ── Audit Trail ───────────────────────────────────────────────────────────────

/// <summary>Paginated audit trail for a specific resource.</summary>
public class AuditTrailResponseDto
{
    public string ResourceType { get; set; } = string.Empty;
    public Guid ResourceId { get; set; }
    public List<AuditEntryResponseDto> Entries { get; set; } = new();
    public int Total { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (Total + PageSize - 1) / PageSize;
}

/// <summary>Single audit entry in an audit trail response.</summary>
public class AuditEntryResponseDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? PiiIndicators { get; set; }
    public int AccessLevel { get; set; }
    public string? ChangeDetails { get; set; }
    public string? FailureReason { get; set; }
}

// ── Access Log ────────────────────────────────────────────────────────────────

/// <summary>Aggregated user audit activity.</summary>
public class AccessLogDto
{
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public List<ActivitySummaryDto> Activities { get; set; } = new();
    public int TotalActions { get; set; }
    public int FailedActions { get; set; }
}

/// <summary>Activity summary grouped by action type.</summary>
public class ActivitySummaryDto
{
    public string Action { get; set; } = string.Empty;
    public int Count { get; set; }
    public DateTime LastOccurred { get; set; }
}

// ── Compliance ────────────────────────────────────────────────────────────────

/// <summary>Single compliance report DTO.</summary>
public class ComplianceReportDto
{
    public Guid Id { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public int TotalActions { get; set; }
    public int FailedActions { get; set; }
    public int DataAccess { get; set; }
    public int DataChanges { get; set; }
    public int UnauthorizedAttempts { get; set; }
    public int PiiAccessed { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
}

/// <summary>Response after generating a new compliance report.</summary>
public class ComplianceReportResponseDto
{
    public Guid Id { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public int TotalActions { get; set; }
    public int FailedActions { get; set; }
    public int DataAccess { get; set; }
    public int DataChanges { get; set; }
    public int UnauthorizedAttempts { get; set; }
    public int PiiAccessed { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
}

// ── Export ────────────────────────────────────────────────────────────────────

/// <summary>Response after exporting audit logs.</summary>
public class AuditExportResponseDto
{
    public Guid Id { get; set; }
    public DateTime ExportedAt { get; set; }
    public Guid ExportedBy { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public int RecordCount { get; set; }
    public string Format { get; set; } = string.Empty;
    public bool IsEncrypted { get; set; }
    public string Status { get; set; } = string.Empty;
    public string FileHash { get; set; } = string.Empty;
}
