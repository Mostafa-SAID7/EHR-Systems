using FluentValidation;
using EHRPlatform.Services.Patient.Application.Features.Patients.Commands;

namespace EHRPlatform.Services.Patient.Application.Features.Patients.Validation;

public class AddAllergyCommandValidator : AbstractValidator<AddAllergyCommand>
{
    public AddAllergyCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.Allergen).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Severity).Must(s => new[] { "Mild", "Moderate", "Severe" }.Contains(s));
    }
}

