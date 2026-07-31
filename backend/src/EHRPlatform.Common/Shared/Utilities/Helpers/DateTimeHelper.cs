#nullable enable

namespace EHRPlatform.Common.Shared.Utilities.Helpers;

/// <summary>
/// Helper methods for DateTime operations and formatting.
/// Use across all services for consistency.
/// </summary>
public static class DateTimeHelper
{
    /// <summary>
    /// Get current UTC time.
    /// </summary>
    public static DateTime UtcNow => DateTime.UtcNow;

    /// <summary>
    /// Convert DateTime to ISO 8601 format string.
    /// </summary>
    public static string ToIso8601(DateTime dateTime)
    {
        return dateTime.ToString("O"); // O = ISO 8601 round-trip format
    }

    /// <summary>
    /// Convert DateTime to short date format (yyyy-MM-dd).
    /// </summary>
    public static string ToShortDate(DateTime dateTime)
    {
        return dateTime.ToString("yyyy-MM-dd");
    }

    /// <summary>
    /// Convert DateTime to long date format (MMMM dd, yyyy).
    /// </summary>
    public static string ToLongDate(DateTime dateTime)
    {
        return dateTime.ToString("MMMM dd, yyyy");
    }

    /// <summary>
    /// Convert DateTime to time-only format (HH:mm:ss).
    /// </summary>
    public static string ToTimeOnly(DateTime dateTime)
    {
        return dateTime.ToString("HH:mm:ss");
    }

    /// <summary>
    /// Convert DateTime to datetime with time format (yyyy-MM-dd HH:mm:ss).
    /// </summary>
    public static string ToDateTimeString(DateTime dateTime)
    {
        return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
    }

    /// <summary>
    /// Convert DateTime to US format (MM/dd/yyyy).
    /// </summary>
    public static string ToUSFormat(DateTime dateTime)
    {
        return dateTime.ToString("MM/dd/yyyy");
    }

    /// <summary>
    /// Parse ISO 8601 string to DateTime.
    /// </summary>
    public static DateTime? ParseIso8601(string? dateString)
    {
        if (string.IsNullOrWhiteSpace(dateString))
            return null;

        if (DateTime.TryParse(dateString, null, System.Globalization.DateTimeStyles.RoundtripKind, out var result))
            return result;

        return null;
    }

    /// <summary>
    /// Get age from date of birth.
    /// </summary>
    public static int GetAge(DateTime dateOfBirth)
    {
        var today = DateTime.UtcNow;
        var age = today.Year - dateOfBirth.Year;

        if (dateOfBirth.Date > today.AddYears(-age))
            age--;

        return age;
    }

    /// <summary>
    /// Check if date is in the past.
    /// </summary>
    public static bool IsPast(DateTime dateTime)
    {
        return dateTime < UtcNow;
    }

    /// <summary>
    /// Check if date is in the future.
    /// </summary>
    public static bool IsFuture(DateTime dateTime)
    {
        return dateTime > UtcNow;
    }

    /// <summary>
    /// Check if date is today.
    /// </summary>
    public static bool IsToday(DateTime dateTime)
    {
        return dateTime.Date == UtcNow.Date;
    }

    /// <summary>
    /// Get the start of day (00:00:00).
    /// </summary>
    public static DateTime GetStartOfDay(DateTime dateTime)
    {
        return dateTime.Date;
    }

    /// <summary>
    /// Get the end of day (23:59:59).
    /// </summary>
    public static DateTime GetEndOfDay(DateTime dateTime)
    {
        return dateTime.Date.AddDays(1).AddSeconds(-1);
    }

    /// <summary>
    /// Get the start of week (Monday 00:00:00).
    /// </summary>
    public static DateTime GetStartOfWeek(DateTime dateTime)
    {
        var diff = (int)dateTime.DayOfWeek - (int)DayOfWeek.Monday;
        if (diff < 0) diff += 7;
        return dateTime.AddDays(-1 * diff).Date;
    }

    /// <summary>
    /// Get the end of week (Sunday 23:59:59).
    /// </summary>
    public static DateTime GetEndOfWeek(DateTime dateTime)
    {
        return GetStartOfWeek(dateTime).AddDays(7).AddSeconds(-1);
    }

    /// <summary>
    /// Get the start of month (01 00:00:00).
    /// </summary>
    public static DateTime GetStartOfMonth(DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, 1);
    }

    /// <summary>
    /// Get the end of month (last day 23:59:59).
    /// </summary>
    public static DateTime GetEndOfMonth(DateTime dateTime)
    {
        return GetStartOfMonth(dateTime).AddMonths(1).AddSeconds(-1);
    }

    /// <summary>
    /// Get the start of year (01-01 00:00:00).
    /// </summary>
    public static DateTime GetStartOfYear(DateTime dateTime)
    {
        return new DateTime(dateTime.Year, 1, 1);
    }

    /// <summary>
    /// Get the end of year (12-31 23:59:59).
    /// </summary>
    public static DateTime GetEndOfYear(DateTime dateTime)
    {
        return GetStartOfYear(dateTime).AddYears(1).AddSeconds(-1);
    }

    /// <summary>
    /// Calculate business days between two dates (excludes weekends).
    /// </summary>
    public static int GetBusinessDaysBetween(DateTime start, DateTime end)
    {
        var businessDays = 0;
        var current = start;

        while (current <= end)
        {
            if (current.DayOfWeek != DayOfWeek.Saturday && current.DayOfWeek != DayOfWeek.Sunday)
                businessDays++;

            current = current.AddDays(1);
        }

        return businessDays;
    }

    /// <summary>
    /// Get human-readable relative time (e.g., "2 hours ago").
    /// </summary>
    public static string GetRelativeTime(DateTime dateTime)
    {
        var now = UtcNow;
        var timeSpan = now - dateTime;

        if (timeSpan.TotalSeconds < 60)
            return "just now";

        if (timeSpan.TotalMinutes < 60)
            return $"{(int)timeSpan.TotalMinutes} minutes ago";

        if (timeSpan.TotalHours < 24)
            return $"{(int)timeSpan.TotalHours} hours ago";

        if (timeSpan.TotalDays < 7)
            return $"{(int)timeSpan.TotalDays} days ago";

        if (timeSpan.TotalDays < 30)
            return $"{(int)(timeSpan.TotalDays / 7)} weeks ago";

        if (timeSpan.TotalDays < 365)
            return $"{(int)(timeSpan.TotalDays / 30)} months ago";

        return $"{(int)(timeSpan.TotalDays / 365)} years ago";
    }
}

