using EHRPlatform.Common.Application.CQRS;
using EHRPlatform.Services.Billing.Application.Reports.Responses;

namespace EHRPlatform.Services.Billing.Features.Reports.Queries;

/// <summary>
/// Get outstanding balance - query.
/// Retrieves comprehensive balance information for a patient.
/// </summary>
public record GetPatientOutstandingBalanceQuery : IQuery<OutstandingBalanceDto>
{
    public Guid PatientId { get; init; }
}

