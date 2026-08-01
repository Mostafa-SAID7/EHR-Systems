using EHRPlatform.Services.Clinical.Application.ClinicalNotes.Responses;
using EHRPlatform.BuildingBlocks.Common.Application.CQRS;

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


