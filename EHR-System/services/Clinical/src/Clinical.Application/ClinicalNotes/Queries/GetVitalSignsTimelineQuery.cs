using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.Services.Clinical.Contracts.Responses;

namespace EHRPlatform.Services.Clinical.Application.ClinicalNotes.Queries;

/// <summary>
/// Get patient vital signs timeline with optional date range.
/// </summary>
public record GetVitalSignsTimelineQuery : IQuery<List<VitalSignsResponse>>
{
    public Guid PatientId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}
