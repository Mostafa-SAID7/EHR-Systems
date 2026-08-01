namespace EHRPlatform.Services.Patient.Application.Features.Patients.Commands;

using MediatR;

/// <summary>
/// Command to create new patient record.
/// Auto-generates MRN (Medical Record Number).
/// </summary>
public class CreatePatientCommand : IRequest<CreatePatientResponse>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    
    // Address
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    
    // Emergency contact
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelationship { get; set; }
    
    public string? BloodType { get; set; }
    public string PreferredContactMethod { get; set; } = "Email";
}

public class CreatePatientResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public Guid? PatientId { get; set; }
    public string? Mrn { get; set; }
}
