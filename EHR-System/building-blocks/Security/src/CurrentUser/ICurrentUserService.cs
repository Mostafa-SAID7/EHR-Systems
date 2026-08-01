using System.Collections.Generic;

namespace EHRPlatform.Security.CurrentUser;

/// <summary>
/// Service to access current authenticated user context.
/// Single responsibility: Current user interface contract.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Get ID of current user.
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Get username of current user.
    /// </summary>
    string? UserName { get; }

    /// <summary>
    /// Get email of current user.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Get roles of current user.
    /// </summary>
    IEnumerable<string> Roles { get; }

    /// <summary>
    /// Check if user is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Check if user has specific role.
    /// </summary>
    bool HasRole(string role);

    /// <summary>
    /// Check if user has any of the specified roles.
    /// </summary>
    bool HasAnyRole(params string[] roles);

    /// <summary>
    /// Get claim value.
    /// </summary>
    string? GetClaimValue(string claimType);
}
