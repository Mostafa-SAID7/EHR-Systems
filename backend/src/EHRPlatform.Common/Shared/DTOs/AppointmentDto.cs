using System;

namespace EHRPlatform.Common.Shared.DTOs
{
    /// <summary>
    /// Shared DTO for Appointment Communication
    /// Used for inter-service events
    /// </summary>
    public class AppointmentDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public Guid ProviderId { get; set; }
        public string ProviderName { get; set; }
        public string Status { get; set; }  // e.g., "Scheduled", "Confirmed", "Completed", "Cancelled"
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string AppointmentType { get; set; }  // e.g., "Initial Consultation", "Follow-up", "Procedure"
        public string Location { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Event: Appointment Scheduled
    /// Published by Appointment Service when appointment is booked
    /// Subscribed by: Notification (send confirmation), Analytics, Audit
    /// </summary>
    public class AppointmentScheduledEvent
    {
        public Guid AppointmentId { get; set; }
        public Guid PatientId { get; set; }
        public Guid ProviderId { get; set; }
        public DateTime AppointmentTime { get; set; }
        public string AppointmentType { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Event: Appointment Confirmed
    /// Published by Appointment Service when appointment is confirmed
    /// Subscribed by: Notification (send confirmation to patient), Clinical Service (prepare materials)
    /// </summary>
    public class AppointmentConfirmedEvent
    {
        public Guid AppointmentId { get; set; }
        public Guid PatientId { get; set; }
        public DateTime AppointmentTime { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Event: Appointment Cancelled
    /// Published by Appointment Service when appointment is cancelled
    /// Subscribed by: Notification (notify patient), Clinical Service (free up provider slot), Billing (no charge)
    /// </summary>
    public class AppointmentCancelledEvent
    {
        public Guid AppointmentId { get; set; }
        public Guid PatientId { get; set; }
        public string CancellationReason { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Event: Appointment Completed
    /// Published by Appointment Service when appointment is marked complete
    /// Subscribed by: Billing (trigger billing), Clinical Service (receive clinical notes)
    /// </summary>
    public class AppointmentCompletedEvent
    {
        public Guid AppointmentId { get; set; }
        public Guid PatientId { get; set; }
        public Guid ProviderId { get; set; }
        public DateTime AppointmentTime { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Event: Appointment Rescheduled
    /// Published by Appointment Service when appointment is moved to different time
    /// Subscribed by: Notification (send new appointment details)
    /// </summary>
    public class AppointmentRescheduledEvent
    {
        public Guid AppointmentId { get; set; }
        public Guid PatientId { get; set; }
        public DateTime OldAppointmentTime { get; set; }
        public DateTime NewAppointmentTime { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }
}
