using EHRPlatform.Common.Events;

namespace EHRPlatform.Services.Patient.Domain.Events;

/// <summary>
/// Raised when a patient is registered through the RegisterPatientCommand.
/// </summary>
public class PatientRegisteredEvent : IntegrationEvent
{
    public Guid PatientId { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public string Email { get; }
    public string MRN { get; }

    public PatientRegisteredEvent(Guid patientId, string firstName, string lastName, string email, string mrn)
    {
        PatientId = patientId;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        MRN = mrn;
    }
}
