namespace EHRPlatform.Services.Analytics.Domain.Exceptions;

/// <summary>
/// Exception thrown when KPI data is invalid
/// </summary>
public class InvalidKPIException : DomainException
{
    public InvalidKPIException(string message) : base(message)
    {
    }

    public InvalidKPIException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Factory method for KPI not found
    /// </summary>
    public static InvalidKPIException NotFound(Guid kpiId) =>
        new($"KPI '{kpiId}' not found");

    /// <summary>
    /// Factory method for invalid date range
    /// </summary>
    public static InvalidKPIException InvalidDateRange(DateTime startDate, DateTime endDate) =>
        new($"Invalid date range: start date '{startDate:yyyy-MM-dd}' cannot be after end date '{endDate:yyyy-MM-dd}'");

    /// <summary>
    /// Factory method for missing summary data
    /// </summary>
    public static InvalidKPIException NoDataForDate(DateTime date) =>
        new($"No KPI summary data available for date '{date:yyyy-MM-dd}'");
}
