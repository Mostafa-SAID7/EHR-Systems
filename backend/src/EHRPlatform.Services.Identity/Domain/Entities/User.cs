using EHRPlatform.Common.Domain.Entities;

namespace EHRPlatform.Services.Identity.Domain.Entities;

/// <summary>
/// User entity for identity and access management.
/// HIPAA compliant with audit trail.
/// </summary>
public class User : AuditableEntity
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool EmailConfirmed { get; set; }
    public DateTime? LastLogin { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEnd { get; set; }
    public bool MfaEnabled { get; set; }
    public string? MfaSecret { get; set; }
    public string? MfaSecretBackupCodes { get; set; }

    // Collections
    public ICollection<UserRole>     Roles         { get; } = new List<UserRole>();
    public ICollection<RefreshToken> RefreshTokens { get; } = new List<RefreshToken>();
    public ICollection<LoginAudit>   LoginAudits   { get; } = new List<LoginAudit>();
    public ICollection<MfaSetup>     MfaSetups     { get; } = new List<MfaSetup>();

    public bool IsLocked() => LockoutEnd.HasValue && LockoutEnd > DateTime.UtcNow;

    public void Lock() => LockoutEnd = DateTime.UtcNow.AddMinutes(15);

    public void Unlock()
    {
        LockoutEnd = null;
        FailedLoginAttempts = 0;
    }
}

