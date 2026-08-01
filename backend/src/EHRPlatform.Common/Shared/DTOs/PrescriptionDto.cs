using System;

namespace EHRPlatform.Common.Shared.DTOs
{
    /// <summary>
    /// Shared DTO for Prescription Communication
    /// </summary>
    public class PrescriptionDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public Guid ProviderId { get; set; }
        public string MedicationName { get; set; }
        public string Dosage { get; set; }
        public string Frequency { get; set; }
        public int Quantity { get; set; }
        public int RefillsRemaining { get; set; }
        public string Status { get; set; }  // e.g., "Active", "Completed", "Cancelled", "Expired"
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Instructions { get; set; }
    }

    /// <summary>
    /// Event: Prescription Created
    /// Published by Prescription Service when provider creates prescription
    /// Subscribed by: Notification (send to pharmacy/patient), Audit, Analytics
    /// </summary>
    public class PrescriptionCreatedEvent
    {
        public Guid PrescriptionId { get; set; }
        public Guid PatientId { get; set; }
        public Guid ProviderId { get; set; }
        public string MedicationName { get; set; }
        public string Dosage { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Event: Prescription Filled
    /// Published by Prescription Service when prescription is filled by pharmacy
    /// Subscribed by: Notification (notify patient), Patient Service (update medication history)
    /// </summary>
    public class PrescriptionFilledEvent
    {
        public Guid PrescriptionId { get; set; }
        public Guid PatientId { get; set; }
        public string MedicationName { get; set; }
        public DateTime FilledDate { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Event: Prescription Refill Requested
    /// Published by Prescription Service when patient/pharmacy requests refill
    /// Subscribed by: Notification (notify provider), Audit
    /// </summary>
    public class PrescriptionRefillRequestedEvent
    {
        public Guid PrescriptionId { get; set; }
        public Guid PatientId { get; set; }
        public Guid ProviderId { get; set; }
        public string MedicationName { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Event: Prescription Refill Approved
    /// Published by Prescription Service when provider approves refill
    /// Subscribed by: Notification (notify pharmacy/patient), Audit
    /// </summary>
    public class PrescriptionRefillApprovedEvent
    {
        public Guid PrescriptionId { get; set; }
        public Guid PatientId { get; set; }
        public string MedicationName { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Event: Prescription Cancelled
    /// Published by Prescription Service when prescription is cancelled
    /// Subscribed by: Notification (notify patient/pharmacy), Audit
    /// </summary>
    public class PrescriptionCancelledEvent
    {
        public Guid PrescriptionId { get; set; }
        public Guid PatientId { get; set; }
        public string MedicationName { get; set; }
        public string CancellationReason { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }
}
