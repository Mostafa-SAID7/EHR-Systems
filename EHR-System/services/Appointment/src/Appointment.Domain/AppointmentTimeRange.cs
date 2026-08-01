using EHRPlatform.Services.Appointment.Domain.Enums;

namespace EHRPlatform.Services.Appointment.Domain.ValueObjects;

/// <summary>
/// Value object representing appointment scheduled times with validation.
/// Encapsulates the scheduled start and end times of an appointment.
/// </summary>
public class AppointmentTimeRange : IEquatable<AppointmentTimeRange>
{
    /// <summary>
    /// Gets the scheduled start time.
    /// </summary>
    public DateTime ScheduledStart { get; }

    /// <summary>
    /// Gets the scheduled end time.
    /// </summary>
    public DateTime ScheduledEnd { get; }

    /// <summary>
    /// Gets the duration in minutes.
    /// </summary>
    public int DurationMinutes { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AppointmentTimeRange"/> class.
    /// </summary>
    /// <param name="scheduledStart">The scheduled start time.</param>
    /// <param name="scheduledEnd">The scheduled end time.</param>
    /// <exception cref="ArgumentException">Thrown when times are invalid.</exception>
    public AppointmentTimeRange(DateTime scheduledStart, DateTime scheduledEnd)
    {
        if (scheduledStart >= scheduledEnd)
            throw new ArgumentException("Scheduled start must be before scheduled end", nameof(scheduledStart));

        if (scheduledStart < DateTime.UtcNow)
            throw new ArgumentException("Appointment cannot be scheduled in the past", nameof(scheduledStart));

        ScheduledStart = scheduledStart;
        ScheduledEnd = scheduledEnd;
        DurationMinutes = (int)(scheduledEnd - scheduledStart).TotalMinutes;
    }

    /// <summary>
    /// Determines whether the appointment is currently scheduled (in the future).
    /// </summary>
    /// <returns>True if the appointment is in the future; otherwise false.</returns>
    public bool IsScheduled()
    {
        return ScheduledStart > DateTime.UtcNow;
    }

    /// <summary>
    /// Determines whether the appointment is currently in progress.
    /// </summary>
    /// <returns>True if current time is within the appointment range; otherwise false.</returns>
    public bool IsInProgress()
    {
        var now = DateTime.UtcNow;
        return now >= ScheduledStart && now <= ScheduledEnd;
    }

    /// <summary>
    /// Determines whether the appointment has passed.
    /// </summary>
    /// <returns>True if the appointment end time is in the past; otherwise false.</returns>
    public bool HasPassed()
    {
        return ScheduledEnd < DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the minutes until the appointment starts.
    /// </summary>
    /// <returns>Minutes until start, or negative if already started.</returns>
    public int GetMinutesUntilStart()
    {
        return (int)(ScheduledStart - DateTime.UtcNow).TotalMinutes;
    }

    /// <summary>
    /// Determines whether a reminder should be sent based on minutes before appointment.
    /// </summary>
    /// <param name="minutesBefore">Minutes before appointment to send reminder.</param>
    /// <returns>True if the current time is within the reminder window; otherwise false.</returns>
    public bool ShouldSendReminder(int minutesBefore)
    {
        var now = DateTime.UtcNow;
        var reminderTime = ScheduledStart.AddMinutes(-minutesBefore);
        return now >= reminderTime && now < ScheduledStart;
    }

    public bool Equals(AppointmentTimeRange? other)
    {
        return other != null && ScheduledStart == other.ScheduledStart && ScheduledEnd == other.ScheduledEnd;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as AppointmentTimeRange);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ScheduledStart, ScheduledEnd);
    }

    public override string ToString()
    {
        return $"{ScheduledStart:yyyy-MM-dd HH:mm:ss} to {ScheduledEnd:yyyy-MM-dd HH:mm:ss} ({DurationMinutes} min)";
    }
}
