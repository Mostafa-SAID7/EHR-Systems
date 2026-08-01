using System;

namespace EHRPlatform.Common.DateTime;

/// <summary>
/// Interface for consistent datetime handling.
/// Single responsibility: DateTime provider contract (testable clock).
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Get current UTC datetime.
    /// </summary>
    System.DateTime UtcNow { get; }

    /// <summary>
    /// Get current local datetime.
    /// </summary>
    System.DateTime LocalNow { get; }

    /// <summary>
    /// Get today's date (local).
    /// </summary>
    System.DateTime Today { get; }

    /// <summary>
    /// Convert UTC to local time.
    /// </summary>
    System.DateTime ConvertToLocal(System.DateTime utcDateTime);

    /// <summary>
    /// Convert local time to UTC.
    /// </summary>
    System.DateTime ConvertToUtc(System.DateTime localDateTime);
}
