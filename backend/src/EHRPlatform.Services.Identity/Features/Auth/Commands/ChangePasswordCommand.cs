#nullable enable

using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.Services.Identity.Application.Identity.DTOs.Responses;

namespace EHRPlatform.Services.Identity.Features.Auth.Commands;

/// <summary>
/// Change password command.
/// </summary>
public class ChangePasswordCommand : ICommand<ChangePasswordResponse>
{
    /// <summary>
    /// User ID.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Current password.
    /// </summary>
    public string CurrentPassword { get; set; } = string.Empty;

    /// <summary>
    /// New password.
    /// </summary>
    public string NewPassword { get; set; } = string.Empty;
}


