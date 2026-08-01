using EHRPlatform.Services.Clinical.Contracts.Responses;
using EHRPlatform.BuildingBlocks.Common.Application.CQRS;

namespace EHRPlatform.Services.Clinical.Application.ClinicalNotes.Commands;

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
