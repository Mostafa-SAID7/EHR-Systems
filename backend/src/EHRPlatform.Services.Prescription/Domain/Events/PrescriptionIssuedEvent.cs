using EHRPlatform.BuildingBlocks.Common.Events;

namespace EHRPlatform.Services.Prescription.Domain.Events;

public class PrescriptionIssuedEvent : IntegrationEvent
{
    public Guid PrescriptionId { get; set; }
    public Guid PatientId { get; set; }
    public Guid ProviderId { get; set; }
    public string MedicationName { get; set; }
    public string Dosage { get; set; }

    public PrescriptionIssuedEvent(Guid id, Guid patientId, Guid providerId, string med, string dosage)
    {
        PrescriptionId = id;
        PatientId = patientId;
        ProviderId = providerId;
        MedicationName = med;
        Dosage = dosage;
    }
}

