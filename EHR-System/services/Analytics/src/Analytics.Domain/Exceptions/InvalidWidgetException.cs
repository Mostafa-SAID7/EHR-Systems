namespace EHRPlatform.Services.Analytics.Domain.Exceptions;

/// <summary>
/// Exception thrown when widget data is invalid
/// </summary>
public class InvalidWidgetException : DomainException
{
    public InvalidWidgetException(string message) : base(message)
    {
    }

    public InvalidWidgetException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Factory method for widget not found
    /// </summary>
    public static InvalidWidgetException NotFound(Guid widgetId) =>
        new($"Widget '{widgetId}' not found");

    /// <summary>
    /// Factory method for invalid widget size
    /// </summary>
    public static InvalidWidgetException InvalidSize(int width, int height) =>
        new($"Invalid widget size: width={width}, height={height}. Minimum size is 1x1");

    /// <summary>
    /// Factory method for invalid widget type
    /// </summary>
    public static InvalidWidgetException InvalidType(string widgetType) =>
        new($"Widget type '{widgetType}' is not recognized");

    /// <summary>
    /// Factory method for invalid position
    /// </summary>
    public static InvalidWidgetException InvalidPosition(int x, int y) =>
        new($"Invalid widget position: x={x}, y={y}. Coordinates must be non-negative");
}
