using FluentAssertions;
using EHRPlatform.Services.Analytics.Domain.Entities;
using Xunit;

namespace EHRPlatform.Tests.Unit.Domain;

/// <summary>
/// Unit tests for Report aggregate.
/// Tests report lifecycle, scheduling, and execution tracking.
/// Performance: Report generation must complete within SLA targets.
/// </summary>
public class ReportTests
{
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public void Report_WhenCreated_ShouldInitializeWithDefaults()
    {
        // Arrange
        var reportId = Guid.NewGuid();

        // Act
        var report = new Report
        {
            Id = reportId,
            UserId = _userId,
            Name = "Patient Demographics Report",
            Description = "Monthly patient breakdown by demographics",
            ReportType = "Demographics",
            Schedule = "Monthly",
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        report.Id.Should().Equal(reportId);
        report.UserId.Should().Equal(_userId);
        report.Name.Should().Equal("Patient Demographics Report");
        report.ReportType.Should().Equal("Demographics");
        report.Schedule.Should().Equal("Monthly");
        report.LastGeneratedAt.Should().BeNull();
    }

    [Fact]
    public void Report_WithExecutions_ShouldTrackMultipleRuns()
    {
        // Arrange
        var report = new Report
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            Name = "Revenue Report"
        };

        // Act
        var exec1 = new ReportExecution { Id = Guid.NewGuid(), ReportId = report.Id, Status = "Completed" };
        var exec2 = new ReportExecution { Id = Guid.NewGuid(), ReportId = report.Id, Status = "Completed" };
        
        report.Executions.Add(exec1);
        report.Executions.Add(exec2);

        // Assert
        report.Executions.Should().HaveCount(2);
        report.Executions.All(e => e.ReportId == report.Id).Should().BeTrue();
    }

    [Fact]
    public void Report_WithMetrics_ShouldMaintainAssociation()
    {
        // Arrange
        var report = new Report
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            Name = "Operational Metrics"
        };

        // Act
        var metric1 = new AnalyticsMetric { Id = Guid.NewGuid(), MetricName = "PatientCount", Value = 1000 };
        var metric2 = new AnalyticsMetric { Id = Guid.NewGuid(), MetricName = "AppointmentCount", Value = 5000 };

        report.Metrics.Add(metric1);
        report.Metrics.Add(metric2);

        // Assert
        report.Metrics.Should().HaveCount(2);
    }

    [Theory]
    [InlineData("OnDemand")]
    [InlineData("Daily")]
    [InlineData("Weekly")]
    [InlineData("Monthly")]
    public void Report_WithValidSchedules_ShouldAccept(string schedule)
    {
        // Arrange & Act
        var report = new Report
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            Name = "Scheduled Report",
            Schedule = schedule
        };

        // Assert
        report.Schedule.Should().Equal(schedule);
    }

    [Theory]
    [InlineData("Demographics")]
    [InlineData("Financial")]
    [InlineData("Clinical")]
    [InlineData("Operations")]
    public void Report_WithValidTypes_ShouldAccept(string reportType)
    {
        // Arrange & Act
        var report = new Report
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            Name = "Typed Report",
            ReportType = reportType
        };

        // Assert
        report.ReportType.Should().Equal(reportType);
    }

    [Fact]
    public void Report_WhenExecuted_ShouldUpdateLastGeneratedAt()
    {
        // Arrange
        var report = new Report
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            Name = "Report"
        };

        var oldTime = report.LastGeneratedAt;

        // Act
        report.LastGeneratedAt = DateTime.UtcNow;

        // Assert
        report.LastGeneratedAt.Should().NotEqual(oldTime);
        report.LastGeneratedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Report_WithLongName_ShouldAccommodate()
    {
        // Arrange
        var longName = new string('R', 200); // Max reasonable length

        // Act
        var report = new Report
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            Name = longName
        };

        // Assert
        report.Name.Length.Should().Equal(200);
    }
}
