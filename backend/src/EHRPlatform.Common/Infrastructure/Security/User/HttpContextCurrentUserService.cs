#nullable enable

using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace EHRPlatform.Common.Infrastructure.Security;

/// <summary>
/// HTTP-context-scoped implementation of <see cref="ICurrentUserService"/>.
/// Reads identity claims from the JWT validated by ASP.NET Core middleware.
///
/// Registration: call <c>services.AddHttpContextAccessor()</c> then
/// <c>services.AddScoped&lt;ICurrentUserService, HttpContextCurrentUserService&gt;()</c>.
///
/// HIPAA:
///  - UserEmail is exposed only for audit trail purposes; never include in response DTOs.
///  - Log only UserId (opaque GUID), never name or email, in structured log properties.
/// </summary>
public sealed class HttpContextCurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _accessor;

    public HttpContextCurrentUserService(IHttpContextAccessor accessor)
    {
        _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
    }

    private ClaimsPrincipal? Principal => _accessor.HttpContext?.User;

    /// <inheritdoc/>
    public Guid UserId
    {
        get
        {
            var raw = Principal?.FindFirstValue("sub")
                   ?? Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(raw, out var id) ? id : Guid.Empty;
        }
    }

    /// <inheritdoc/>
    public string? UserEmail =>
        Principal?.FindFirstValue("email")
        ?? Principal?.FindFirstValue(ClaimTypes.Email);

    /// <inheritdoc/>
    public string? UserRole =>
        Principal?.FindFirstValue("role")
        ?? Principal?.FindFirstValue(ClaimTypes.Role);

    /// <inheritdoc/>
    public Guid? TenantId
    {
        get
        {
            var raw = Principal?.FindFirstValue("tenant_id")
                   ?? Principal?.FindFirstValue("tenantId");
            return Guid.TryParse(raw, out var id) ? id : null;
        }
    }

    /// <inheritdoc/>
    public bool IsAuthenticated =>
        Principal?.Identity?.IsAuthenticated == true;
}

