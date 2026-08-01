using EHRPlatform.BuildingBlocks.Common.Events;

namespace EHRPlatform.Services.Patient.Domain.Events;

/// <summary>
/// Patient condition added event.
/// </summary>
public class PatientConditionAddedEvent : IntegrationEvent
{
    public Guid PatientId { get; set; }
    public string Condition { get; set; }
    public string ICD10Code { get; set; }

    public PatientConditionAddedEvent(Guid patientId, string condition, string icd10Code)
    {
        PatientId = patientId;
        Condition = condition;
        ICD10Code = icd10Code;
    }
}

