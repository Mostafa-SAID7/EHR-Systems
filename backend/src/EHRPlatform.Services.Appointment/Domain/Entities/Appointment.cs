using EHRPlatform.Common.Domain.Entities;
using EHRPlatform.Services.Appointment.Domain.Enums;
using EHRPlatform.Common.Events;

namespace EHRPlatform.Services.Appointment.Features.Appointments.Domain;

/// <summary>
/// Appointment aggregate root.
/// Manages scheduling, availability, reminders, and cancellations.
/// </summary>
public class Appointment : AuditableEntity
{
    /// <summary>
    /// Gets or sets the patient identifier.
    /// </summary>
    public Guid PatientId { get; set; }

    /// <summary>
    /// Gets or sets the provider identifier.
    /// </summary>
    public Guid ProviderId { get; set; }

    /// <summary>
    /// Gets or sets the scheduled start time.
    /// </summary>
    public DateTime ScheduledStart { get; set; }

    /// <summary>
    /// Gets or sets the scheduled end time.
    /// </summary>
    public DateTime ScheduledEnd { get; set; }

    /// <summary>
    /// Gets or sets the appointment type (Office, Telehealth, Phone).
    /// </summary>
    public AppointmentType AppointmentType { get; set; }

    /// <summary>
    /// Gets or sets the current appointment status.
    /// </summary>
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;

    /// <summary>
    /// Gets or sets the reason for visit.
    /// </summary>
    public string? ReasonForVisit { get; set; }

    /// <summary>
    /// Gets or sets additional notes about the appointment.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Gets or sets the duration of the appointment in minutes.
    /// </summary>
    public int DurationMinutes { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a reminder has been sent.
    /// </summary>
    public bool ReminderSent { get; set; }

    /// <summary>
    /// Gets or sets the date and time the appointment was confirmed.
    /// </summary>
    public DateTime? ConfirmedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time the appointment was cancelled.
    /// </summary>
    public DateTime? CancelledAt { get; set; }

    /// <summary>
    /// Gets or sets the cancellation reason.
    /// </summary>
    public CancellationReason? CancellationReason { get; set; }

    /// <summary>
    /// Gets the collection of reminders for this appointment.
    /// </summary>
    public ICollection<AppointmentReminder> Reminders { get; } = new List<AppointmentReminder>();

    /// <summary>
    /// Gets the collection of notes for this appointment.
    /// </summary>
    public ICollection<AppointmentNote> AppointmentNotes { get; } = new List<AppointmentNote>();

    /// <summary>
    /// Gets the collection of reschedule history for this appointment.
    /// </summary>
    public ICollection<RescheduleHistory> RescheduleHistory { get; } = new List<RescheduleHistory>();

    private readonly List<IntegrationEvent> _domainEvents = new();

    /// <summary>
    /// Gets a value indicating whether the appointment is available (scheduled and in the future).
    /// </summary>
    public bool IsAvailable => Status == AppointmentStatus.Scheduled && ScheduledStart > DateTime.UtcNow;

    /// <summary>
    /// Confirms the appointment.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if appointment is not scheduled.</exception>
    public void Confirm()
    {
        if (Status != AppointmentStatus.Scheduled)
            throw new InvalidOperationException("Only scheduled appointments can be confirmed");

        Status = AppointmentStatus.Confirmed;
        ConfirmedAt = DateTime.UtcNow;
        RaiseEvent(new AppointmentConfirmedEvent(Id, PatientId, ProviderId, ScheduledStart));
    }

    /// <summary>
    /// Cancels the appointment.
    /// </summary>
    /// <param name="reason">Reason for cancellation.</param>
    /// <exception cref="InvalidOperationException">Thrown if appointment is completed or already cancelled.</exception>
    public void Cancel(CancellationReason reason)
    {
        if (Status == AppointmentStatus.Completed || Status == AppointmentStatus.Cancelled)
            throw new InvalidOperationException($"Cannot cancel {Status} appointment");

        Status = AppointmentStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        CancellationReason = reason;
        RaiseEvent(new AppointmentCancelledEvent(Id, PatientId, ProviderId, reason.ToString()));
    }

    /// <summary>
    /// Marks the appointment as checked in.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if appointment is not confirmed.</exception>
    public void CheckIn()
    {
        if (Status != AppointmentStatus.Confirmed)
            throw new InvalidOperationException("Only confirmed appointments can be checked in");

        Status = AppointmentStatus.InProgress;
        RaiseEvent(new AppointmentCheckedInEvent(Id, PatientId, ProviderId, DateTime.UtcNow));
    }

    /// <summary>
    /// Marks the appointment as completed.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if appointment is not checked in.</exception>
    public void Complete()
    {
        if (Status != AppointmentStatus.InProgress)
            throw new InvalidOperationException("Only checked-in appointments can be completed");

        Status = AppointmentStatus.Completed;
        RaiseEvent(new AppointmentCompletedEvent(Id, PatientId, ProviderId, DateTime.UtcNow));
    }

    /// <summary>
    /// Adds a reminder for this appointment.
    /// </summary>
    /// <param name="reminderTime">The time for the reminder.</param>
    /// <param name="method">The reminder method.</param>
    public void AddReminder(DateTime reminderTime, ReminderType method = ReminderType.Email)
    {
        var reminder = new AppointmentReminder
        {
            Id = Guid.NewGuid(),
            AppointmentId = Id,
            ReminderTime = reminderTime,
            Method = method,
            Status = ReminderStatus.Scheduled,
            IsSent = false
        };
        Reminders.Add(reminder);
    }

    /// <summary>
    /// Marks a reminder as sent.
    /// </summary>
    /// <param name="reminderId">The reminder identifier.</param>
    public void MarkReminderSent(Guid reminderId)
    {
        var reminder = Reminders.FirstOrDefault(r => r.Id == reminderId);
        if (reminder != null)
        {
            reminder.Status = ReminderStatus.Sent;
            reminder.IsSent = true;
        }
    }

    /// <summary>
    /// Adds a note to this appointment.
    /// </summary>
    /// <param name="content">Note content.</param>
    /// <param name="createdById">User ID of note creator.</param>
    /// <param name="privacyLevel">Privacy level for the note.</param>
    /// <param name="category">Note category (optional).</param>
    public void AddNote(string content, Guid createdById, NotePrivacyLevel privacyLevel = NotePrivacyLevel.InternalOnly, string? category = null)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Note content cannot be empty");

        var note = new AppointmentNote
        {
            Id = Guid.NewGuid(),
            AppointmentId = Id,
            CreatedById = createdById,
            Content = content,
            PrivacyLevel = privacyLevel,
            Category = category,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Notes.Add(note);
        RaiseEvent(new AppointmentNoteAddedEvent(Id, createdById, content));
    }

    /// <summary>
    /// Reschedules this appointment to a new time.
    /// </summary>
    /// <param name="newScheduledStart">New start time.</param>
    /// <param name="durationMinutes">Duration in minutes.</param>
    /// <param name="initiatedById">User ID who initiated reschedule.</param>
    /// <param name="initiatedBy">Who initiated (Patient/Provider/Admin).</param>
    /// <param name="reason">Reason for reschedule (optional).</param>
    public void Reschedule(DateTime newScheduledStart, int durationMinutes, Guid initiatedById, string initiatedBy = "Provider", string? reason = null)
    {
        if (newScheduledStart <= DateTime.UtcNow)
            throw new InvalidOperationException("New appointment time must be in the future");

        if (Status == AppointmentStatus.Completed || Status == AppointmentStatus.Cancelled)
            throw new InvalidOperationException($"Cannot reschedule {Status} appointment");

        var oldStart = ScheduledStart;
        
        // Record history
        var history = new RescheduleHistory
        {
            Id = Guid.NewGuid(),
            AppointmentId = Id,
            OriginalScheduledStart = oldStart,
            NewScheduledStart = newScheduledStart,
            InitiatedBy = initiatedBy,
            InitiatedByUserId = initiatedById,
            Reason = reason,
            RescheduleDateTime = DateTime.UtcNow
        };

        RescheduleHistory.Add(history);

        // Update appointment
        ScheduledStart = newScheduledStart;
        ScheduledEnd = newScheduledStart.AddMinutes(durationMinutes);
        Status = AppointmentStatus.Rescheduled;

        // Raise event
        RaiseEvent(new AppointmentRescheduledEvent(
            Id, PatientId, ProviderId, oldStart, newScheduledStart, reason));
    }

    /// <summary>
    /// Raises a domain event.
    /// </summary>
    /// <param name="event">The domain event to raise.</param>
    public void RaiseEvent(IntegrationEvent @event) => _domainEvents.Add(@event);

    /// <summary>
    /// Gets all raised domain events.
    /// </summary>
    /// <returns>Read-only list of domain events.</returns>
    public IReadOnlyList<IntegrationEvent> GetDomainEvents() => _domainEvents.AsReadOnly();

    /// <summary>
    /// Clears all raised domain events.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}

