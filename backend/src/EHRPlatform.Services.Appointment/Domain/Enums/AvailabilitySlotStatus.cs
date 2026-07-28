namespace EHRPlatform.Services.Appointment.Domain.Enums;

/// <summary>
/// Status of an availability slot.
/// </summary>
public enum AvailabilitySlotStatus
{
    /// <summary>Slot is available for booking.</summary>
    Available = 1,

    /// <summary>Slot is fully booked.</summary>
    Booked = 2,

    /// <summary>Slot is blocked/unavailable.</summary>
    Blocked = 3,

    /// <summary>Slot is reserved.</summary>
    Reserved = 4,

    /// <summary>Slot is in progress.</summary>
    InProgress = 5,

    /// <summary>Slot is expired/past.</summary>
    Expired = 6
}
