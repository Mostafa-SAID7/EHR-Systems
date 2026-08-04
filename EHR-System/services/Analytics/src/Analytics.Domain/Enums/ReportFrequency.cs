namespace EHRPlatform.Services.Analytics.Domain.Enums;

/// <summary>
/// Enumeration for report execution frequency
/// </summary>
public enum ReportFrequency
{
    /// <summary>Report executed manually on demand</summary>
    OnDemand = 0,
    
    /// <summary>Report executed daily</summary>
    Daily = 1,
    
    /// <summary>Report executed weekly</summary>
    Weekly = 2,
    
    /// <summary>Report executed bi-weekly (every 2 weeks)</summary>
    BiWeekly = 3,
    
    /// <summary>Report executed monthly</summary>
    Monthly = 4,
    
    /// <summary>Report executed quarterly</summary>
    Quarterly = 5,
    
    /// <summary>Report executed yearly</summary>
    Yearly = 6,
    
    /// <summary>Report executed at custom schedule via cron</summary>
    Custom = 7
}
