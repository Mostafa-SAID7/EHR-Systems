using EHRPlatform.BuildingBlocks.Common.Events;

namespace EHRPlatform.Services.Clinical.Domain.Events;

public class VitalSignsRecordedEvent : IntegrationEvent
{
    public Guid ClinicalNoteId { get; set; }
    public Guid PatientId { get; set; }
    public int SystolicBP { get; set; }
    public int DiastolicBP { get; set; }
    public int HeartRate { get; set; }

    public VitalSignsRecordedEvent(Guid noteId, Guid patientId, int systolic, int diastolic, int hr)
    {
        ClinicalNoteId = noteId;
        PatientId = patientId;
        SystolicBP = systolic;
        DiastolicBP = diastolic;
        HeartRate = hr;
    }
}
