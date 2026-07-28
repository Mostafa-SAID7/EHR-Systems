using EHRPlatform.Common.Events;

namespace EHRPlatform.Services.Appointment.Domain.Events;

/// <summary>
/// Domain event raised when a note is added to an appointment.
/// </summary>
public class AppointmentNoteAddedEvent : IntegrationEvent
{
    /// <summary>
    /// Gets the appointment identifier.
    /// </summary>
    public Guid AppointmentId { get; }

    /// <summary>
    /// Gets the user ID who added the note.
    /// </summary>
    public Guid AddedById { get; }

    /// <summary>
    /// Gets the note content.
    /// </summary>
    public string Content { get; }

    public AppointmentNoteAddedEvent(Guid appointmentId, Guid addedById, string content)
    {
        AppointmentId = appointmentId;
        AddedById = addedById;
        Content = content;
    }
}
