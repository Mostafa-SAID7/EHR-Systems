using EHRPlatform.BuildingBlocks.Common.Events;

namespace EHRPlatform.Services.Appointment.Domain.Events;

/// <summary>
/// Domain event raised when a provider availability slot is released.
/// </summary>
public class ProviderAvailabilitySlotReleasedEvent : IntegrationEvent
{
    /// <summary>
    /// Gets the availability identifier.
    /// </summary>
    public Guid AvailabilityId { get; set; }

    /// <summary>
    /// Gets the appointment identifier that was using this slot.
    /// </summary>
    public Guid AppointmentId { get; set; }

    /// <summary>
    /// Gets the provider identifier.
    /// </summary>
    public Guid ProviderId { get; set; }

    /// <summary>
    /// Gets the current bookings count after release.
    /// </summary>
    public int RemainingBookings { get; set; }

    /// <summary>
    /// Gets the release reason.
    /// </summary>
    public string Reason { get; set; }

    /// <summary>
    /// Gets the time the slot was released.
    /// </summary>
    public DateTime ReleasedAt { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProviderAvailabilitySlotReleasedEvent"/> class.
    /// </summary>
    public ProviderAvailabilitySlotReleasedEvent(
        Guid availabilityId,
        Guid appointmentId,
        Guid providerId,
        int remainingBookings,
        string reason,
        DateTime releasedAt)
    {
        AvailabilityId = availabilityId;
        AppointmentId = appointmentId;
        ProviderId = providerId;
        RemainingBookings = remainingBookings;
        Reason = reason;
        ReleasedAt = releasedAt;
    }
}

