using FluentAssertions;
using FluentValidation;
using EHRPlatform.Services.Analytics.Features.Analytics.Commands;
using EHRPlatform.Services.Analytics.Features.Analytics.Queries;
using EHRPlatform.Services.Analytics.Features.Analytics.Validation;
using Xunit;

namespace EHRPlatform.Tests.Unit.Application;

/// <summary>
/// Unit tests for Analytics validators.
/// Tests input validation for reports, dashboards, and queries.
/// Performance: Validators must execute <1ms per request.
/// </summary>
public class AnalyticsValidatorTests
{
    [Fact]
    public async Task CreateReportValidator_WithValidData_ShouldPass()
    {
        // Arrange
        var validator = new CreateReportValidator();
        var command = new CreateReportCommand
        {
            UserId = Guid.NewGuid(),
            Name = "Monthly Revenue Report",
            Description = "Revenue breakdown by department",
            ReportType = "Financial",
            Schedule = "Monthly",
            Metrics = new() { "TotalRevenue", "PatientCount" }
        };

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task CreateReportValidator_WithoutName_ShouldFail()
    {
        // Arrange
        var validator = new CreateReportValidator();
        var command = new CreateReportCommand
        {
            UserId = Guid.NewGuid(),
            Name = "",
            ReportType = "Financial"
        };

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Name");
    }

    [Fact]
    public async Task CreateReportValidator_WithoutUserId_ShouldFail()
    {
        // Arrange
        var validator = new CreateReportValidator();
        var command = new CreateReportCommand
        {
            UserId = Guid.Empty,
            Name = "Report",
            ReportType = "Financial"
        };

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "UserId");
    }

    [Fact]
    public async Task CreateReportValidator_WithTooLongName_ShouldFail()
    {
        // Arrange
        var validator = new CreateReportValidator();
        var command = new CreateReportCommand
        {
            UserId = Guid.NewGuid(),
            Name = new string('R', 201), // Exceeds 200 char limit
            ReportType = "Financial"
        };

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Name");
    }

    [Fact]
    public async Task GetReportsValidator_WithValidPaging_ShouldPass()
    {
        // Arrange
        var validator = new GetReportsValidator();
        var query = new GetReportsQuery
        {
            UserId = Guid.NewGuid(),
            Schedule = "Monthly",
            PageNumber = 1,
            PageSize = 50
        };

        // Act
        var result = await validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task GetReportsValidator_WithInvalidPageNumber_ShouldFail()
    {
        // Arrange
        var validator = new GetReportsValidator();
        var query = new GetReportsQuery
        {
            PageNumber = 0, // Invalid: must be > 0
            PageSize = 50
        };

        // Act
        var result = await validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "PageNumber");
    }

    [Fact]
    public async Task GetReportsValidator_WithTooLargePageSize_ShouldFail()
    {
        // Arrange
        var validator = new GetReportsValidator();
        var query = new GetReportsQuery
        {
            PageNumber = 1,
            PageSize = 1001 // Exceeds 1000 limit
        };

        // Act
        var result = await validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "PageSize");
    }

    [Fact]
    public async Task GetDashboardsValidator_WithValidPaging_ShouldPass()
    {
        // Arrange
        var validator = new GetDashboardsValidator();
        var query = new GetDashboardsQuery
        {
            UserId = Guid.NewGuid(),
            PageNumber = 2,
            PageSize = 25
        };

        // Act
        var result = await validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0, 50)]
    [InlineData(1, 0)]
    [InlineData(1, 1001)]
    public async Task GetDashboardsValidator_WithInvalidPaging_ShouldFail(int pageNumber, int pageSize)
    {
        // Arrange
        var validator = new GetDashboardsValidator();
        var query = new GetDashboardsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        // Act
        var result = await validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}
