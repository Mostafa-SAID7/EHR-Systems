namespace EHRPlatform.Services.Patient.Application.Features.Patients.Queries;

using MediatR;

/// <summary>
/// Query to get patient by ID (cached 10 minutes).
/// </summary>
public class GetPatientQuery : IRequest<GetPatientResponse>
{
    public Guid PatientId { get; set; }
}

public class GetPatientResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public PatientDto? Patient { get; set; }
}

public class PatientDto
{
    public Guid Id { get; set; }
    public string Mrn { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public int Age { get; set; }
    public string? BloodType { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; }
}
