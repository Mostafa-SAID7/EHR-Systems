namespace EHRPlatform.Services.Analytics.Domain.ValueObjects;

/// <summary>
/// Value object representing a metric name with validation
/// Metric names must be unique, non-empty, and follow naming conventions
/// </summary>
public class MetricName : IEquatable<MetricName>
{
    /// <summary>
    /// The metric name value
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates new MetricName with validation
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if name is invalid</exception>
    public MetricName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Metric name cannot be empty");
        }

        if (value.Length > 100)
        {
            throw new ArgumentException("Metric name cannot exceed 100 characters");
        }

        // Allow only alphanumeric, underscore, and hyphen
        if (!System.Text.RegularExpressions.Regex.IsMatch(value, @"^[a-zA-Z0-9_-]+$"))
        {
            throw new ArgumentException("Metric name can only contain letters, numbers, underscores, and hyphens");
        }

        Value = value;
    }

    public bool Equals(MetricName? other)
    {
        if (other is null) return false;
        return Value == other.Value;
    }

    public override bool Equals(object? obj) => Equals(obj as MetricName);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value;

    /// <summary>
    /// Implicit conversion from string
    /// </summary>
    public static implicit operator MetricName(string value) => new(value);

    /// <summary>
    /// Implicit conversion to string
    /// </summary>
    public static implicit operator string(MetricName metricName) => metricName.Value;
}
