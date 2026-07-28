using System;
using System.Collections.Generic;

namespace EHRPlatform.Tests.Performance.Metrics;

/// <summary>
/// Performance metrics collection and reporting
/// </summary>
public class PerformanceMetrics
{
    public string TestName { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public long MemoryUsedBytes { get; set; }
    public int ThreadCount { get; set; }
    public Dictionary<string, double> CustomMetrics { get; } = new();

    public TimeSpan Duration => EndTime - StartTime;

    public double DurationMs => Duration.TotalMilliseconds;

    public void RecordMetric(string name, double value)
    {
        CustomMetrics[name] = value;
    }

    public override string ToString()
    {
        var result = $"Test: {TestName}\n";
        result += $"Duration: {DurationMs}ms\n";
        result += $"Memory: {MemoryUsedBytes / 1024.0 / 1024.0:F2}MB\n";
        result += $"Threads: {ThreadCount}\n";

        foreach (var metric in CustomMetrics)
        {
            result += $"{metric.Key}: {metric.Value}\n";
        }

        return result;
    }
}
