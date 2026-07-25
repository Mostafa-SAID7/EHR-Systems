using EHRPlatform.Common.CQRS;
using EHRPlatform.Services.Patient.Application.Patients.Responses;

namespace EHRPlatform.Services.Patient.Features.Patients.Commands;

/// <summary>
/// Add allergy to an existing patient's record.
/// </summary>
public record AddPatientAllergyCommand : ICommand<PatientResponse>
{
    public Guid PatientId { get; init; }
    public string Allergen { get; init; } = string.Empty;
    public string Severity { get; init; } = string.Empty;
    public string? Notes { get; init; }
}
