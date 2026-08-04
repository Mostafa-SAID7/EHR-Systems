namespace EHRPlatform.Services.Analytics.Domain.ValueObjects;

/// <summary>
/// Value object representing display/sort order with validation
/// </summary>
public class DisplayOrder : IEquatable<DisplayOrder>, IComparable<DisplayOrder>
{
    /// <summary>
    /// Order value (0 = first, 1 = second, etc.)
    /// </summary>
    public int Value { get; }

    /// <summary>
    /// Creates new DisplayOrder with validation
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if order is negative</exception>
    public DisplayOrder(int value)
    {
        if (value < 0)
        {
            throw new ArgumentException("Display order cannot be negative");
        }

        Value = value;
    }

    /// <summary>
    /// Gets next order
    /// </summary>
    public DisplayOrder Next() => new(Value + 1);

    /// <summary>
    /// Gets previous order
    /// </summary>
    public DisplayOrder Previous() => new(Math.Max(0, Value - 1));

    public int CompareTo(DisplayOrder? other)
    {
        if (other is null) return 1;
        return Value.CompareTo(other.Value);
    }

    public bool Equals(DisplayOrder? other)
    {
        if (other is null) return false;
        return Value == other.Value;
    }

    public override bool Equals(object? obj) => Equals(obj as DisplayOrder);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value.ToString();

    public static implicit operator DisplayOrder(int value) => new(value);
    public static implicit operator int(DisplayOrder order) => order.Value;
}
