using EHRPlatform.BuildingBlocks.Common.Events;
using EHRPlatform.Services.Appointment.Domain.Enums;

namespace EHRPlatform.Services.Appointment.Domain.Events;

/// <summary>
/// Domain event raised when an appointment reminder is sent.
/// </summary>
public class AppointmentReminderSentEvent : IntegrationEvent
{
    /// <summary>
    /// Gets the appointment identifier.
    /// </summary>
    public Guid AppointmentId { get; set; }

    /// <summary>
    /// Gets the reminder identifier.
    /// </summary>
    public Guid ReminderId { get; set; }

    /// <summary>
    /// Gets the patient identifier.
    /// </summary>
    public Guid PatientId { get; set; }

    /// <summary>
    /// Gets the reminder method (Email, SMS, InApp, Push).
    /// </summary>
    public ReminderType Method { get; set; }

    /// <summary>
    /// Gets the time the reminder was sent.
    /// </summary>
    public DateTime SentAt { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AppointmentReminderSentEvent"/> class.
    /// </summary>
    public AppointmentReminderSentEvent(
        Guid appointmentId,
        Guid reminderId,
        Guid patientId,
        ReminderType method,
        DateTime sentAt)
    {
        AppointmentId = appointmentId;
        ReminderId = reminderId;
        PatientId = patientId;
        Method = method;
        SentAt = sentAt;
    }
}

