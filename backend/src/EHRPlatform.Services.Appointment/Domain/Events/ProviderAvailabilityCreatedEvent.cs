using EHRPlatform.BuildingBlocks.Common.Events;
using EHRPlatform.Services.Appointment.Domain.Enums;

namespace EHRPlatform.Services.Appointment.Domain.Events;

/// <summary>
/// Domain event raised when provider availability is created.
/// </summary>
public class ProviderAvailabilityCreatedEvent : IntegrationEvent
{
    /// <summary>
    /// Gets the provider availability identifier.
    /// </summary>
    public Guid AvailabilityId { get; set; }

    /// <summary>
    /// Gets the provider identifier.
    /// </summary>
    public Guid ProviderId { get; set; }

    /// <summary>
    /// Gets the slot start time.
    /// </summary>
    public DateTime SlotStart { get; set; }

    /// <summary>
    /// Gets the slot end time.
    /// </summary>
    public DateTime SlotEnd { get; set; }

    /// <summary>
    /// Gets the recurrence pattern.
    /// </summary>
    public RecurrencePattern RecurrencePattern { get; set; }

    /// <summary>
    /// Gets the maximum appointments per slot.
    /// </summary>
    public int? MaxAppointmentsPerSlot { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProviderAvailabilityCreatedEvent"/> class.
    /// </summary>
    public ProviderAvailabilityCreatedEvent(
        Guid availabilityId,
        Guid providerId,
        DateTime slotStart,
        DateTime slotEnd,
        RecurrencePattern recurrencePattern,
        int? maxAppointmentsPerSlot)
    {
        AvailabilityId = availabilityId;
        ProviderId = providerId;
        SlotStart = slotStart;
        SlotEnd = slotEnd;
        RecurrencePattern = recurrencePattern;
        MaxAppointmentsPerSlot = maxAppointmentsPerSlot;
    }
}

