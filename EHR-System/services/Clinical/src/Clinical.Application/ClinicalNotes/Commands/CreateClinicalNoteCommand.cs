using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.Services.Clinical.Contracts.Responses;

namespace EHRPlatform.Services.Clinical.Application.ClinicalNotes.Commands;

/// <summary>
/// Create clinical note command.
/// Initializes SOAP note in draft status.
/// </summary>
public record CreateClinicalNoteCommand : ICommand<ClinicalNoteResponse>
{
    public Guid PatientId { get; init; }
    public Guid ProviderId { get; init; }
    public DateTime EncounterDate { get; init; }
    public string EncounterType { get; init; } = string.Empty; // Office, Telehealth, Emergency, Hospital

    // Optional SOAP fields for initial note creation
    public string? Subjective { get; init; }
    public string? Objective { get; init; }
    public string? Assessment { get; init; }
    public string? Plan { get; init; }
}
