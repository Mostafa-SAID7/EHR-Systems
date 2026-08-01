using EHRPlatform.BuildingBlocks.Common.Events;

namespace EHRPlatform.Services.Prescription.Domain.Events;

public class PrescriptionRefillApprovedEvent : IntegrationEvent
{
    public Guid PrescriptionId { get; set; }
    public Guid PatientId { get; set; }
    public string MedicationName { get; set; }

    public PrescriptionRefillApprovedEvent(Guid id, Guid patientId, string med)
    {
        PrescriptionId = id;
        PatientId = patientId;
        MedicationName = med;
    }
}

