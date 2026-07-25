using FluentValidation;
using EHRPlatform.Services.Prescription.Features.Prescriptions.Commands;

namespace EHRPlatform.Services.Prescription.Features.Prescriptions.Validation;

public class CreatePrescriptionValidator : AbstractValidator<IssuePrescriptionCommand>
{
    public CreatePrescriptionValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.ProviderId).NotEmpty();
        RuleFor(x => x.MedicationName).NotEmpty();
    }
}
