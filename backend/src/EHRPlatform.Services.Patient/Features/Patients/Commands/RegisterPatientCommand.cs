using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.Services.Patient.Application.Patients.Responses;

namespace EHRPlatform.Services.Patient.Features.Patients.Commands;

/// <summary>
/// Register patient command — creates a new patient with a pre-assigned MRN.
/// </summary>
public record RegisterPatientCommand : ICommand<PatientResponse>
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public DateTime DateOfBirth { get; init; }
    public string Gender { get; init; } = string.Empty;
    public string MRN { get; init; } = string.Empty;
    public string BloodType { get; init; } = string.Empty;
    public string? EmergencyContact { get; init; }
    public string? EmergencyPhone { get; init; }
}


