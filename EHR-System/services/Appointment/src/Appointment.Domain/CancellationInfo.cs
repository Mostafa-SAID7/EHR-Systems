using EHRPlatform.Services.Appointment.Domain.Enums;

namespace EHRPlatform.Services.Appointment.Domain.ValueObjects;

/// <summary>
/// Value object representing cancellation information.
/// Encapsulates cancellation reason and timestamp.
/// </summary>
public class CancellationInfo : IEquatable<CancellationInfo>
{
    /// <summary>
    /// Gets the cancellation reason.
    /// </summary>
    public CancellationReason Reason { get; }

    /// <summary>
    /// Gets the date and time the appointment was cancelled.
    /// </summary>
    public DateTime CancelledAt { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CancellationInfo"/> class.
    /// </summary>
    /// <param name="reason">The cancellation reason.</param>
    /// <param name="cancelledAt">The cancellation time.</param>
    public CancellationInfo(CancellationReason reason, DateTime cancelledAt)
    {
        Reason = reason;
        CancelledAt = cancelledAt;
    }

    /// <summary>
    /// Determines whether this cancellation is recent (within 24 hours).
    /// </summary>
    /// <returns>True if cancelled within the last 24 hours; otherwise false.</returns>
    public bool IsRecent()
    {
        return DateTime.UtcNow.Subtract(CancelledAt).TotalHours < 24;
    }

    public bool Equals(CancellationInfo? other)
    {
        return other != null && Reason == other.Reason && CancelledAt == other.CancelledAt;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as CancellationInfo);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Reason, CancelledAt);
    }

    public override string ToString()
    {
        return $"Cancelled: {Reason} at {CancelledAt:yyyy-MM-dd HH:mm:ss}";
    }
}
