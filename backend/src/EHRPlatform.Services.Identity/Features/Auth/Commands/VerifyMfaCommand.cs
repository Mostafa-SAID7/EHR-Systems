#nullable enable

using EHRPlatform.Common.Application.CQRS;
using EHRPlatform.Services.Identity.Application.Identity.DTOs.Responses;

namespace EHRPlatform.Services.Identity.Features.Auth.Commands;

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

