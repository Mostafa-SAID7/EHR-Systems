namespace EHRPlatform.Services.Patient.Application.Features.Patients.Commands;

using MediatR;

/// <summary>
/// Command to update patient information.
/// </summary>
public class UpdatePatientCommand : IRequest<UpdatePatientResponse>
{
    public Guid PatientId { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; }
    public string? PreferredContactMethod { get; set; }
    public string? BloodType { get; set; }
}

public class UpdatePatientResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}
