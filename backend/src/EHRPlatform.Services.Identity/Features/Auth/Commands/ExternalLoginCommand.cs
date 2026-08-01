#nullable enable

using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.Services.Identity.Application.Identity.DTOs.Responses;

namespace EHRPlatform.Services.Identity.Features.Auth.Commands;

/// <summary>
/// Command for external OAuth authentication (Google, Facebook, etc.).
/// Single Responsibility: Encapsulate external provider payload (provider, idToken/accessToken).
/// </summary>
public class ExternalLoginCommand : ICommand<LoginResponse>
{
    /// <summary>
    /// OAuth Provider (e.g., "Google", "Facebook").
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// ID Token or Access Token from provider.
    /// </summary>
    public string IdToken { get; set; } = string.Empty;

    /// <summary>
    /// User email address obtained from OAuth provider payload.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// User last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// External Provider User Key / Subject ID.
    /// </summary>
    public string ProviderKey { get; set; } = string.Empty;
}


