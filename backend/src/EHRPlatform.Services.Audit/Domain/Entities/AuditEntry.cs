using EHRPlatform.BuildingBlocks.SharedKernel.Entities;

namespace EHRPlatform.Services.Audit.Domain.Entities;

/// <summary>
/// Immutable audit entry (HIPAA-compliant).
/// Cannot be deleted or modified - compliance requirement.
/// </summary>
public class AuditEntry : BaseEntity
{
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // Create, Read, Update, Delete, Export, Print
    public string ResourceType { get; set; } = string.Empty; // Patient, Appointment, Clinical Note, etc.
    public Guid ResourceId { get; set; }
    public string Status { get; set; } = string.Empty; // Success, Failure
    public DateTime Timestamp { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string? PiiIndicators { get; set; } // Comma-separated: SSN, DOB, MRN, etc.
    public int AccessLevel { get; set; } // 1=Public, 2=Internal, 3=Confidential, 4=Restricted
    public string? ChangeDetails { get; set; } // JSON: {fieldName: {old, new}}
    public string? FailureReason { get; set; }
    public string IntegrityHash { get; set; } = string.Empty; // SHA-256 for tampering detection
    public int? SessionDurationSeconds { get; set; }
    public bool IsEncrypted { get; set; }

    /// <summary>
    /// Verify data integrity using hash.
    /// </summary>
    public bool VerifyIntegrity(string computedHash) => IntegrityHash == computedHash;
}


