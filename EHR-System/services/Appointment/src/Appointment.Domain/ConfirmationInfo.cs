namespace EHRPlatform.Services.Appointment.Domain.ValueObjects;

/// <summary>
/// Value object representing appointment confirmation information.
/// Encapsulates when the appointment was confirmed.
/// </summary>
public class ConfirmationInfo : IEquatable<ConfirmationInfo>
{
    /// <summary>
    /// Gets the date and time the appointment was confirmed.
    /// </summary>
    public DateTime ConfirmedAt { get; }

    /// <summary>
    /// Gets the minutes elapsed since confirmation.
    /// </summary>
    public int MinutesSinceConfirmation => (int)(DateTime.UtcNow - ConfirmedAt).TotalMinutes;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfirmationInfo"/> class.
    /// </summary>
    /// <param name="confirmedAt">The confirmation time.</param>
    public ConfirmationInfo(DateTime confirmedAt)
    {
        if (confirmedAt > DateTime.UtcNow)
            throw new ArgumentException("Confirmation time cannot be in the future", nameof(confirmedAt));

        ConfirmedAt = confirmedAt;
    }

    /// <summary>
    /// Determines whether this confirmation is recent (within 1 hour).
    /// </summary>
    /// <returns>True if confirmed within the last hour; otherwise false.</returns>
    public bool IsRecent()
    {
        return MinutesSinceConfirmation < 60;
    }

    public bool Equals(ConfirmationInfo? other)
    {
        return other != null && ConfirmedAt == other.ConfirmedAt;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as ConfirmationInfo);
    }

    public override int GetHashCode()
    {
        return ConfirmedAt.GetHashCode();
    }

    public override string ToString()
    {
        return $"Confirmed at {ConfirmedAt:yyyy-MM-dd HH:mm:ss}";
    }
}
