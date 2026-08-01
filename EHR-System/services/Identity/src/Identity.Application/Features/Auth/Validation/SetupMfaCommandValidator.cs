#nullable enable

using EHRPlatform.Services.Identity.Features.Auth.Commands;
using FluentValidation;

namespace EHRPlatform.Services.Identity.Application.Features.Auth.Validation;

/// <summary>
/// Validator for setup MFA command.
/// </summary>
public class SetupMfaCommandValidator : AbstractValidator<SetupMfaCommand>
{
    /// <summary>
    /// Initialize MFA setup validator with rules.
    /// </summary>
    public SetupMfaCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.MfaMethod)
            .NotEmpty().WithMessage("MFA method is required")
            .Must(m => m == "TOTP" || m == "EMAIL").WithMessage("MFA method must be either TOTP or EMAIL");
    }
}

