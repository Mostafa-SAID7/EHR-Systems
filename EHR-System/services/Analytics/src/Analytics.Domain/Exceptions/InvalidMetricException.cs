namespace EHRPlatform.Services.Analytics.Domain.Exceptions;

/// <summary>
/// Exception thrown when metric data is invalid
/// </summary>
public class InvalidMetricException : DomainException
{
    public InvalidMetricException(string message) : base(message)
    {
    }

    public InvalidMetricException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Factory method for invalid metric name
    /// </summary>
    public static InvalidMetricException InvalidName(string name) =>
        new($"Metric name '{name}' is invalid");

    /// <summary>
    /// Factory method for invalid metric category
    /// </summary>
    public static InvalidMetricException InvalidCategory(string category) =>
        new($"Metric category '{category}' is not recognized");

    /// <summary>
    /// Factory method for invalid metric unit
    /// </summary>
    public static InvalidMetricException InvalidUnit(string unit) =>
        new($"Metric unit '{unit}' is not recognized");

    /// <summary>
    /// Factory method for metric not found
    /// </summary>
    public static InvalidMetricException NotFound(Guid metricId) =>
        new($"Metric '{metricId}' not found");
}
