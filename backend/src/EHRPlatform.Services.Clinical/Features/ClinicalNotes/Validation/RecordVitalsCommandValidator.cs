using FluentValidation;
using EHRPlatform.Services.Clinical.Features.ClinicalNotes.Commands;

namespace EHRPlatform.Services.Clinical.Features.ClinicalNotes.Validation;

/// <summary>
/// Validator for RecordVitalsCommand.
/// Single Responsibility: Enforce physiological range constraints on vital sign recordings.
/// </summary>
public class RecordVitalsCommandValidator : AbstractValidator<RecordVitalsCommand>
{
    public RecordVitalsCommandValidator()
    {
        RuleFor(x => x.ClinicalNoteId).NotEmpty();
        RuleFor(x => x.Temperature)
            .GreaterThan(35).WithMessage("Temperature must be above 35°C")
            .LessThan(42).WithMessage("Temperature must be below 42°C");
        RuleFor(x => x.SystolicBP)
            .GreaterThan(60).WithMessage("Systolic BP must be above 60 mmHg")
            .LessThan(250).WithMessage("Systolic BP must be below 250 mmHg");
        RuleFor(x => x.DiastolicBP)
            .GreaterThan(40).WithMessage("Diastolic BP must be above 40 mmHg")
            .LessThan(150).WithMessage("Diastolic BP must be below 150 mmHg");
        RuleFor(x => x.HeartRate)
            .GreaterThan(20).WithMessage("Heart rate must be above 20 bpm")
            .LessThan(250).WithMessage("Heart rate must be below 250 bpm");
        RuleFor(x => x.RespiratoryRate)
            .GreaterThan(8).WithMessage("Respiratory rate must be above 8 breaths/min")
            .LessThan(60).WithMessage("Respiratory rate must be below 60 breaths/min");
        RuleFor(x => x.Weight)
            .GreaterThan(5).WithMessage("Weight must be above 5 kg")
            .LessThan(500).WithMessage("Weight must be below 500 kg")
            .When(x => x.Weight.HasValue);
    }
}
