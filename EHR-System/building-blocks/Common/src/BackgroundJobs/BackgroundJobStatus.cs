namespace EHRPlatform.Common.BackgroundJobs;

/// <summary>
/// Background job status enumeration.
/// Single responsibility: Background job status values.
/// </summary>
public enum BackgroundJobStatus
{
    /// <summary>
    /// Job is scheduled.
    /// </summary>
    Scheduled = 0,

    /// <summary>
    /// Job is executing.
    /// </summary>
    Processing = 1,

    /// <summary>
    /// Job completed successfully.
    /// </summary>
    Succeeded = 2,

    /// <summary>
    /// Job failed.
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Job was deleted.
    /// </summary>
    Deleted = 4
}
