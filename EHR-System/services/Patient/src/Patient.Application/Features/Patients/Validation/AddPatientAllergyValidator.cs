using FluentValidation;
using EHRPlatform.Services.Patient.Application.Features.Patients.Commands;

namespace EHRPlatform.Services.Patient.Application.Features.Patients.Validation;

/// <summary>
/// Validator for AddPatientAllergyCommand.
/// </summary>
public class AddPatientAllergyValidator : AbstractValidator<AddPatientAllergyCommand>
{
    public AddPatientAllergyValidator()
    {
        RuleFor(x => x.PatientId)
            .NotEmpty().WithMessage("PatientId is required");

        RuleFor(x => x.Allergen)
            .NotEmpty().WithMessage("Allergen is required")
            .MaximumLength(200).WithMessage("Allergen must not exceed 200 characters");

        RuleFor(x => x.Severity)
            .NotEmpty().WithMessage("Severity is required")
            .Must(x => new[] { "Mild", "Moderate", "Severe" }.Contains(x))
            .WithMessage("Severity must be Mild, Moderate, or Severe");
    }
}

