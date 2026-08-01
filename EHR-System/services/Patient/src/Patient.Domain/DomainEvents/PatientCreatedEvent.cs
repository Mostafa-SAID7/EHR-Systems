using EHRPlatform.BuildingBlocks.Common.Events;

namespace EHRPlatform.Services.Patient.Domain.DomainEvents;

/// <summary>
/// Patient created domain event.
/// </summary>
public class PatientCreatedEvent : IntegrationEvent
{
    public Guid PatientId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string MRN { get; set; }

    public PatientCreatedEvent(Guid patientId, string firstName, string lastName, string email, string mrn)
    {
        PatientId = patientId;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        MRN = mrn;
    }
}


