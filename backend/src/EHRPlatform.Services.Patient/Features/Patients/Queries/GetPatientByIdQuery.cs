using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.Services.Patient.Application.Patients.Responses;

namespace EHRPlatform.Services.Patient.Features.Patients.Queries;

/// <summary>
/// Query to retrieve a single patient by their ID.
/// </summary>
public class GetPatientByIdQuery : IQuery<PatientResponse>
{
    public Guid PatientId { get; set; }
}


