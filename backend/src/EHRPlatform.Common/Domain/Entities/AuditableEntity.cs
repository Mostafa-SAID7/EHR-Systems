#nullable enable

namespace EHRPlatform.Common.Domain.Entities;

/// <summary>
/// Entity with comprehensive audit tracking.
/// Critical for HIPAA compliance - tracks all changes with who, what, when, why.
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    /// <summary>
    /// Indicates if this entity contains PII (Personally Identifiable Information).
    /// Used for audit trail filtering and HIPAA compliance.
    /// </summary>
    public bool ContainsPII { get; set; } = true;

    /// <summary>
    /// Access level required to view this entity's audit trail.
    /// </summary>
    public AuditAccessLevel AccessLevel { get; set; } = AuditAccessLevel.Standard;

    /// <summary>
    /// Reason for the last change (for compliance tracking).
    /// </summary>
    public string? ChangeReason { get; set; }

    /// <summary>
    /// IP address from which the last change was made.
    /// </summary>
    public string? SourceIPAddress { get; set; }

    /// <summary>
    /// Data encryption status.
    /// </summary>
    public bool IsEncrypted { get; set; } = true;

    /// <summary>
    /// Version number for optimistic concurrency control.
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// Tenant ID for multi-tenant systems.
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Archive date for records moved to cold storage.
    /// </summary>
    public DateTime? ArchivedAt { get; set; }
}

/// <summary>
/// Access levels for audit trail visibility.
/// </summary>
public enum AuditAccessLevel
{
    /// <summary>
    /// Only the user who made the change can access the audit trail.
    /// </summary>
    Personal = 0,

    /// <summary>
    /// Department/team level access.
    /// </summary>
    Department = 1,

    /// <summary>
    /// Standard organizational access.
    /// </summary>
    Standard = 2,

    /// <summary>
    /// Senior management access.
    /// </summary>
    Management = 3,

    /// <summary>
    /// Compliance officer / Auditor access.
    /// </summary>
    Auditor = 4,

    /// <summary>
    /// System administrator access.
    /// </summary>
    Administrator = 5
}

