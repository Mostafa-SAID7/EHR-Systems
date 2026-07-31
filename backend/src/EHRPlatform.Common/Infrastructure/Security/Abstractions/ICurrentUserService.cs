#nullable enable

namespace EHRPlatform.Common.Infrastructure.Security;

/// <summary>
/// Provides the current authenticated user's identity.
/// Inject this in handlers and services that need to know who is acting.
/// HIPAA: Every command that modifies data should record the acting user.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>ID of the authenticated user, or <see cref="Guid.Empty"/> for system/anonymous.</summary>
    Guid UserId { get; }

    /// <summary>Email address of the authenticated user.</summary>
    string? UserEmail { get; }

    /// <summary>Primary role of the authenticated user (e.g. "Doctor", "Admin").</summary>
    string? UserRole { get; }

    /// <summary>Tenant / organisation ID for multi-tenant deployments.</summary>
    Guid? TenantId { get; }

    /// <summary>Returns true if a real authenticated user is present.</summary>
    bool IsAuthenticated { get; }
}

/// <summary>
/// System-level implementation used by background services and seed jobs where
/// there is no HTTP context.  All PHI access performed under this identity is
/// flagged as a system operation in the audit trail.
/// </summary>
public sealed class SystemCurrentUserService : ICurrentUserService
{
    public static readonly SystemCurrentUserService Instance = new();

    public Guid   UserId          => Guid.Empty;
    public string? UserEmail      => "system@ehr-platform.internal";
    public string? UserRole       => "System";
    public Guid?  TenantId        => null;
    public bool   IsAuthenticated => false;
}

