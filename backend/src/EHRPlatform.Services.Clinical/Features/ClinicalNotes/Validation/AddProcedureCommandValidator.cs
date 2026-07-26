using FluentValidation;
using EHRPlatform.Services.Clinical.Features.ClinicalNotes.Commands;

namespace EHRPlatform.Services.Clinical.Features.ClinicalNotes.Validation;

/// <summary>
/// Validator for AddProcedureCommand.
/// Single Responsibility: Enforce required fields for adding a clinical procedure (CPT/SNOMED codes).
/// </summary>
public class AddProcedureCommandValidator : AbstractValidator<AddProcedureCommand>
{
    public AddProcedureCommandValidator()
    {
        RuleFor(x => x.ClinicalNoteId)
            .NotEmpty().WithMessage("ClinicalNoteId is required");
        RuleFor(x => x.ProcedureName)
            .NotEmpty().WithMessage("ProcedureName is required")
            .MaximumLength(255).WithMessage("ProcedureName must not exceed 255 characters");
        RuleFor(x => x.ProcedureCode)
            .NotEmpty().WithMessage("ProcedureCode (CPT or SNOMED) is required");
    }
}
