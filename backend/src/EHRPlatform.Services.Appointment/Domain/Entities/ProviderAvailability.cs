using EHRPlatform.Common.Domain.Entities;

namespace EHRPlatform.Services.Appointment.Features.Appointments.Domain;

/// <summary>
/// Provider availability slot entity.
/// Represents recurring or one-time availability slots for healthcare providers.
/// </summary>
public class ProviderAvailability : BaseEntity
{
    /// <summary>
    /// Gets or sets the provider identifier.
    /// </summary>
    public Guid ProviderId { get; set; }

    /// <summary>
    /// Gets or sets the start time of the availability slot.
    /// </summary>
    public DateTime SlotStart { get; set; }

    /// <summary>
    /// Gets or sets the end time of the availability slot.
    /// </summary>
    public DateTime SlotEnd { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this slot is recurring.
    /// </summary>
    public bool IsRecurring { get; set; }

    /// <summary>
    /// Gets or sets the recurrence pattern.
    /// Possible values: Daily, Weekly, Monthly
    /// </summary>
    public string? RecurrencePattern { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of appointments allowed per slot.
    /// Null means unlimited appointments.
    /// </summary>
    public int? MaxAppointmentsPerSlot { get; set; }

    /// <summary>
    /// Gets or sets the current number of appointments booked in this slot.
    /// </summary>
    public int CurrentBookings { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this availability slot is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Determines whether the slot has availability for additional appointments.
    /// </summary>
    /// <returns>True if the slot can accommodate more appointments; otherwise false.</returns>
    public bool HasAvailability() =>
        MaxAppointmentsPerSlot == null || CurrentBookings < MaxAppointmentsPerSlot.Value;

    /// <summary>
    /// Books a slot by incrementing the current bookings count.
    /// </summary>
    public void BookSlot() => CurrentBookings++;

    /// <summary>
    /// Releases a slot by decrementing the current bookings count.
    /// </summary>
    public void ReleaseSlot() => CurrentBookings = Math.Max(0, CurrentBookings - 1);
}

