using EHRPlatform.Common.CQRS;
using EHRPlatform.Services.Clinical.Application.ClinicalNotes.Responses;
using FluentValidation;

namespace EHRPlatform.Services.Clinical.Features.ClinicalNotes.Commands;

/// <summary>
/// Record vital signs command.
/// </summary>
public record RecordVitalsCommand : ICommand<ClinicalNoteResponse>
{
    public Guid ClinicalNoteId { get; init; }
    public decimal Temperature { get; init; } // Celsius
    public int SystolicBP { get; init; }
    public int DiastolicBP { get; init; }
    public int HeartRate { get; init; }
    public int RespiratoryRate { get; init; }
    public decimal? Weight { get; init; }
}

public class RecordVitalsCommandValidator : AbstractValidator<RecordVitalsCommand>
{
    public RecordVitalsCommandValidator()
    {
        RuleFor(x => x.ClinicalNoteId).NotEmpty();
        RuleFor(x => x.Temperature).GreaterThan(35).LessThan(42); // Normal range + fever
        RuleFor(x => x.SystolicBP).GreaterThan(60).LessThan(250);
        RuleFor(x => x.DiastolicBP).GreaterThan(40).LessThan(150);
        RuleFor(x => x.HeartRate).GreaterThan(20).LessThan(250);
        RuleFor(x => x.RespiratoryRate).GreaterThan(8).LessThan(60);
        RuleFor(x => x.Weight).GreaterThan(5).LessThan(500).When(x => x.Weight.HasValue);
    }
}
