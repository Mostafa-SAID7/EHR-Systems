using FluentValidation;
using EHRPlatform.Services.Prescription.Features.Prescriptions.Commands;

namespace EHRPlatform.Services.Prescription.Features.Prescriptions.Validation;

/// <summary>
/// Validator for IssuePrescriptionCommand.
/// Single Responsibility: Enforce all business rules for issuing a new prescription.
/// </summary>
public class IssuePrescriptionCommandValidator : AbstractValidator<IssuePrescriptionCommand>
{
    public IssuePrescriptionCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.ProviderId).NotEmpty();
        RuleFor(x => x.MedicationName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Strength).NotEmpty();
        RuleFor(x => x.Dosage).NotEmpty();
        RuleFor(x => x.Frequency).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.RefillsAllowed).GreaterThanOrEqualTo(0);
        RuleFor(x => x.StartDate).LessThanOrEqualTo(DateTime.UtcNow);
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate).When(x => x.EndDate.HasValue);
    }
}
