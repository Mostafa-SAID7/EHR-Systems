using EHRPlatform.Common.CQRS;
using EHRPlatform.Services.Patient.Application.Patients.Responses;

namespace EHRPlatform.Services.Patient.Features.Patients.Queries;

public class GetPatientsQuery : IQuery<PatientListDto>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Status { get; set; }
}
