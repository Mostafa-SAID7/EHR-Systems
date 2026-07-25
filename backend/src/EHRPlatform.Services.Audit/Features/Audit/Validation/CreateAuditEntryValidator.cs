using FluentValidation;
using EHRPlatform.Services.Audit.Features.Audit.Commands;

namespace EHRPlatform.Services.Audit.Features.Audit.Validation;

/// <summary>
/// Validator for CreateAuditEntryCommand.
/// </summary>
public class CreateAuditEntryValidator : AbstractValidator<RecordAuditEntryCommand>
{
    public CreateAuditEntryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");

        RuleFor(x => x.Action)
            .NotEmpty().WithMessage("Action is required")
            .MaximumLength(50).WithMessage("Action must not exceed 50 characters");

        RuleFor(x => x.ResourceType)
            .NotEmpty().WithMessage("ResourceType is required");
    }
}
