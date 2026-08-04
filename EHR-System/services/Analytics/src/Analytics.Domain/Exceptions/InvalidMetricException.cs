using System;

namespace EHRPlatform.Services.Analytics.Domain.Exceptions;

/// <summary>
/// Exception thrown when metric validation fails
/// </summary>
public class InvalidMetricException : Exception
{
    public InvalidMetricException(string message) : base(message)
    {
    }

    public InvalidMetricException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }

    public static InvalidMetricException MetricNotFound(Guid id)
        => new($"Metric with ID '{id}' not found.");

    public static InvalidMetricException MetricNameRequired()
        => new("Metric name is required.");

    public static InvalidMetricException InvalidMetricValue(double value)
        => new($"Invalid metric value: {value}");

    public static InvalidMetricException CategoryRequired()
        => new("Metric category is required.");

    public static InvalidMetricException SourceServiceRequired()
        => new("Source service is required.");
}
