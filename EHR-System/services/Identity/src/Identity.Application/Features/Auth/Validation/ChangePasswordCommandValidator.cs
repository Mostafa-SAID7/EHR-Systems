#nullable enable

using EHRPlatform.Services.Identity.Features.Auth.Commands;
using FluentValidation;

namespace EHRPlatform.Services.Identity.Application.Features.Auth.Validation;

/// <summary>
/// Validator for change password command.
/// Enforces HIPAA-compliant password requirements.
/// </summary>
public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    /// <summary>
    /// Initialize change password validator with rules.
    /// </summary>
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required")
            .MinimumLength(12).WithMessage("New password must be at least 12 characters")
            .Matches(@"[A-Z]").WithMessage("New password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("New password must contain at least one lowercase letter")
            .Matches(@"[0-9]").WithMessage("New password must contain at least one digit")
            .Matches(@"[!@#$%^&*]").WithMessage("New password must contain at least one special character (!@#$%^&*)")
            .NotEqual(x => x.CurrentPassword).WithMessage("New password must be different from current password");
    }
}

