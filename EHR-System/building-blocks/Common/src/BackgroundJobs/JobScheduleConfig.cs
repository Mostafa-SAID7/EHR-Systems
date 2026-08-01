using System;

namespace EHRPlatform.Common.BackgroundJobs;

/// <summary>
/// Job schedule configuration.
/// Single responsibility: Job schedule configuration data structure.
/// </summary>
public class JobScheduleConfig
{
    /// <summary>
    /// Execution time (for one-time jobs).
    /// </summary>
    public DateTime? ExecuteAt { get; set; }

    /// <summary>
    /// Cron expression (for recurring jobs).
    /// </summary>
    public string? CronExpression { get; set; }

    /// <summary>
    /// Time zone ID.
    /// </summary>
    public string TimeZoneId { get; set; } = "UTC";

    /// <summary>
    /// Maximum retry attempts.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Retry delay in seconds.
    /// </summary>
    public int RetryDelaySeconds { get; set; } = 60;
}
