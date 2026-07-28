using EHRPlatform.Common.DTOs;

namespace EHRPlatform.Services.Appointment.Application.ProviderAvailability.Responses;

/// <summary>
/// Provider availability DTO.
/// Contains provider availability slot information.
/// </summary>
public class ProviderAvailabilityDto : StatusDto
{
    /// <summary>Gets or sets the availability identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the provider identifier.</summary>
    public Guid ProviderId { get; set; }

    /// <summary>Gets or sets the slot start time.</summary>
    public DateTime SlotStart { get; set; }

    /// <summary>Gets or sets the slot end time.</summary>
    public DateTime SlotEnd { get; set; }

    /// <summary>Gets or sets a value indicating whether the slot is recurring.</summary>
    public bool IsRecurring { get; set; }

    /// <summary>Gets or sets the recurrence pattern (Daily, Weekly, Monthly).</summary>
    public string? RecurrencePattern { get; set; }

    /// <summary>Gets or sets the maximum appointments per slot.</summary>
    public int? MaxAppointmentsPerSlot { get; set; }

    /// <summary>Gets or sets the current number of bookings.</summary>
    public int CurrentBookings { get; set; }

    /// <summary>Gets or sets a value indicating whether the slot is active.</summary>
    public bool IsActive { get; set; }
}
