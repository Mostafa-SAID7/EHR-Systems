using EHRPlatform.Common.CQRS;
using EHRPlatform.Services.Patient.Application.PatientManagement.Responses;

namespace EHRPlatform.Services.Patient.Features.Patients.Commands;

/// <summary>
/// Create patient command.
/// Initiates creation of a new patient profile.
/// </summary>
public record CreatePatientCommand : ICommand<PatientResponseDto>
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public DateTime DateOfBirth { get; init; }
    public string Gender { get; init; } = string.Empty;
    public string BloodType { get; init; } = string.Empty;
    public string? EmergencyContact { get; init; }
    public string? EmergencyPhone { get; init; }
}
