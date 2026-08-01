using FluentValidation;
using EHRPlatform.Services.Patient.Application.Features.Patients.Commands;

namespace EHRPlatform.Services.Patient.Application.Features.Patients.Validation;

/// <summary>
/// Validator for RegisterPatientCommand.
/// </summary>
public class RegisterPatientValidator : AbstractValidator<RegisterPatientCommand>
{
    public RegisterPatientValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email format is invalid");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Phone number format is invalid");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Date of birth is required")
            .LessThan(DateTime.UtcNow).WithMessage("Date of birth cannot be in the future");

        RuleFor(x => x.MRN)
            .NotEmpty().WithMessage("Medical Record Number is required")
            .Matches(@"^[A-Z0-9\-]+$").WithMessage("MRN format is invalid");

        RuleFor(x => x.Gender)
            .NotEmpty().WithMessage("Gender is required")
            .Must(x => new[] { "Male", "Female", "Other" }.Contains(x))
            .WithMessage("Gender must be Male, Female, or Other");

        RuleFor(x => x.BloodType)
            .Must(x => new[] { "O+", "O-", "A+", "A-", "B+", "B-", "AB+", "AB-" }.Contains(x))
            .WithMessage("Blood type must be a valid blood type");
    }
}

