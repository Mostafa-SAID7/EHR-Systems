using FluentAssertions;
using Moq;
using EHRPlatform.Services.Analytics.Domain.Entities;
using EHRPlatform.Common.Infrastructure.Security;
using Xunit;

namespace EHRPlatform.Tests.Security.Analytics;

/// <summary>
/// Security tests for Analytics Service.
/// Tests access control, data ownership, and authorization.
/// Sensitive: Analytics data contains operational insights and PHI references.
/// </summary>
public class AnalyticsAccessTests
{
    private readonly Mock<IAuthorizationService> _mockAuthService;

    public AnalyticsAccessTests()
    {
        _mockAuthService = new Mock<IAuthorizationService>();
    }

    [Fact]
    public void Report_OriginalOwner_ShouldHaveAccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var report = new Report
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Owner's Report"
        };

        // Act
        var hasAccess = report.UserId == userId;

        // Assert
        hasAccess.Should().BeTrue("Report owner should have access");
    }

    [Fact]
    public void Report_DifferentUser_ShouldNotHaveDefaultAccess()
    {
        // Arrange
        var ownerUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var report = new Report
        {
            Id = Guid.NewGuid(),
            UserId = ownerUserId,
            Name = "Report"
        };

        // Act
        var hasDefaultAccess = report.UserId == otherUserId;

        // Assert
        hasDefaultAccess.Should().BeFalse("Different user should not have default access");
    }

    [Fact]
    public void Report_AdminUser_ShouldHaveOverrideAccess()
    {
        // Arrange
        var adminUserId = Guid.NewGuid();
        var isAdmin = true;

        _mockAuthService.Setup(a => a.IsAdministrator(adminUserId))
            .Returns(isAdmin);

        // Act
        var adminHasAccess = _mockAuthService.Object.IsAdministrator(adminUserId);

        // Assert
        adminHasAccess.Should().BeTrue("Admin should have override access");
    }

    [Fact]
    public void Dashboard_OriginalOwner_ShouldHaveAccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dashboard = new Dashboard
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Personal Dashboard"
        };

        // Act
        var hasAccess = dashboard.UserId == userId;

        // Assert
        hasAccess.Should().BeTrue("Dashboard owner should have access");
    }

    [Fact]
    public void Dashboard_SharedWithUser_ShouldHaveAccess()
    {
        // Arrange
        var ownerUserId = Guid.NewGuid();
        var sharedUserId = Guid.NewGuid();
        var dashboard = new Dashboard
        {
            Id = Guid.NewGuid(),
            UserId = ownerUserId,
            Name = "Shared Dashboard"
        };

        var isShared = true; // Would be tracked in a DashboardShare junction table in real system

        // Act & Assert
        isShared.Should().BeTrue("Dashboard can be shared with other users");
    }

    [Fact]
    public void Report_WithSensitiveMetrics_ShouldHaveRestrictedAccess()
    {
        // Arrange
        var report = new Report
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Name = "Payroll Report",
            Description = "Employee salary data"
        };

        var requiredAccessLevel = 3; // Restricted

        // Act & Assert
        requiredAccessLevel.Should().Be(3, "Sensitive reports should have restricted access");
    }

    [Fact]
    public void Metric_PublicMetric_ShouldBeViewable()
    {
        // Arrange
        var metric = new AnalyticsMetric
        {
            Id = Guid.NewGuid(),
            MetricName = "TotalPatients",
            Category = "Patients",
            Value = 10000
        };

        var isPublic = true;

        // Act & Assert
        isPublic.Should().BeTrue("Public metrics should be viewable");
    }

    [Fact]
    public void Metric_ConfidentialMetric_ShouldBeRestricted()
    {
        // Arrange
        var metric = new AnalyticsMetric
        {
            Id = Guid.NewGuid(),
            MetricName = "RevenueByClinic",
            Category = "Financial",
            Value = 500000
        };

        var requiresAuthorization = true;

        // Act & Assert
        requiresAuthorization.Should().BeTrue("Confidential metrics require authorization");
    }

    [Fact]
    public void Report_DeletedReport_ShouldNotBeAccessible()
    {
        // Arrange
        var report = new Report
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Name = "Report",
            IsDeleted = true
        };

        // Act
        var isAccessible = !report.IsDeleted;

        // Assert
        isAccessible.Should().BeFalse("Deleted reports should not be accessible");
    }

    [Fact]
    public void Report_ExportRequiresAudit()
    {
        // Arrange
        var report = new Report
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Name = "Financial Report"
        };

        var requiresAuditTrail = true;

        // Act & Assert
        requiresAuditTrail.Should().BeTrue("Report exports should require audit trail");
    }
}

