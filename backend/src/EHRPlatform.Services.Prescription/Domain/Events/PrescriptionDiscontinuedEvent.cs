using EHRPlatform.BuildingBlocks.Common.Events;

namespace EHRPlatform.Services.Prescription.Domain.Events;

public class PrescriptionDiscontinuedEvent : IntegrationEvent
{
    public Guid PrescriptionId { get; set; }
    public Guid PatientId { get; set; }
    public string MedicationName { get; set; }
    public string Reason { get; set; }

    public PrescriptionDiscontinuedEvent(Guid id, Guid patientId, string med, string reason)
    {
        PrescriptionId = id;
        PatientId = patientId;
        MedicationName = med;
        Reason = reason;
    }
}

