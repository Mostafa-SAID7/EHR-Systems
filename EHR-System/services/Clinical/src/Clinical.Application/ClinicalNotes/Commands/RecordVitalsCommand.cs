using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.Services.Clinical.Contracts.Responses;

namespace EHRPlatform.Services.Clinical.Application.ClinicalNotes.Commands;

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
