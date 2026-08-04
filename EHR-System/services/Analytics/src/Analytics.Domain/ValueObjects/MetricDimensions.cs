namespace EHRPlatform.Services.Analytics.Domain.ValueObjects;

/// <summary>
/// Value object representing metric filtering dimensions
/// Dimensions enable multi-dimensional analysis: Provider, Department, Location, etc.
/// </summary>
public class MetricDimensions : IEquatable<MetricDimensions>
{
    /// <summary>
    /// Primary dimension (e.g., Provider ID, Department ID, Location ID)
    /// </summary>
    public string? Dimension1 { get; }

    /// <summary>
    /// Secondary dimension (e.g., Service Type, Status, Category)
    /// </summary>
    public string? Dimension2 { get; }

    /// <summary>
    /// Tertiary dimension (e.g., Sub-category, Classification)
    /// </summary>
    public string? Dimension3 { get; }

    /// <summary>
    /// Creates new MetricDimensions
    /// </summary>
    public MetricDimensions(string? dimension1 = null, string? dimension2 = null, string? dimension3 = null)
    {
        Dimension1 = dimension1;
        Dimension2 = dimension2;
        Dimension3 = dimension3;
    }

    public bool Equals(MetricDimensions? other)
    {
        if (other is null) return false;
        return Dimension1 == other.Dimension1 && Dimension2 == other.Dimension2 && Dimension3 == other.Dimension3;
    }

    public override bool Equals(object? obj) => Equals(obj as MetricDimensions);

    public override int GetHashCode() => HashCode.Combine(Dimension1, Dimension2, Dimension3);

    public override string ToString() => $"{Dimension1}|{Dimension2}|{Dimension3}";

    /// <summary>
    /// Factory method to create empty dimensions
    /// </summary>
    public static MetricDimensions Empty => new();

    /// <summary>
    /// Checks if any dimension is set
    /// </summary>
    public bool IsEmpty => string.IsNullOrEmpty(Dimension1) && string.IsNullOrEmpty(Dimension2) && string.IsNullOrEmpty(Dimension3);
}
