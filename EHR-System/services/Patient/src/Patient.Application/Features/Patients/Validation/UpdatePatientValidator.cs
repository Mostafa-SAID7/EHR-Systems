using FluentValidation;
using EHRPlatform.Services.Patient.Application.Features.Patients.Commands;

namespace EHRPlatform.Services.Patient.Application.Features.Patients.Validation;

/// <summary>
/// Validator for UpdatePatientCommand.
/// </summary>
public class UpdatePatientValidator : AbstractValidator<UpdatePatientCommand>
{
    public UpdatePatientValidator()
    {
        RuleFor(x => x.PatientId)
            .NotEmpty().WithMessage("PatientId is required");

        RuleFor(x => x.FirstName)
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.FirstName));

        RuleFor(x => x.LastName)
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.LastName));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Email format is invalid")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Phone number format is invalid")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.Gender)
            .Must(x => new[] { "Male", "Female", "Other" }.Contains(x))
            .WithMessage("Gender must be Male, Female, or Other")
            .When(x => !string.IsNullOrEmpty(x.Gender));
    }
}

