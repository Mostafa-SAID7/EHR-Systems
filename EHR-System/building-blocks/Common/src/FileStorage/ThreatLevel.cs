namespace EHRPlatform.Common.FileStorage;

/// <summary>
/// Threat level enumeration.
/// Single responsibility: Threat severity values.
/// </summary>
public enum ThreatLevel
{
    /// <summary>
    /// No threat detected.
    /// </summary>
    None = 0,

    /// <summary>
    /// Low threat level.
    /// </summary>
    Low = 1,

    /// <summary>
    /// Medium threat level.
    /// </summary>
    Medium = 2,

    /// <summary>
    /// High threat level.
    /// </summary>
    High = 3,

    /// <summary>
    /// Critical threat - file must be quarantined.
    /// </summary>
    Critical = 4
}
