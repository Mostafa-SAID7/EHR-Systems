#nullable enable

using EHRPlatform.BuildingBlocks.Common.Application.CQRS;

namespace EHRPlatform.Services.Identity.Application.Features.Auth.Commands;

/// <summary>
/// Logout command to revoke refresh token.
/// </summary>
public class LogoutCommand : ICommand<LogoutResponse>
{
    /// <summary>
    /// User ID.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Refresh token to revoke.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
/// Logout response.
/// </summary>
public class LogoutResponse
{
    /// <summary>
    /// Success message.
    /// </summary>
    public string Message { get; set; } = "Logged out successfully";
}



