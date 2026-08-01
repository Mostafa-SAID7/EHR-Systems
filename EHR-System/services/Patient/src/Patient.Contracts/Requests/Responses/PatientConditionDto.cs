namespace EHRPlatform.Services.Patient.Application.Patients.Responses;

/// <summary>
/// Patient condition nested DTO.
/// </summary>
public class PatientConditionDto
{
    public Guid Id { get; set; }
    public string Condition { get; set; } = string.Empty;
    public string ICD10Code { get; set; } = string.Empty;
    public DateTime? OnsetDate { get; set; }
}
