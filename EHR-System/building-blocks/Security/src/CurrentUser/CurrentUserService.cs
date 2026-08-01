using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace EHRPlatform.Security.CurrentUser;

/// <summary>
/// Service to access current authenticated user context.
/// Injected into application services to identify who performed an action.
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

/// <summary>
/// Implementation of CurrentUserService using HttpContext.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => GetClaimValue(ClaimTypes.NameIdentifier) ?? GetClaimValue("userId");

    public string? UserName => GetClaimValue(ClaimTypes.Name);

    public string? Email => GetClaimValue(ClaimTypes.Email);

    public IEnumerable<string> Roles =>
        _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role) ?? Enumerable.Empty<Claim>();

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public bool HasRole(string role)
    {
        return Roles.Any(r => r.Equals(role, StringComparison.OrdinalIgnoreCase));
    }

    public bool HasAnyRole(params string[] roles)
    {
        return roles.Any(HasRole);
    }

    public string? GetClaimValue(string claimType)
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst(claimType)?.Value;
    }
}

/// <summary>
/// Mock implementation for unit testing.
/// </summary>
public class MockCurrentUserService : ICurrentUserService
{
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public IEnumerable<string> Roles { get; set; } = new List<string>();
    public bool IsAuthenticated { get; set; }

    public bool HasRole(string role)
    {
        return Roles.Contains(role);
    }

    public bool HasAnyRole(params string[] roles)
    {
        return roles.Any(HasRole);
    }

    public string? GetClaimValue(string claimType)
    {
        return claimType switch
        {
            ClaimTypes.NameIdentifier => UserId,
            ClaimTypes.Name => UserName,
            ClaimTypes.Email => Email,
            _ => null
        };
    }
}
