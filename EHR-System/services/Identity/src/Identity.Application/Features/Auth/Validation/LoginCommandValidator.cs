#nullable enable

using EHRPlatform.Services.Identity.Features.Auth.Commands;
using FluentValidation;

namespace EHRPlatform.Services.Identity.Application.Features.Auth.Validation;

/// <summary>
/// Validator for login command.
/// </summary>
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    /// <summary>
    /// Initialize login command validator with rules.
    /// </summary>
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters");
    }
}

