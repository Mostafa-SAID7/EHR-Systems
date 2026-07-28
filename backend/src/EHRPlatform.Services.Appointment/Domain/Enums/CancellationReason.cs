namespace EHRPlatform.Services.Appointment.Domain.Enums;

/// <summary>
/// Reasons for appointment cancellation.
/// </summary>
public enum CancellationReason
{
    /// <summary>Cancelled by patient.</summary>
    PatientRequested = 1,

    /// <summary>Cancelled by provider.</summary>
    ProviderRequested = 2,

    /// <summary>Cancelled due to emergency.</summary>
    Emergency = 3,

    /// <summary>Cancelled due to double booking.</summary>
    DoubleBooking = 4,

    /// <summary>Cancelled due to scheduling conflict.</summary>
    SchedulingConflict = 5,

    /// <summary>Cancelled due to weather.</summary>
    Weather = 6,

    /// <summary>Cancelled due to system error.</summary>
    SystemError = 7,

    /// <summary>Cancelled for other reason.</summary>
    Other = 8
}
