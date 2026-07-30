#nullable enable

using EHRPlatform.Common.Domain.Enums;

namespace EHRPlatform.Common.Domain.Entities;

/// <summary>
/// Immutable audit log record for HIPAA compliance.
/// Every action that affects data is logged here.
/// </summary>
public class AuditLog
{
    /// <summary>
    /// Unique ID for this audit record.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Tenant ID for multi-tenant systems.
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// User ID who performed the action.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// User email (denormalized for reporting).
    /// </summary>
    public string? UserEmail { get; set; }

    /// <summary>
    /// User role at time of action.
    /// </summary>
    public string? UserRole { get; set; }

    /// <summary>
    /// When the action occurred.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The action that was performed (Create, Read, Update, Delete, Export).
    /// </summary>
    public AuditAction Action { get; set; }

    /// <summary>
    /// The type of resource affected.
    /// </summary>
    public string ResourceType { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the resource affected.
    /// </summary>
    public string? ResourceId { get; set; }

    /// <summary>
    /// Human-readable resource name for easier reporting.
    /// </summary>
    public string? ResourceName { get; set; }

    /// <summary>
    /// The result of the action (Success, Failure, Denied).
    /// </summary>
    public AuditResult Result { get; set; }

    /// <summary>
    /// Details about what changed (JSON format).
    /// Contains before/after values for tracking changes.
    /// </summary>
    public string? Changes { get; set; }

    /// <summary>
    /// Reason for the action (for compliance tracking).
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Error message if the action failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// IP address from which the action was performed.
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent / Device information.
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Geolocation of the action (if available).
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Correlation ID for linking related audit entries.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Indicates if PII was accessed in this action.
    /// </summary>
    public bool AccessedPII { get; set; }

    /// <summary>
    /// The specific PII fields that were accessed (for tracking consent).
    /// </summary>
    public string? AccessedPIIFields { get; set; }

    /// <summary>
    /// Hash of the audit log for integrity verification (immutability proof).
    /// </summary>
    public string? IntegrityHash { get; set; }

    /// <summary>
    /// Indicates if this record has been verified/sealed.
    /// </summary>
    public bool IsSealed { get; set; }

    // ── Integrity ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Compute a SHA-256 integrity hash over the immutable audit fields and seal
    /// this record.  Call once before persisting; any subsequent modification will
    /// invalidate the hash and can be detected by <see cref="VerifyIntegrity"/>.
    /// </summary>
    public AuditLog Seal()
    {
        IntegrityHash = ComputeHash();
        IsSealed      = true;
        return this;
    }

    /// <summary>
    /// Returns <c>true</c> if the record has not been tampered with since sealing.
    /// </summary>
    public bool VerifyIntegrity() =>
        IsSealed && IntegrityHash == ComputeHash();

    private string ComputeHash()
    {
        // Canonical payload — covers all immutable identity/action fields.
        var payload = string.Join("|",
            Id, UserId, Timestamp.ToString("O"),
            (int)Action, ResourceType, ResourceId ?? "",
            (int)Result, CorrelationId ?? "");

        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(payload);
        return Convert.ToBase64String(sha.ComputeHash(bytes));
    }
}
