using EHRPlatform.Common.Data;
using EHRPlatform.Services.Appointment.Features.Appointments.Domain;

namespace EHRPlatform.Services.Appointment.Domain.Repositories;

/// <summary>
/// Repository interface for AppointmentReminder entity.
/// Defines contract for appointment reminder persistence operations.
/// </summary>
public interface IAppointmentReminderRepository : IRepository<AppointmentReminder>
{
    /// <summary>
    /// Gets reminders that need to be sent.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of reminders to be sent.</returns>
    Task<ICollection<AppointmentReminder>> GetRemindersToSendAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all reminders for a specific appointment.
    /// </summary>
    /// <param name="appointmentId">The appointment identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of reminders for the appointment.</returns>
    Task<ICollection<AppointmentReminder>> GetAppointmentRemindersAsync(
        Guid appointmentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts unsent reminders for an appointment.
    /// </summary>
    /// <param name="appointmentId">The appointment identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The count of unsent reminders.</returns>
    Task<int> CountUnsentRemindersAsync(
        Guid appointmentId,
        CancellationToken cancellationToken = default);
}
