using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace EHRPlatform.Security.CurrentUser;

/// <summary>
/// Implementation of CurrentUserService using HttpContext.
/// Single responsibility: HttpContext-based current user access.
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
        _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role)?.Select(c => c.Value) ?? Enumerable.Empty<string>();

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
