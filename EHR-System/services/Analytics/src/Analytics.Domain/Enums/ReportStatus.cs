namespace EHRPlatform.Services.Analytics.Domain.Enums;

/// <summary>
/// Enumeration for report definition status
/// </summary>
public enum ReportStatus
{
    /// <summary>Report is active and scheduled/executable</summary>
    Active = 1,
    
    /// <summary>Report is temporarily disabled but not deleted</summary>
    Inactive = 2,
    
    /// <summary>Report has been archived and is hidden from lists</summary>
    Archived = 3,
    
    /// <summary>Report is in draft state and not yet published</summary>
    Draft = 4
}
