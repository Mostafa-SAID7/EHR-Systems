using EHRPlatform.Common.Domain.Entities;

namespace EHRPlatform.Services.Identity.Domain.Entities;

/// <summary>
/// MFA (Multi-Factor Authentication) setup record for a user.
/// Supports TOTP, SMS, and EMAIL second factors.
/// A user may have at most one verified setup per type.
/// </summary>
public class MfaSetup : BaseEntity
{
    public Guid     UserId     { get; set; }
    public string   MfaType   { get; set; } = string.Empty; // "TOTP" | "SMS" | "EMAIL"
    public string   Secret    { get; set; } = string.Empty; // encrypted TOTP secret or masked phone/email
    public bool     IsVerified { get; set; }
    public DateTime SetupAt   { get; set; } = DateTime.UtcNow;
    public DateTime? VerifiedAt { get; set; }

    // ── Navigation ───────────────────────────────────────────────────────────
    public User? User { get; set; }
}

