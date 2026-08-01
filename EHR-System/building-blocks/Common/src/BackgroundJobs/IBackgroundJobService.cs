using System;
using System.Threading;
using System.Threading.Tasks;

namespace EHRPlatform.Common.BackgroundJobs;

/// <summary>
/// Interface for background job execution service.
/// Single responsibility: Schedule and execute background jobs.
/// </summary>
public interface IBackgroundJobService
{
    /// <summary>
    /// Enqueue job for immediate execution.
    /// </summary>
    Task<string> EnqueueAsync<T>(T job, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Schedule job for later execution.
    /// </summary>
    Task<string> ScheduleAsync<T>(T job, DateTime enqueueAt, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Schedule recurring job.
    /// </summary>
    Task<string> ScheduleRecurringAsync<T>(string jobId, T job, string cronExpression, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Delete job.
    /// </summary>
    Task<bool> DeleteAsync(string jobId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get job status.
    /// </summary>
    Task<BackgroundJobStatus?> GetStatusAsync(string jobId, CancellationToken cancellationToken = default);
}
