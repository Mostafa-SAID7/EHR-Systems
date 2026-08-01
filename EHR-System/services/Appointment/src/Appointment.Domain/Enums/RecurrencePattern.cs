namespace EHRPlatform.Services.Appointment.Domain.Enums;

/// <summary>
/// Recurrence pattern for provider availability slots.
/// </summary>
public enum RecurrencePattern
{
    /// <summary>Once (no recurrence).</summary>
    Once = 0,

    /// <summary>Every day.</summary>
    Daily = 1,

    /// <summary>Every week.</summary>
    Weekly = 2,

    /// <summary>Every 2 weeks.</summary>
    BiWeekly = 3,

    /// <summary>Every month.</summary>
    Monthly = 4,

    /// <summary>Every year.</summary>
    Yearly = 5
}
