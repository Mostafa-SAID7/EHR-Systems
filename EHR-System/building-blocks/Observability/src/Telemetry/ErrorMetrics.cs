using System;
using System.Collections.Generic;

namespace EHRPlatform.Observability.Telemetry;

/// <summary>
/// Implementation of error metrics tracking
/// </summary>
public class ErrorMetrics : IErrorMetrics
{
    private readonly ApplicationMetrics _metrics;
    private readonly Dictionary<string, long> _errorTypeCounters = new();
    private readonly object _lockObject = new();

    public ErrorMetrics(ApplicationMetrics metrics)
    {
        _metrics = metrics;
    }

    public void RecordError(string errorType, string? message = null, Dictionary<string, object>? context = null)
    {
        lock (_lockObject)
        {
            if (!_errorTypeCounters.ContainsKey(errorType))
            {
                _errorTypeCounters[errorType] = 0;
            }
            _errorTypeCounters[errorType]++;
        }
    }

    public void RecordException(Exception exception, string? operation = null, Dictionary<string, object>? context = null)
    {
        lock (_lockObject)
        {
            var exceptionType = exception.GetType().Name;
            RecordError(exceptionType, exception.Message, context);
        }
    }

    public double GetErrorRate(TimeSpan timeWindow)
    {
        lock (_lockObject)
        {
            long totalErrors = 0;
            foreach (var count in _errorTypeCounters.Values)
            {
                totalErrors += count;
            }
            return totalErrors;
        }
    }

    public Dictionary<string, long> GetErrorsByType()
    {
        lock (_lockObject)
        {
            return new Dictionary<string, long>(_errorTypeCounters);
        }
    }
}
