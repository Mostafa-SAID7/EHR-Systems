using EHRPlatform.Tests.Common.Helpers;
using System;
using System.Collections.Generic;

namespace EHRPlatform.Tests.Common.Builders;

/// <summary>
/// Fluent builder for Appointment test entities.
/// Creates realistic appointment data for integration and contract tests.
/// </summary>
public class AppointmentBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _patientId = Guid.NewGuid();
    private Guid _providerId = Guid.NewGuid();
    private DateTime _scheduledStart = DateTime.UtcNow.AddDays(7);
    private DateTime _scheduledEnd = DateTime.UtcNow.AddDays(7).AddMinutes(30);
    private string _appointmentType = "Office";
    private string _status = "Scheduled";
    private string? _reasonForVisit = "Annual Checkup";
    private string? _notes;
    private int _durationMinutes = 30;
    private bool _reminderSent = false;
    private DateTime? _confirmedAt;
    private DateTime? _cancelledAt;
    private string? _cancellationReason;
    private readonly List<string> _reminders = new();
    private DateTime? _createdAt = DateTime.UtcNow;
    private DateTime? _updatedAt = DateTime.UtcNow;

    /// <summary>
    /// Sets the appointment ID.
    /// </summary>
    public AppointmentBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the patient ID.
    /// </summary>
    public AppointmentBuilder WithPatientId(Guid patientId)
    {
        _patientId = patientId;
        return this;
    }

    /// <summary>
    /// Sets the provider ID.
    /// </summary>
    public AppointmentBuilder WithProviderId(Guid providerId)
    {
        _providerId = providerId;
        return this;
    }

    /// <summary>
    /// Sets the scheduled start time.
    /// </summary>
    public AppointmentBuilder WithScheduledStart(DateTime start)
    {
        _scheduledStart = start;
        return this;
    }

    /// <summary>
    /// Sets the scheduled end time.
    /// </summary>
    public AppointmentBuilder WithScheduledEnd(DateTime end)
    {
        _scheduledEnd = end;
        return this;
    }

    /// <summary>
    /// Sets both start and end times based on duration.
    /// </summary>
    public AppointmentBuilder WithScheduledTime(DateTime start, int durationMinutes)
    {
        _scheduledStart = start;
        _scheduledEnd = start.AddMinutes(durationMinutes);
        _durationMinutes = durationMinutes;
        return this;
    }

    /// <summary>
    /// Sets the appointment type (Office, Telehealth, Phone).
    /// </summary>
    public AppointmentBuilder WithAppointmentType(string type)
    {
        _appointmentType = type ?? throw new ArgumentNullException(nameof(type));
        return this;
    }

    /// <summary>
    /// Sets the appointment status.
    /// Valid values: Scheduled, Confirmed, CheckedIn, Completed, Cancelled, NoShow
    /// </summary>
    public AppointmentBuilder WithStatus(string status)
    {
        _status = status ?? throw new ArgumentNullException(nameof(status));
        return this;
    }

    /// <summary>
    /// Sets the reason for visit.
    /// </summary>
    public AppointmentBuilder WithReasonForVisit(string reason)
    {
        _reasonForVisit = reason;
        return this;
    }

    /// <summary>
    /// Sets additional notes.
    /// </summary>
    public AppointmentBuilder WithNotes(string notes)
    {
        _notes = notes;
        return this;
    }

    /// <summary>
    /// Sets the duration in minutes.
    /// </summary>
    public AppointmentBuilder WithDuration(int minutes)
    {
        _durationMinutes = minutes;
        _scheduledEnd = _scheduledStart.AddMinutes(minutes);
        return this;
    }

    /// <summary>
    /// Sets the reminder sent status.
    /// </summary>
    public AppointmentBuilder WithReminderSent(bool sent)
    {
        _reminderSent = sent;
        return this;
    }

    /// <summary>
    /// Sets the confirmed time.
    /// </summary>
    public AppointmentBuilder WithConfirmedAt(DateTime? confirmedAt)
    {
        _confirmedAt = confirmedAt;
        return this;
    }

    /// <summary>
    /// Sets cancellation information.
    /// </summary>
    public AppointmentBuilder WithCancellation(DateTime cancelledAt, string reason)
    {
        _cancelledAt = cancelledAt;
        _cancellationReason = reason ?? throw new ArgumentNullException(nameof(reason));
        _status = "Cancelled";
        return this;
    }

    /// <summary>
    /// Adds a reminder for this appointment.
    /// </summary>
    public AppointmentBuilder WithReminder(string method = "Email")
    {
        _reminders.Add(method);
        return this;
    }

    /// <summary>
    /// Creates a confirmed appointment.
    /// </summary>
    public AppointmentBuilder AsConfirmed()
    {
        _status = "Confirmed";
        _confirmedAt = DateTime.UtcNow;
        return this;
    }

    /// <summary>
    /// Creates a completed appointment.
    /// </summary>
    public AppointmentBuilder AsCompleted()
    {
        _status = "Completed";
        _confirmedAt = DateTime.UtcNow.AddDays(-1);
        _scheduledStart = DateTime.UtcNow.AddDays(-1);
        _scheduledEnd = DateTime.UtcNow.AddDays(-1).AddMinutes(_durationMinutes);
        return this;
    }

    /// <summary>
    /// Creates a cancelled appointment.
    /// </summary>
    public AppointmentBuilder AsCancelled(string reason = "Patient requested cancellation")
    {
        _status = "Cancelled";
        _cancelledAt = DateTime.UtcNow;
        _cancellationReason = reason;
        return this;
    }

    /// <summary>
    /// Creates a no-show appointment.
    /// </summary>
    public AppointmentBuilder AsNoShow()
    {
        _status = "NoShow";
        _scheduledStart = DateTime.UtcNow.AddDays(-1);
        _scheduledEnd = DateTime.UtcNow.AddDays(-1).AddMinutes(_durationMinutes);
        return this;
    }

    /// <summary>
    /// Resets the builder to default state.
    /// </summary>
    public AppointmentBuilder Reset()
    {
        _id = Guid.NewGuid();
        _patientId = Guid.NewGuid();
        _providerId = Guid.NewGuid();
        _scheduledStart = DateTime.UtcNow.AddDays(7);
        _scheduledEnd = DateTime.UtcNow.AddDays(7).AddMinutes(30);
        _appointmentType = "Office";
        _status = "Scheduled";
        _reasonForVisit = "Annual Checkup";
        _notes = null;
        _durationMinutes = 30;
        _reminderSent = false;
        _confirmedAt = null;
        _cancelledAt = null;
        _cancellationReason = null;
        _reminders.Clear();
        _createdAt = DateTime.UtcNow;
        _updatedAt = DateTime.UtcNow;
        return this;
    }

    /// <summary>
    /// Builds the test appointment entity as a dictionary of properties.
    /// </summary>
    public Dictionary<string, object> Build()
    {
        return new Dictionary<string, object>
        {
            { "Id", _id },
            { "PatientId", _patientId },
            { "ProviderId", _providerId },
            { "ScheduledStart", _scheduledStart },
            { "ScheduledEnd", _scheduledEnd },
            { "AppointmentType", _appointmentType },
            { "Status", _status },
            { "ReasonForVisit", _reasonForVisit ?? string.Empty },
            { "Notes", _notes ?? string.Empty },
            { "DurationMinutes", _durationMinutes },
            { "ReminderSent", _reminderSent },
            { "ConfirmedAt", _confirmedAt },
            { "CancelledAt", _cancelledAt },
            { "CancellationReason", _cancellationReason ?? string.Empty },
            { "Reminders", _reminders },
            { "CreatedAt", _createdAt },
            { "UpdatedAt", _updatedAt }
        };
    }
}
