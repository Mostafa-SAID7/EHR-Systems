namespace EHRPlatform.Services.Appointment.Application.ProviderAvailability.Responses;

/// <summary>
/// Provider availability slot DTO.
/// Represents a single availability slot with booking information.
/// </summary>
public class ProviderAvailabilitySlotDto
{
    /// <summary>Gets or sets the slot identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the slot start time.</summary>
    public DateTime SlotStart { get; set; }

    /// <summary>Gets or sets the slot end time.</summary>
    public DateTime SlotEnd { get; set; }

    /// <summary>Gets or sets a value indicating whether the slot is recurring.</summary>
    public bool IsRecurring { get; set; }

    /// <summary>Gets or sets the recurrence pattern.</summary>
    public string? RecurrencePattern { get; set; }

    /// <summary>Gets or sets the maximum appointments per slot.</summary>
    public int? MaxAppointmentsPerSlot { get; set; }

    /// <summary>Gets or sets the current number of bookings.</summary>
    public int CurrentBookings { get; set; }

    /// <summary>Gets or sets a value indicating whether the slot has available capacity.</summary>
    public bool HasAvailability { get; set; }
}
