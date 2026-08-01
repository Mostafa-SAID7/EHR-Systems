#nullable enable

using EHRPlatform.Services.Identity.Features.Auth.Commands;
using FluentValidation;

namespace EHRPlatform.Services.Identity.Application.Features.Auth.Validation;

/// <summary>
/// Validator for verify MFA command.
/// </summary>
public class VerifyMfaCommandValidator : AbstractValidator<VerifyMfaCommand>
{
    /// <summary>
    /// Initialize MFA verification validator with rules.
    /// </summary>
    public VerifyMfaCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Verification code is required")
            .Length(6).WithMessage("Verification code must be exactly 6 digits")
            .Matches(@"^\d{6}$").WithMessage("Verification code must contain only digits");
    }
}

