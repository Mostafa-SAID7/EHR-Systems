using EHRPlatform.BuildingBlocks.Common.Application.Common.Models;
using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.Services.Clinical.Contracts.Responses;

namespace EHRPlatform.Services.Clinical.Application.ClinicalNotes.Queries;

/// <summary>
/// Get patient clinical notes timeline (paginated).
/// </summary>
public record GetPatientClinicalTimelineQuery : IQuery<PagedResult<ClinicalNoteResponse>>
{
    public Guid PatientId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
