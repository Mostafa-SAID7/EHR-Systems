#nullable enable

using EHRPlatform.Common.Domain.Enums;

namespace EHRPlatform.Common.Domain.Entities;

/// <summary>
/// Entity with comprehensive audit tracking.
/// Critical for HIPAA compliance - tracks all changes with who, what, when, why.
/// Single responsibility: Define auditable entity contract only.
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

