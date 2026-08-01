#nullable enable

using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.Services.Identity.Contracts.Responses;

namespace EHRPlatform.Services.Identity.Application.Features.Auth.Commands;

/// <summary>
/// Verify MFA code command.
/// </summary>
public class VerifyMfaCommand : ICommand<VerifyMfaResponse>
{
    /// <summary>
    /// User ID.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// TOTP code (6 digits).
    /// </summary>
    public string Code { get; set; } = string.Empty;
}




