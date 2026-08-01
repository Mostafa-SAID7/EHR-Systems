using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.Services.Clinical.Contracts.Responses;

namespace EHRPlatform.Services.Clinical.Application.ClinicalNotes.Queries;

/// <summary>
/// Get clinical note by ID query.
/// Includes vitals, diagnoses, procedures.
/// </summary>
public record GetClinicalNoteQuery : IQuery<ClinicalNoteResponse>
{
    public Guid ClinicalNoteId { get; init; }
}
