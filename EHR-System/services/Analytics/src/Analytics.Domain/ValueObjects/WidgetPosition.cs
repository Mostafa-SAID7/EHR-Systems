namespace EHRPlatform.Services.Analytics.Domain.ValueObjects;

/// <summary>
/// Value object representing widget position on dashboard
/// </summary>
public class WidgetPosition : IEquatable<WidgetPosition>
{
    /// <summary>
    /// Horizontal position (column)
    /// </summary>
    public int X { get; }

    /// <summary>
    /// Vertical position (row)
    /// </summary>
    public int Y { get; }

    /// <summary>
    /// Creates new WidgetPosition with validation
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if X or Y are negative</exception>
    public WidgetPosition(int x, int y)
    {
        if (x < 0 || y < 0)
        {
            throw new ArgumentException("Position coordinates must be non-negative");
        }

        X = x;
        Y = y;
    }

    /// <summary>
    /// Checks if two positions overlap (same grid cell)
    /// </summary>
    public bool OverlapsWith(WidgetPosition other) => X == other.X && Y == other.Y;

    /// <summary>
    /// Factory method for top-left position
    /// </summary>
    public static WidgetPosition TopLeft() => new(0, 0);

    public bool Equals(WidgetPosition? other)
    {
        if (other is null) return false;
        return X == other.X && Y == other.Y;
    }

    public override bool Equals(object? obj) => Equals(obj as WidgetPosition);

    public override int GetHashCode() => HashCode.Combine(X, Y);

    public override string ToString() => $"({X}, {Y})";
}
