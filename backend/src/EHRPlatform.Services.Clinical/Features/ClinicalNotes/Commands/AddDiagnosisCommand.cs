using EHRPlatform.Services.Clinical.Application.ClinicalNotes.Responses;
using EHRPlatform.Common.CQRS;
using FluentValidation;

namespace EHRPlatform.Services.Clinical.Features.ClinicalNotes.Commands;

/// <summary>
/// Add diagnosis command.
/// </summary>
public record AddDiagnosisCommand : ICommand<ClinicalNoteResponse>
{
    public Guid ClinicalNoteId { get; init; }
    public string DiagnosisCode { get; init; } = string.Empty; // ICD-10
    public string DiagnosisText { get; init; } = string.Empty;
    public string DiagnosisType { get; init; } = "Secondary"; // Principal or Secondary
}

public class AddDiagnosisCommandValidator : AbstractValidator<AddDiagnosisCommand>
{
    public AddDiagnosisCommandValidator()
    {
        RuleFor(x => x.ClinicalNoteId).NotEmpty();
        RuleFor(x => x.DiagnosisCode).NotEmpty().Matches(@"^[A-Z][0-9]{2}(\.[0-9]{1,2})?$");
        RuleFor(x => x.DiagnosisText).NotEmpty();
        RuleFor(x => x.DiagnosisType).Must(t => new[] { "Principal", "Secondary" }.Contains(t));
    }
}
