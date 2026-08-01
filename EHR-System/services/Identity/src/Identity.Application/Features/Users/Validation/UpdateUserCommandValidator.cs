#nullable enable

using EHRPlatform.Services.Identity.Features.Users.Commands;
using FluentValidation;

namespace EHRPlatform.Services.Identity.Application.Features.Users.Validation;

/// <summary>
/// Validator for update user command.
/// </summary>
public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    /// <summary>
    /// Initialize update user validator with rules.
    /// </summary>
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.FirstName)
            .MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.FirstName))
            .WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.LastName))
            .WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("Email must be a valid email address");

        RuleFor(x => x.UpdatedBy)
            .NotEmpty().WithMessage("UpdatedBy user ID is required");
    }
}

