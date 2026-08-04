namespace EHRPlatform.Services.Analytics.Domain.Exceptions;

/// <summary>
/// Exception thrown when report data is invalid
/// </summary>
public class InvalidReportException : DomainException
{
    public InvalidReportException(string message) : base(message)
    {
    }

    public InvalidReportException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Factory method for report not found
    /// </summary>
    public static InvalidReportException NotFound(Guid reportId) =>
        new($"Report '{reportId}' not found");

    /// <summary>
    /// Factory method for invalid report status
    /// </summary>
    public static InvalidReportException InvalidStatus(string status) =>
        new($"Report status '{status}' is not valid");

    /// <summary>
    /// Factory method for invalid query definition
    /// </summary>
    public static InvalidReportException InvalidQueryDefinition(string details) =>
        new($"Invalid query definition: {details}");

    /// <summary>
    /// Factory method for invalid cron expression
    /// </summary>
    public static InvalidReportException InvalidCronExpression(string cronExpression) =>
        new($"Invalid cron expression: '{cronExpression}'");

    /// <summary>
    /// Factory method for report execution error
    /// </summary>
    public static InvalidReportException ExecutionFailed(string reason) =>
        new($"Report execution failed: {reason}");
}
