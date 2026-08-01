using System;

namespace EHRPlatform.Common.Shared.DTOs
{
    /// <summary>
    /// Shared DTO for Inter-Service Patient Communication
    /// Used ONLY for inter-service events and API calls.
    /// NOT mapped to Patient Service's internal Patient entity.
    /// Services receive this via:
    /// 1. Kafka events (PatientCreated, PatientUpdated, etc.)
    /// 2. REST API calls from Patient Service
    /// </summary>
    public class PatientDto
    {
        public Guid Id { get; set; }
        public string MedicalRecordNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }

        /// <summary>Full name (convenience property)</summary>
        public string FullName => $"{FirstName} {LastName}";

        /// <summary>Calculate patient's current age</summary>
        public int GetAge()
        {
            var today = DateTime.Today;
            var age = today.Year - DateOfBirth.Year;
            if (DateOfBirth.Date > today.AddYears(-age))
                age--;
            return age;
        }
    }

    /// <summary>
    /// Event: Patient Created
    /// Published by Patient Service when a new patient is registered
    /// Subscribed by: Appointment, Clinical, Billing, Notification services
    /// </summary>
    public class PatientCreatedEvent
    {
        public Guid PatientId { get; set; }
        public PatientDto PatientData { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Event: Patient Updated
    /// Published by Patient Service when patient demographics change
    /// Subscribed by: Services that cache patient information
    /// </summary>
    public class PatientUpdatedEvent
    {
        public Guid PatientId { get; set; }
        public PatientDto PatientData { get; set; }
        public Guid UpdatedByUserId { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Event: Patient Archived/Deleted
    /// Published by Patient Service when a patient is archived
    /// Subscribed by: All services (to stop processing for this patient)
    /// </summary>
    public class PatientArchivedEvent
    {
        public Guid PatientId { get; set; }
        public string Reason { get; set; }
        public Guid ArchivedByUserId { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Event: Patient Allergy Added
    /// Published by Patient Service when an allergy is recorded
    /// Subscribed by: Clinical, Prescription services
    /// </summary>
    public class PatientAllergyAddedEvent
    {
        public Guid PatientId { get; set; }
        public string AllergenName { get; set; }
        public string Severity { get; set; }
        public string Reaction { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Event: Patient Condition Added
    /// Published by Patient Service when a diagnosis is recorded
    /// Subscribed by: Clinical, Analytics services
    /// </summary>
    public class PatientConditionAddedEvent
    {
        public Guid PatientId { get; set; }
        public string ConditionName { get; set; }
        public string ICD10Code { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Event: Patient Status Changed
    /// Published by Patient Service when patient status changes (Active, Inactive, etc.)
    /// Subscribed by: All services
    /// </summary>
    public class PatientStatusChangedEvent
    {
        public Guid PatientId { get; set; }
        public string OldStatus { get; set; }
        public string NewStatus { get; set; }
        public string Reason { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }
}
