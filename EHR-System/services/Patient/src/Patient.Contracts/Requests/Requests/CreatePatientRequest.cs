namespace EHRPlatform.Services.Patient.Application.Patients.Requests;

public class CreatePatientRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? MRN { get; set; }
}
