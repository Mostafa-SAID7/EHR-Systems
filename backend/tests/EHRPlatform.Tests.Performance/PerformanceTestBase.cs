using System;
using System.Diagnostics;
using Xunit;

namespace EHRPlatform.Tests.Performance;

/// <summary>
/// Base class for performance tests
/// </summary>
public abstract class PerformanceTestBase
{
    protected TimeSpan MeasureExecution(Action action)
    {
        var stopwatch = Stopwatch.StartNew();
        action();
        stopwatch.Stop();
        return stopwatch.Elapsed;
    }

    protected TimeSpan MeasureExecution<T>(Func<T> action)
    {
        var stopwatch = Stopwatch.StartNew();
        action();
        stopwatch.Stop();
        return stopwatch.Elapsed;
    }

    protected async Task<TimeSpan> MeasureExecutionAsync(Func<Task> action)
    {
        var stopwatch = Stopwatch.StartNew();
        await action();
        stopwatch.Stop();
        return stopwatch.Elapsed;
    }

    protected async Task<TimeSpan> MeasureExecutionAsync<T>(Func<Task<T>> action)
    {
        var stopwatch = Stopwatch.StartNew();
        await action();
        stopwatch.Stop();
        return stopwatch.Elapsed;
    }

    protected void AssertExecutionTime(TimeSpan elapsed, TimeSpan maxExpected, string message = "")
    {
        Assert.True(elapsed <= maxExpected, 
            $"Execution time {elapsed.TotalMilliseconds}ms exceeded maximum {maxExpected.TotalMilliseconds}ms. {message}");
    }
}
