using EHRPlatform.BuildingBlocks.Common.Events;

namespace EHRPlatform.Services.Clinical.Domain.Events;

public class ProcedurePerformedEvent : IntegrationEvent
{
    public Guid ClinicalNoteId { get; set; }
    public Guid PatientId { get; set; }
    public string ProcedureName { get; set; }
    public string ProcedureCode { get; set; }

    public ProcedurePerformedEvent(Guid noteId, Guid patientId, string name, string code)
    {
        ClinicalNoteId = noteId;
        PatientId = patientId;
        ProcedureName = name;
        ProcedureCode = code;
    }
}

