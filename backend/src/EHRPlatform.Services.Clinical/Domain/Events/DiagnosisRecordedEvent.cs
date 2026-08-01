using EHRPlatform.BuildingBlocks.Common.Events;

namespace EHRPlatform.Services.Clinical.Domain.Events;

public class DiagnosisRecordedEvent : IntegrationEvent
{
    public Guid ClinicalNoteId { get; set; }
    public Guid PatientId { get; set; }
    public string DiagnosisCode { get; set; }
    public string DiagnosisText { get; set; }

    public DiagnosisRecordedEvent(Guid noteId, Guid patientId, string code, string text)
    {
        ClinicalNoteId = noteId;
        PatientId = patientId;
        DiagnosisCode = code;
        DiagnosisText = text;
    }
}

