using System;

namespace EHRPlatform.Services.Analytics.Domain.Exceptions;

/// <summary>
/// Exception thrown when KPI validation fails
/// </summary>
public class InvalidKPIException : Exception
{
    public InvalidKPIException(string message) : base(message)
    {
    }

    public InvalidKPIException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }

    public static InvalidKPIException KPINotFound(Guid id)
        => new($"KPI with ID '{id}' not found.");

    public static InvalidKPIException KPINameRequired()
        => new("KPI name is required.");

    public static InvalidKPIException InvalidMetricValue(double value)
        => new($"Invalid metric value: {value}");

    public static InvalidKPIException TargetValueMustBePositive()
        => new("Target value must be a positive number.");
}
