using EHRPlatform.BuildingBlocks.Common.Events;

namespace EHRPlatform.Services.Clinical.Domain.Events;

public class ClinicalNoteCompletedEvent : IntegrationEvent
{
    public Guid ClinicalNoteId { get; set; }
    public Guid PatientId { get; set; }
    public DateTime EncounterDate { get; set; }

    public ClinicalNoteCompletedEvent(Guid noteId, Guid patientId, DateTime encounterDate)
    {
        ClinicalNoteId = noteId;
        PatientId = patientId;
        EncounterDate = encounterDate;
    }
}
