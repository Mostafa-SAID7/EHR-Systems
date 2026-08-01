using EHRPlatform.BuildingBlocks.Common.Events;

namespace EHRPlatform.Services.Prescription.Domain.Events;

public class PrescriptionRefillRequestedEvent : IntegrationEvent
{
    public Guid PrescriptionId { get; set; }
    public Guid PatientId { get; set; }
    public Guid ProviderId { get; set; }
    public string MedicationName { get; set; }

    public PrescriptionRefillRequestedEvent(Guid id, Guid patientId, Guid providerId, string med)
    {
        PrescriptionId = id;
        PatientId = patientId;
        ProviderId = providerId;
        MedicationName = med;
    }
}

