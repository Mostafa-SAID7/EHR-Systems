#nullable enable

using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.Services.Identity.Application.Identity.DTOs.Responses;

namespace EHRPlatform.Services.Identity.Features.Auth.Commands;

/// <summary>
/// Setup MFA command.
/// </summary>
public class SetupMfaCommand : ICommand<SetupMfaResponse>
{
    /// <summary>
    /// User ID.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// MFA method (e.g., "TOTP" or "EMAIL").
    /// </summary>
    public string MfaMethod { get; set; } = "TOTP";
}


