using EHRPlatform.Common.Data;
using EHRPlatform.Services.Appointment.Features.Appointments.Domain;

namespace EHRPlatform.Services.Appointment.Domain.Repositories;

/// <summary>
/// Repository interface for ProviderAvailability aggregate.
/// Defines contract for provider availability persistence operations.
/// </summary>
public interface IProviderAvailabilityRepository : IRepository<ProviderAvailability>
{
    /// <summary>
    /// Gets available slots for booking within a date range.
    /// </summary>
    /// <param name="providerId">The provider identifier.</param>
    /// <param name="startDate">The start date.</param>
    /// <param name="endDate">The end date.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of available slots for booking.</returns>
    Task<ICollection<ProviderAvailability>> GetAvailableSlotsAsync(
        Guid providerId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all availability slots for a provider within a date range.
    /// </summary>
    /// <param name="providerId">The provider identifier.</param>
    /// <param name="startDate">The start date.</param>
    /// <param name="endDate">The end date.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of availability slots.</returns>
    Task<ICollection<ProviderAvailability>> GetProviderAvailabilitySlotsAsync(
        Guid providerId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a provider has an availability slot covering a specific time range.
    /// </summary>
    /// <param name="providerId">The provider identifier.</param>
    /// <param name="startTime">The start time.</param>
    /// <param name="endTime">The end time.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if slot exists and is available; otherwise false.</returns>
    Task<bool> HasAvailableSlotAsync(
        Guid providerId,
        DateTime startTime,
        DateTime endTime,
        CancellationToken cancellationToken = default);
}
