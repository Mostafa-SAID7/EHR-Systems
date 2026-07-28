using EHRPlatform.Common.Data;
using EHRPlatform.Services.Appointment.Features.Appointments.Domain;

namespace EHRPlatform.Services.Appointment.Domain.Repositories;

/// <summary>
/// Repository interface for Appointment aggregate.
/// Defines contract for appointment persistence operations.
/// </summary>
public interface IAppointmentRepository : IRepository<Appointment>
{
    /// <summary>
    /// Gets an appointment by identifier with related reminders.
    /// </summary>
    /// <param name="appointmentId">The appointment identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The appointment with reminders, or null if not found.</returns>
    Task<Appointment?> GetWithRemindersAsync(Guid appointmentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets appointments for a patient within a date range.
    /// </summary>
    /// <param name="patientId">The patient identifier.</param>
    /// <param name="startDate">The start date.</param>
    /// <param name="endDate">The end date.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of appointments for the patient in the date range.</returns>
    Task<ICollection<Appointment>> GetPatientAppointmentsAsync(
        Guid patientId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets appointments for a provider on a specific date.
    /// </summary>
    /// <param name="providerId">The provider identifier.</param>
    /// <param name="date">The date.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of appointments for the provider on the date.</returns>
    Task<ICollection<Appointment>> GetProviderAppointmentsByDateAsync(
        Guid providerId,
        DateTime date,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets upcoming appointments for a patient.
    /// </summary>
    /// <param name="patientId">The patient identifier.</param>
    /// <param name="daysAhead">Number of days to look ahead.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of upcoming appointments.</returns>
    Task<ICollection<Appointment>> GetUpcomingAppointmentsAsync(
        Guid patientId,
        int daysAhead = 30,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a provider has availability conflict with existing appointments.
    /// </summary>
    /// <param name="providerId">The provider identifier.</param>
    /// <param name="startTime">The start time to check.</param>
    /// <param name="endTime">The end time to check.</param>
    /// <param name="excludeAppointmentId">Optional appointment ID to exclude from check.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if there's a conflict; otherwise false.</returns>
    Task<bool> HasConflictAsync(
        Guid providerId,
        DateTime startTime,
        DateTime endTime,
        Guid? excludeAppointmentId = null,
        CancellationToken cancellationToken = default);
}
