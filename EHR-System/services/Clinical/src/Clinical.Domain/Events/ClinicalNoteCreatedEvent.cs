using EHRPlatform.BuildingBlocks.Common.Events;

namespace EHRPlatform.Services.Clinical.Domain.Events;

/// <summary>
/// Event raised when a new clinical note is created.
/// </summary>
public class ClinicalNoteCreatedEvent : IntegrationEvent
{
    public Guid ClinicalNoteId { get; }
    public Guid PatientId { get; }
    public Guid ProviderId { get; }
    public DateTime EncounterDate { get; }
    public string Status { get; } = "Draft";

    public ClinicalNoteCreatedEvent(Guid clinicalNoteId, Guid patientId, Guid providerId, DateTime encounterDate)
    {
        ClinicalNoteId = clinicalNoteId;
        PatientId = patientId;
        ProviderId = providerId;
        EncounterDate = encounterDate;
    }

    public ClinicalNoteCreatedEvent(Guid id, Guid patientId, Guid providerId, DateTime encounterDate, string status)
    {
        ClinicalNoteId = id;
        PatientId = patientId;
        ProviderId = providerId;
        EncounterDate = encounterDate;
        Status = status;
    }
}
