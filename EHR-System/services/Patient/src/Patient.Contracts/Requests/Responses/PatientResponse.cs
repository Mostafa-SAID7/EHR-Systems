namespace EHRPlatform.Services.Patient.Application.Patients.Responses;

/// <summary>
/// Patient response DTO with nested allergies and conditions.
/// </summary>
public class PatientResponse
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string MRN { get; set; } = string.Empty;
    public string BloodType { get; set; } = string.Empty;
    public string? EmergencyContact { get; set; }
    public string? EmergencyPhone { get; set; }
    public string Status { get; set; } = string.Empty;

    // Nested objects
    public List<PatientAllergyDto> Allergies { get; set; } = new();
    public List<PatientConditionDto> Conditions { get; set; } = new();

    // Audit fields
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
