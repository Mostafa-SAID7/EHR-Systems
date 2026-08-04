namespace EHRPlatform.Services.Analytics.Domain.ValueObjects;

/// <summary>
/// Value object representing widget size on dashboard
/// Size is measured in grid units (columns x rows)
/// </summary>
public class WidgetSize : IEquatable<WidgetSize>
{
    /// <summary>
    /// Width in grid columns (minimum 1)
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Height in grid rows (minimum 1)
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Creates new WidgetSize with validation
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if Width or Height are less than 1</exception>
    public WidgetSize(int width, int height)
    {
        if (width < 1 || height < 1)
        {
            throw new ArgumentException("Widget size must be at least 1x1");
        }

        Width = width;
        Height = height;
    }

    /// <summary>
    /// Gets total grid cells occupied
    /// </summary>
    public int GridCells => Width * Height;

    /// <summary>
    /// Standard sizes
    /// </summary>
    public static WidgetSize Small => new(1, 1);      // 1 cell
    public static WidgetSize Medium => new(2, 2);     // 4 cells
    public static WidgetSize Large => new(3, 3);      // 9 cells
    public static WidgetSize Full => new(12, 1);      // Full width, 1 row

    public bool Equals(WidgetSize? other)
    {
        if (other is null) return false;
        return Width == other.Width && Height == other.Height;
    }

    public override bool Equals(object? obj) => Equals(obj as WidgetSize);

    public override int GetHashCode() => HashCode.Combine(Width, Height);

    public override string ToString() => $"{Width}x{Height}";
}
