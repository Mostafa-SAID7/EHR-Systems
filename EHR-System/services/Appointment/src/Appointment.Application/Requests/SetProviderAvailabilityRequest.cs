namespace EHRPlatform.Services.Appointment.Application.ProviderAvailability.Requests;

/// <summary>
/// Set provider availability request DTO.
/// Contains data required to create or update provider availability slots.
/// </summary>
public class SetProviderAvailabilityRequest
{
    /// <summary>Gets or sets the provider identifier.</summary>
    public Guid ProviderId { get; set; }

    /// <summary>Gets or sets the slot start time.</summary>
    public DateTime SlotStart { get; set; }

    /// <summary>Gets or sets the slot end time.</summary>
    public DateTime SlotEnd { get; set; }

    /// <summary>Gets or sets a value indicating whether this slot is recurring.</summary>
    public bool IsRecurring { get; set; }

    /// <summary>Gets or sets the recurrence pattern (Daily, Weekly, Monthly).</summary>
    public string? RecurrencePattern { get; set; }

    /// <summary>Gets or sets the maximum appointments allowed per slot.</summary>
    public int? MaxAppointmentsPerSlot { get; set; }
}
