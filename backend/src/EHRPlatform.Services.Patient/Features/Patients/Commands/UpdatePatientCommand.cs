using EHRPlatform.Common.CQRS;
using EHRPlatform.Services.Patient.Application.PatientManagement.Responses;

namespace EHRPlatform.Services.Patient.Features.Patients.Commands;

/// <summary>
/// Update patient command.
/// Updates an existing patient's profile information.
/// </summary>
public record UpdatePatientCommand : ICommand<PatientResponseDto>
{
    public Guid PatientId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string? Gender { get; init; }
    public string BloodType { get; init; } = string.Empty;
    public string? EmergencyContact { get; init; }
    public string? EmergencyPhone { get; init; }
}
