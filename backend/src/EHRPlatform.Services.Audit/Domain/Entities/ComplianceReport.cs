using EHRPlatform.Common.Domain.Entities;

namespace EHRPlatform.Services.Audit.Domain.Entities;

/// <summary>
/// Compliance report (periodic audit summary).
/// </summary>
public class ComplianceReport : BaseEntity
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public int TotalActions { get; set; }
    public int FailedActions { get; set; }
    public int DataAccess { get; set; }
    public int DataChanges { get; set; }
    public int UnauthorizedAttempts { get; set; }
    public List<string> PiiAccessed { get; set; } = new(); // PII types accessed in period
    public string Status { get; set; } = "Generated"; // Generated, Reviewed, Signed, Archived
    public string? SignedBy { get; set; }
    public DateTime? SignedAt { get; set; }
    public string? DigitalSignature { get; set; }
}

