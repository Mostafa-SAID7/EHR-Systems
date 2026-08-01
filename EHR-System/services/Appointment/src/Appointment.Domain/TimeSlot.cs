namespace EHRPlatform.Services.Appointment.Domain.ValueObjects;

/// <summary>
/// Value object representing a time slot with start and end times.
/// Ensures consistency and validation of appointment durations.
/// </summary>
public class TimeSlot : IEquatable<TimeSlot>
{
    /// <summary>
    /// Gets the start time of the slot.
    /// </summary>
    public DateTime Start { get; }

    /// <summary>
    /// Gets the end time of the slot.
    /// </summary>
    public DateTime End { get; }

    /// <summary>
    /// Gets the duration of the slot in minutes.
    /// </summary>
    public int DurationMinutes { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeSlot"/> class.
    /// </summary>
    /// <param name="start">The start time.</param>
    /// <param name="end">The end time.</param>
    /// <exception cref="ArgumentException">Thrown when start time is not before end time.</exception>
    public TimeSlot(DateTime start, DateTime end)
    {
        if (start >= end)
            throw new ArgumentException("Start time must be before end time", nameof(start));

        if (start < DateTime.UtcNow)
            throw new ArgumentException("Start time cannot be in the past", nameof(start));

        Start = start;
        End = end;
        DurationMinutes = (int)(end - start).TotalMinutes;
    }

    /// <summary>
    /// Determines whether this time slot overlaps with another.
    /// </summary>
    /// <param name="other">The other time slot.</param>
    /// <returns>True if the slots overlap; otherwise false.</returns>
    public bool OverlapsWith(TimeSlot other)
    {
        return Start < other.End && End > other.Start;
    }

    /// <summary>
    /// Determines whether this time slot contains another.
    /// </summary>
    /// <param name="other">The other time slot.</param>
    /// <returns>True if this slot contains the other; otherwise false.</returns>
    public bool Contains(TimeSlot other)
    {
        return Start <= other.Start && End >= other.End;
    }

    /// <summary>
    /// Determines whether this time slot is after another.
    /// </summary>
    /// <param name="other">The other time slot.</param>
    /// <returns>True if this slot starts after the other ends; otherwise false.</returns>
    public bool IsAfter(TimeSlot other)
    {
        return Start >= other.End;
    }

    /// <summary>
    /// Determines whether this time slot is before another.
    /// </summary>
    /// <param name="other">The other time slot.</param>
    /// <returns>True if this slot ends before the other starts; otherwise false.</returns>
    public bool IsBefore(TimeSlot other)
    {
        return End <= other.Start;
    }

    public bool Equals(TimeSlot? other)
    {
        return other != null && Start == other.Start && End == other.End;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as TimeSlot);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Start, End);
    }

    public override string ToString()
    {
        return $"{Start:yyyy-MM-dd HH:mm:ss} - {End:yyyy-MM-dd HH:mm:ss} ({DurationMinutes} min)";
    }
}
