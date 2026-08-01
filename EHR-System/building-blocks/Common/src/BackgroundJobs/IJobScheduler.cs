using System;
using System.Threading;
using System.Threading.Tasks;

namespace EHRPlatform.Common.BackgroundJobs;

/// <summary>
/// Interface for job scheduling.
/// Single responsibility: Schedule jobs with specific timing.
/// </summary>
public interface IJobScheduler
{
    /// <summary>
    /// Add job to schedule.
    /// </summary>
    Task<string> AddJobAsync<T>(T job, JobScheduleConfig config, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Update job schedule.
    /// </summary>
    Task<bool> UpdateJobAsync(string jobId, JobScheduleConfig config, CancellationToken cancellationToken = default);

    /// <summary>
    /// Pause job.
    /// </summary>
    Task<bool> PauseJobAsync(string jobId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resume job.
    /// </summary>
    Task<bool> ResumeJobAsync(string jobId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get next execution time.
    /// </summary>
    Task<DateTime?> GetNextExecutionAsync(string jobId, CancellationToken cancellationToken = default);
}
