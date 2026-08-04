namespace EHRPlatform.Services.Analytics.Domain.Enums;

/// <summary>
/// Enumeration for metric units - specifies the measurement unit
/// </summary>
public enum MetricUnit
{
    /// <summary>Count or number (e.g., number of patients)</summary>
    Count = 1,
    
    /// <summary>Percentage value (0-100)</summary>
    Percentage = 2,
    
    /// <summary>Currency value (dollars, euros, etc.)</summary>
    Currency = 3,
    
    /// <summary>Time duration in minutes</summary>
    Minutes = 4,
    
    /// <summary>Time duration in hours</summary>
    Hours = 5,
    
    /// <summary>Time duration in seconds</summary>
    Seconds = 6,
    
    /// <summary>Bytes of data</summary>
    Bytes = 7,
    
    /// <summary>Generic numeric value</summary>
    Numeric = 8,
    
    /// <summary>Boolean/Yes-No value</summary>
    Boolean = 9,
    
    /// <summary>Text/String value</summary>
    Text = 10
}
