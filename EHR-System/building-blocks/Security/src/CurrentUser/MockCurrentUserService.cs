using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace EHRPlatform.Security.CurrentUser;

/// <summary>
/// Mock implementation for unit testing.
/// Single responsibility: Test double for CurrentUserService.
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
