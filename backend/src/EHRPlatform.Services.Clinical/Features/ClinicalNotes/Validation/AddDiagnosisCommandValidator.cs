using FluentValidation;
using EHRPlatform.Services.Clinical.Features.ClinicalNotes.Commands;

namespace EHRPlatform.Services.Clinical.Features.ClinicalNotes.Validation;

/// <summary>
/// Validator for AddDiagnosisCommand.
/// Single Responsibility: Enforce ICD-10 code format and required fields for adding a diagnosis.
/// </summary>
public class AddDiagnosisCommandValidator : AbstractValidator<AddDiagnosisCommand>
{
    public AddDiagnosisCommandValidator()
    {
        RuleFor(x => x.ClinicalNoteId)
            .NotEmpty().WithMessage("ClinicalNoteId is required");
        RuleFor(x => x.DiagnosisCode)
            .NotEmpty().WithMessage("DiagnosisCode is required")
            .Matches(@"^[A-Z][0-9]{2}(\.[0-9]{1,2})?$")
            .WithMessage("DiagnosisCode must be a valid ICD-10 format (e.g. A00 or A00.1)");
        RuleFor(x => x.DiagnosisText)
            .NotEmpty().WithMessage("DiagnosisText is required");
        RuleFor(x => x.DiagnosisType)
            .Must(t => new[] { "Principal", "Secondary" }.Contains(t))
            .WithMessage("DiagnosisType must be 'Principal' or 'Secondary'");
    }
}
