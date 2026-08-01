namespace EHRPlatform.Services.Appointment.Domain.Entities;

/// <summary>
/// Appointment aggregate root - Scheduling with conflict detection.
/// Status: Scheduled → Confirmed → InProgress → Completed / Cancelled
/// Types: Office, Telehealth, Phone
/// </summary>
public class Appointment
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Guid ProviderId { get; set; }
    public string AppointmentType { get; set; } = "Office"; // Office, Telehealth, Phone
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public string Status { get; set; } = "Scheduled"; // Scheduled, Confirmed, InProgress, Completed, Cancelled, NoShow
    public string ReasonForVisit { get; set; } = string.Empty;
    public string? Notes { get; set; }
    
    // Check-in tracking
    public DateTime? CheckedInAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    
    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Relations
    public ICollection<AppointmentReminder> Reminders { get; } = new List<AppointmentReminder>();
    public ICollection<AppointmentNote> Notes_Collection { get; } = new List<AppointmentNote>();
    public ICollection<RescheduleHistory> RescheduleHistory { get; } = new List<RescheduleHistory>();

    private readonly List<object> _domainEvents = new();

    public int GetDurationMinutes() => (int)(ScheduledEnd - ScheduledStart).TotalMinutes;

    public void Confirm()
    {
        Status = "Confirmed";
        UpdatedAt = DateTime.UtcNow;
        RaiseEvent(new AppointmentConfirmedEvent(Id, PatientId, ProviderId, ScheduledStart));
    }

    public void CheckIn()
    {
        Status = "InProgress";
        CheckedInAt = DateTime.UtcNow;
        StartedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        RaiseEvent(new AppointmentCheckedInEvent(Id, PatientId));
    }

    public void Complete()
    {
        Status = "Completed";
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        RaiseEvent(new AppointmentCompletedEvent(Id, PatientId, ProviderId, GetDurationMinutes()));
    }

    public void Cancel(string reason)
    {
        Status = "Cancelled";
        CancelledAt = DateTime.UtcNow;
        CancellationReason = reason;
        UpdatedAt = DateTime.UtcNow;
        RaiseEvent(new AppointmentCancelledEvent(Id, PatientId, reason));
    }

    public void MarkNoShow()
    {
        Status = "NoShow";
        UpdatedAt = DateTime.UtcNow;
        RaiseEvent(new AppointmentNoShowEvent(Id, PatientId));
    }

    public void Reschedule(DateTime newStart, DateTime newEnd, string reason)
    {
        var history = new RescheduleHistory
        {
            Id = Guid.NewGuid(),
            AppointmentId = Id,
            OriginalStart = ScheduledStart,
            OriginalEnd = ScheduledEnd,
            NewStart = newStart,
            NewEnd = newEnd,
            Reason = reason,
            RescheduledAt = DateTime.UtcNow
        };
        RescheduleHistory.Add(history);

        ScheduledStart = newStart;
        ScheduledEnd = newEnd;
        Status = "Scheduled";
        UpdatedAt = DateTime.UtcNow;
        RaiseEvent(new AppointmentRescheduleEvent(Id, PatientId, newStart, reason));
    }

    public void AddNote(Guid noteId, Guid createdById, string content, string privacyLevel)
    {
        var note = new AppointmentNote
        {
            Id = noteId,
            AppointmentId = Id,
            CreatedById = createdById,
            Content = content,
            PrivacyLevel = privacyLevel,
            CreatedAt = DateTime.UtcNow
        };
        Notes_Collection.Add(note);
    }

    public void ScheduleReminder(string method, int minutesBefore)
    {
        var reminderTime = ScheduledStart.AddMinutes(-minutesBefore);
        var reminder = new AppointmentReminder
        {
            Id = Guid.NewGuid(),
            AppointmentId = Id,
            ReminderMethod = method, // Email, SMS, Push, InApp
            MinutesBefore = minutesBefore,
            ScheduledReminderTime = reminderTime,
            Status = "Scheduled",
            CreatedAt = DateTime.UtcNow
        };
        Reminders.Add(reminder);
    }

    public void RaiseEvent(object @event) => _domainEvents.Add(@event);
    public IReadOnlyList<object> GetDomainEvents() => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();
}

/// <summary>
/// AppointmentReminder - Multi-channel reminder tracking
/// </summary>
public class AppointmentReminder
{
    public Guid Id { get; set; }
    public Guid AppointmentId { get; set; }
    public string ReminderMethod { get; set; } = string.Empty; // Email, SMS, Push, InApp
    public int MinutesBefore { get; set; }
    public DateTime ScheduledReminderTime { get; set; }
    public string Status { get; set; } = "Scheduled"; // Scheduled, Sent, Failed, Skipped
    public int RetryCount { get; set; }
    public DateTime? SentAt { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CreatedAt { get; set; }

    public Appointment Appointment { get; set; } = null!;
}

/// <summary>
/// AppointmentNote - Notes attached to appointment
/// </summary>
public class AppointmentNote
{
    public Guid Id { get; set; }
    public Guid AppointmentId { get; set; }
    public Guid CreatedById { get; set; }
    public string Content { get; set; } = string.Empty;
    public string PrivacyLevel { get; set; } = "Internal"; // Private, Shared, Internal
    public DateTime CreatedAt { get; set; }

    public Appointment Appointment { get; set; } = null!;
}

/// <summary>
/// RescheduleHistory - Track reschedule events
/// </summary>
public class RescheduleHistory
{
    public Guid Id { get; set; }
    public Guid AppointmentId { get; set; }
    public DateTime OriginalStart { get; set; }
    public DateTime OriginalEnd { get; set; }
    public DateTime NewStart { get; set; }
    public DateTime NewEnd { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime RescheduledAt { get; set; }

    public Appointment Appointment { get; set; } = null!;
}

/// <summary>
/// ProviderAvailability - Working hours and breaks
/// </summary>
public class ProviderAvailability
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int SlotDurationMinutes { get; set; } = 30;
    public bool IsAvailable { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

// Domain Events
public record AppointmentScheduledEvent(Guid AppointmentId, Guid PatientId, Guid ProviderId, DateTime ScheduledStart)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}

public record AppointmentConfirmedEvent(Guid AppointmentId, Guid PatientId, Guid ProviderId, DateTime ScheduledStart)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}

public record AppointmentCheckedInEvent(Guid AppointmentId, Guid PatientId)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}

public record AppointmentCompletedEvent(Guid AppointmentId, Guid PatientId, Guid ProviderId, int DurationMinutes)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}

public record AppointmentCancelledEvent(Guid AppointmentId, Guid PatientId, string Reason)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}

public record AppointmentNoShowEvent(Guid AppointmentId, Guid PatientId)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}

public record AppointmentRescheduleEvent(Guid AppointmentId, Guid PatientId, DateTime NewStart, string Reason)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}

public record ReminderScheduledEvent(Guid ReminderId, Guid AppointmentId, string Method, DateTime ReminderTime)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}
