namespace EHRPlatform.Services.Identity.Domain.Entities;

/// <summary>
/// User aggregate root - Core user account entity with roles and authentication.
/// Properties: Email, PasswordHash, Status (Active/Locked/Inactive), LastLogin, MFA setup
/// RBAC: Users have multiple Roles; Roles have multiple Permissions
/// </summary>
public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty; // Bcrypt
    public bool EmailVerified { get; set; }
    public DateTime? EmailVerifiedAt { get; set; }
    public string Status { get; set; } = "Active"; // Active, Locked, Inactive, Pending
    public int LoginFailureCount { get; set; }
    public DateTime? LastLockedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    
    // MFA
    public bool MfaEnabled { get; set; }
    public string? MfaSecret { get; set; } // TOTP secret (encrypted)
    public string? PhoneNumber { get; set; }
    public bool PhoneNumberVerified { get; set; }
    
    // External Auth
    public string? ExternalProviderId { get; set; } // OAuth provider ID (Google, Microsoft, etc.)
    public string? ExternalProvider { get; set; } // google, microsoft, apple, etc.
    
    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; } // Soft delete
    public bool IsDeleted { get; set; }

    public ICollection<UserRole> UserRoles { get; } = new List<UserRole>();
    public ICollection<RefreshToken> RefreshTokens { get; } = new List<RefreshToken>();

    private readonly List<object> _domainEvents = new();

    public void SetPassword(string hashedPassword)
    {
        PasswordHash = hashedPassword;
        UpdatedAt = DateTime.UtcNow;
    }

    public void VerifyEmail()
    {
        EmailVerified = true;
        EmailVerifiedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        RaiseEvent(new UserEmailVerifiedEvent(Id, Email));
    }

    public void EnableMfa(string secret)
    {
        MfaEnabled = true;
        MfaSecret = secret;
        UpdatedAt = DateTime.UtcNow;
        RaiseEvent(new UserMfaEnabledEvent(Id, Email));
    }

    public void DisableMfa()
    {
        MfaEnabled = false;
        MfaSecret = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Lock()
    {
        Status = "Locked";
        LastLockedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        RaiseEvent(new UserLockedEvent(Id, Email));
    }

    public void Unlock()
    {
        Status = "Active";
        LoginFailureCount = 0;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordLoginFailure()
    {
        LoginFailureCount++;
        if (LoginFailureCount >= 5)
        {
            Lock();
        }
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordLoginSuccess()
    {
        LastLoginAt = DateTime.UtcNow;
        LoginFailureCount = 0;
        UpdatedAt = DateTime.UtcNow;
        RaiseEvent(new UserLoggedInEvent(Id, Email, LastLoginAt.Value));
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        Status = "Inactive";
        UpdatedAt = DateTime.UtcNow;
        RaiseEvent(new UserDeletedEvent(Id, Email));
    }

    public void LinkExternalProvider(string provider, string providerId)
    {
        ExternalProvider = provider;
        ExternalProviderId = providerId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RaiseEvent(object @event) => _domainEvents.Add(@event);
    public IReadOnlyList<object> GetDomainEvents() => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();
}

/// <summary>
/// Role - Role definition for RBAC
/// </summary>
public class Role
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty; // Admin, Doctor, Nurse, Patient, Receptionist
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<UserRole> UserRoles { get; } = new List<UserRole>();
    public ICollection<RolePermission> RolePermissions { get; } = new List<RolePermission>();
}

/// <summary>
/// Permission - Permission definition (resource + action)
/// </summary>
public class Permission
{
    public Guid Id { get; set; }
    public string Resource { get; set; } = string.Empty; // Patient, Appointment, Invoice, etc.
    public string Action { get; set; } = string.Empty; // Create, Read, Update, Delete, Export
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public ICollection<RolePermission> RolePermissions { get; } = new List<RolePermission>();
}

/// <summary>
/// UserRole - Many-to-many relationship between User and Role
/// </summary>
public class UserRole
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public DateTime AssignedAt { get; set; }

    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}

/// <summary>
/// RolePermission - Many-to-many relationship between Role and Permission
/// </summary>
public class RolePermission
{
    public Guid Id { get; set; }
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }
    public DateTime AssignedAt { get; set; }

    public Role Role { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}

/// <summary>
/// RefreshToken - JWT refresh token tracking for token management
/// </summary>
public class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByToken { get; set; }

    public User User { get; set; } = null!;

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;
}

// Domain Events
public record UserCreatedEvent(Guid UserId, string Email, string FirstName, string LastName)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}

public record UserUpdatedEvent(Guid UserId, string Email)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}

public record UserDeletedEvent(Guid UserId, string Email)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}

public record PasswordChangedEvent(Guid UserId, string Email)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}

public record UserEmailVerifiedEvent(Guid UserId, string Email)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}

public record UserMfaEnabledEvent(Guid UserId, string Email)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}

public record UserLockedEvent(Guid UserId, string Email)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}

public record UserLoggedInEvent(Guid UserId, string Email, DateTime LoginTime)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}
