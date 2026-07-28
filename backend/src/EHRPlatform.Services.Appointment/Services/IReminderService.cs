using EHRPlatform.Services.Appointment.Application.Appointments.Responses;
using EHRPlatform.Services.Appointment.Domain.Enums;

namespace EHRPlatform.Services.Appointment.Services;

/// <summary>
/// Interface for appointment reminder service.
/// Handles reminder scheduling, sending, and management.
/// </summary>
public interface IReminderService
{
    /// <summary>
    /// Schedule a reminder for an appointment.
    /// </summary>
    /// <param name="appointmentId">The appointment identifier.</param>
    /// <param name="reminderTime">The time to send the reminder.</param>
    /// <param name="reminderType">The reminder delivery method.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task ScheduleReminderAsync(Guid appointmentId, DateTime reminderTime, ReminderType reminderType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all pending reminders that need to be sent.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Collection of pending reminders.</returns>
    Task<IEnumerable<AppointmentReminderDto>> GetPendingRemindersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Send a specific reminder.
    /// </summary>
    /// <param name="reminderId">The reminder identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task SendReminderAsync(Guid reminderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send all pending reminders.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Number of reminders sent.</returns>
    Task<int> SendPendingRemindersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancel a scheduled reminder.
    /// </summary>
    /// <param name="reminderId">The reminder identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task CancelReminderAsync(Guid reminderId, CancellationToken cancellationToken = default);
}
