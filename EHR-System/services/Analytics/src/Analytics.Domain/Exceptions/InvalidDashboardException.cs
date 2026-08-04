using System;

namespace EHRPlatform.Services.Analytics.Domain.Exceptions;

/// <summary>
/// Exception thrown when dashboard validation fails
/// </summary>
public class InvalidDashboardException : Exception
{
    public InvalidDashboardException(string message) : base(message)
    {
    }

    public InvalidDashboardException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }

    public static InvalidDashboardException DashboardNotFound(Guid id)
        => new($"Dashboard with ID '{id}' not found.");

    public static InvalidDashboardException DashboardNameRequired()
        => new("Dashboard name is required.");

    public static InvalidDashboardException InvalidDashboardData()
        => new("Invalid dashboard data provided.");
}
