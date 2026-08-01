using EHRPlatform.BuildingBlocks.Common.Events;

namespace EHRPlatform.Services.Patient.Domain.Events;

/// <summary>
/// Patient allergy added event.
/// </summary>
public class PatientAllergyAddedEvent : IntegrationEvent
{
    public Guid PatientId { get; set; }
    public string Allergen { get; set; }
    public string Severity { get; set; }

    public PatientAllergyAddedEvent(Guid patientId, string allergen, string severity)
    {
        PatientId = patientId;
        Allergen = allergen;
        Severity = severity;
    }
}

