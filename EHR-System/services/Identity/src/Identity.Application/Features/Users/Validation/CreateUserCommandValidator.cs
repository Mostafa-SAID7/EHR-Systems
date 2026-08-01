#nullable enable

using EHRPlatform.Services.Identity.Features.Users.Commands;
using FluentValidation;

namespace EHRPlatform.Services.Identity.Application.Features.Users.Validation;

/// <summary>
/// Validator for create user command.
/// </summary>
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    /// <summary>
    /// Initialize create user validator with rules.
    /// </summary>
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required")
            .Must(r => new[] { "Admin", "Doctor", "Nurse", "Receptionist", "Patient" }.Contains(r))
            .WithMessage("Role must be one of: Admin, Doctor, Nurse, Receptionist, Patient");

        RuleFor(x => x.CreatedBy)
            .NotEmpty().WithMessage("CreatedBy user ID is required");
    }
}

