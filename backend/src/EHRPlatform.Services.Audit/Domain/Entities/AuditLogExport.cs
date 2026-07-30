using EHRPlatform.Common.Domain.Entities;

namespace EHRPlatform.Services.Audit.Domain.Entities;

/// <summary>
/// Audit log export (immutable snapshot for compliance).
/// </summary>
public class AuditLogExport : BaseEntity
{
    public DateTime ExportedAt { get; set; }
    public Guid ExportedBy { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public int RecordCount { get; set; }
    public string FileHash { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public string Format { get; set; } = string.Empty; // PDF, CSV, JSON
    public string Status { get; set; } = string.Empty; // Pending, Completed, Failed
    public bool IsEncrypted { get; set; }
}

