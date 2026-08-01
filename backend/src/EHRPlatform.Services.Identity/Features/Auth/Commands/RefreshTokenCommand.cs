#nullable enable

using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.Services.Identity.Application.Identity.DTOs.Responses;

namespace EHRPlatform.Services.Identity.Features.Auth.Commands;

/// <summary>
/// Refresh access token command.
/// </summary>
public class RefreshTokenCommand : ICommand<LoginResponse>
{
    /// <summary>
    /// Current access token.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Refresh token.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
}


