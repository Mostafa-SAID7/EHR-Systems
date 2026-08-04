namespace EHRPlatform.Services.Analytics.Domain.ValueObjects;

/// <summary>
/// Value object representing a date range with validation
/// </summary>
public class DateRange : IEquatable<DateRange>
{
    /// <summary>
    /// Start date (inclusive)
    /// </summary>
    public DateTime StartDate { get; }

    /// <summary>
    /// End date (inclusive)
    /// </summary>
    public DateTime EndDate { get; }

    /// <summary>
    /// Creates new DateRange with validation
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if StartDate > EndDate</exception>
    public DateRange(DateTime startDate, DateTime endDate)
    {
        if (startDate > endDate)
        {
            throw new ArgumentException("Start date must be less than or equal to end date");
        }

        StartDate = startDate;
        EndDate = endDate;
    }

    /// <summary>
    /// Gets the duration of the range
    /// </summary>
    public TimeSpan Duration => EndDate - StartDate;

    /// <summary>
    /// Gets number of days in range
    /// </summary>
    public int Days => (int)Duration.TotalDays + 1;

    /// <summary>
    /// Checks if a date falls within this range
    /// </summary>
    public bool Contains(DateTime date) => date >= StartDate && date <= EndDate;

    /// <summary>
    /// Checks if this range overlaps with another range
    /// </summary>
    public bool Overlaps(DateRange other) => StartDate <= other.EndDate && EndDate >= other.StartDate;

    /// <summary>
    /// Creates range for today
    /// </summary>
    public static DateRange Today()
    {
        var today = DateTime.UtcNow.Date;
        return new DateRange(today, today);
    }

    /// <summary>
    /// Creates range for last N days
    /// </summary>
    public static DateRange LastDays(int days)
    {
        var endDate = DateTime.UtcNow.Date;
        var startDate = endDate.AddDays(-(days - 1));
        return new DateRange(startDate, endDate);
    }

    /// <summary>
    /// Creates range for current month
    /// </summary>
    public static DateRange CurrentMonth()
    {
        var today = DateTime.UtcNow.Date;
        var startDate = new DateTime(today.Year, today.Month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);
        return new DateRange(startDate, endDate);
    }

    public bool Equals(DateRange? other)
    {
        if (other is null) return false;
        return StartDate == other.StartDate && EndDate == other.EndDate;
    }

    public override bool Equals(object? obj) => Equals(obj as DateRange);

    public override int GetHashCode() => HashCode.Combine(StartDate, EndDate);

    public override string ToString() => $"{StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}";
}
