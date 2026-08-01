using FluentValidation;
using EHRPlatform.Services.Analytics.Features.Analytics.Commands;

namespace EHRPlatform.Services.Analytics.Features.Analytics.Validation;

/// <summary>
/// Validator for CreateReportCommand.
/// </summary>
public class CreateReportValidator : AbstractValidator<CreateReportCommand>
{
    public CreateReportValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");
    }
}

public class CreateReportCommand
{
    public Guid UserId { get; set; }
    public string? Name { get; set; }
}
