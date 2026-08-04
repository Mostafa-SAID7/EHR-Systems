namespace EHRPlatform.Services.Analytics.Domain.Services;

using EHRPlatform.Services.Analytics.Domain.Entities;
using EHRPlatform.Services.Analytics.Domain.Enums;
using EHRPlatform.Services.Analytics.Domain.Exceptions;

/// <summary>
/// Factory service for creating Report aggregates
/// Ensures consistent creation and validation
/// </summary>
public class ReportFactory
{
    /// <summary>
    /// Creates new on-demand report
    /// </summary>
    public Report CreateOnDemandReport(
        string name,
        string description,
        string reportType,
        string queryDefinition,
        Guid createdBy,
        long tenantId)
    {
        ValidateReportInput(name, description, queryDefinition);

        return new Report
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            ReportType = reportType,
            QueryDefinition = queryDefinition,
            Status = ReportStatus.Active,
            CreatedBy = createdBy,
            TenantId = tenantId,
            IsScheduled = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates new scheduled report
    /// </summary>
    public Report CreateScheduledReport(
        string name,
        string description,
        string reportType,
        string queryDefinition,
        string scheduleCron,
        Guid createdBy,
        long tenantId)
    {
        ValidateReportInput(name, description, queryDefinition);
        ValidateCronExpression(scheduleCron);

        return new Report
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            ReportType = reportType,
            QueryDefinition = queryDefinition,
            Status = ReportStatus.Active,
            CreatedBy = createdBy,
            TenantId = tenantId,
            IsScheduled = true,
            ScheduleCron = scheduleCron,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Validates report input parameters
    /// </summary>
    private void ValidateReportInput(string name, string description, string queryDefinition)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Report name is required", nameof(name));

        if (name.Length > 200)
            throw new ArgumentException("Report name cannot exceed 200 characters", nameof(name));

        if (description.Length > 1000)
            throw new ArgumentException("Report description cannot exceed 1000 characters", nameof(description));

        if (string.IsNullOrWhiteSpace(queryDefinition))
            throw InvalidReportException.InvalidQueryDefinition("Query definition is required");
    }

    /// <summary>
    /// Validates cron expression format (basic validation)
    /// </summary>
    private void ValidateCronExpression(string cron)
    {
        if (string.IsNullOrWhiteSpace(cron))
            throw InvalidReportException.InvalidCronExpression("Cron expression is required");

        var parts = cron.Split(' ');
        if (parts.Length < 5 || parts.Length > 6)
            throw InvalidReportException.InvalidCronExpression("Invalid format");
    }
}
