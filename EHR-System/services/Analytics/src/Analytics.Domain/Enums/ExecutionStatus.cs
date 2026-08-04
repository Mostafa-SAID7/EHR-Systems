namespace EHRPlatform.Services.Analytics.Domain.Enums;

/// <summary>
/// Enumeration for report execution status
/// </summary>
public enum ExecutionStatus
{
    /// <summary>Report execution is queued and waiting to run</summary>
    Queued = 0,
    
    /// <summary>Report is currently executing</summary>
    Running = 1,
    
    /// <summary>Report execution completed successfully</summary>
    Success = 2,
    
    /// <summary>Report execution failed with error</summary>
    Failed = 3,
    
    /// <summary>Report execution was cancelled by user</summary>
    Cancelled = 4,
    
    /// <summary>Report execution timed out</summary>
    TimedOut = 5,
    
    /// <summary>Report execution partially completed with warnings</summary>
    PartialSuccess = 6
}
