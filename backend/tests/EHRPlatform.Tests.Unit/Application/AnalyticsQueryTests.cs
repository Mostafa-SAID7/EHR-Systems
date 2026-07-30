using FluentAssertions;
using Moq;
using EHRPlatform.Common.Data.Abstractions;
using EHRPlatform.Common.Shared.DTOs;
using EHRPlatform.Services.Analytics.Domain.Entities;
using EHRPlatform.Services.Analytics.Features.Analytics.Queries;
using EHRPlatform.Services.Analytics.Features.Analytics.Handlers;
using EHRPlatform.Services.Analytics.Application.Analytics.Mappers;
using Microsoft.Extensions.Logging;
using Xunit;

namespace EHRPlatform.Tests.Unit.Application;

/// <summary>
/// Unit tests for Analytics query handlers.
/// Tests pagination, filtering, and result mapping.
/// Performance: Queries must execute <100ms even with pagination.
/// </summary>
public class AnalyticsQueryTests
{
    private readonly Mock<IRepository<Report>> _mockReportRepo;
    private readonly Mock<IRepository<Dashboard>> _mockDashboardRepo;
    private readonly AnalyticsMapper _mapper;
    private readonly Mock<ILogger<GetReportsQueryHandler>> _mockLogger;

    public AnalyticsQueryTests()
    {
        _mockReportRepo = new Mock<IRepository<Report>>();
        _mockDashboardRepo = new Mock<IRepository<Dashboard>>();
        _mapper = new AnalyticsMapper();
        _mockLogger = new Mock<ILogger<GetReportsQueryHandler>>();
    }

    [Fact]
    public async Task GetReportsQuery_WithValidPagination_ShouldReturnPagedResults()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var reports = Enumerable.Range(1, 100)
            .Select(i => new Report
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = $"Report {i}",
                Schedule = "Monthly"
            })
            .ToList();

        var query = new GetReportsQuery
        {
            UserId = userId,
            PageNumber = 1,
            PageSize = 25
        };

        _mockReportRepo.Setup(r => r.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Report, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(reports.Where(r => r.UserId == userId).Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize).ToList());

        var handler = new GetReportsQueryHandler(_mapper, _mockLogger.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(25);
    }

    [Fact]
    public async Task GetReportsQuery_Page2_ShouldReturnSecondPage()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var reports = Enumerable.Range(1, 100)
            .Select(i => new Report { Id = Guid.NewGuid(), UserId = userId, Name = $"Report {i}" })
            .ToList();

        var query = new GetReportsQuery { UserId = userId, PageNumber = 2, PageSize = 25 };

        var pageItems = reports.Skip(25).Take(25).ToList();
        _mockReportRepo.Setup(r => r.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Report, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pageItems);

        var handler = new GetReportsQueryHandler(_mapper, _mockLogger.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.PageNumber.Should().Equal(2);
    }

    [Fact]
    public async Task GetReportsQuery_FilterBySchedule_ShouldReturnFiltered()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var reports = new List<Report>
        {
            new() { Id = Guid.NewGuid(), UserId = userId, Name = "Daily Report", Schedule = "Daily" },
            new() { Id = Guid.NewGuid(), UserId = userId, Name = "Weekly Report", Schedule = "Weekly" },
            new() { Id = Guid.NewGuid(), UserId = userId, Name = "Monthly Report", Schedule = "Monthly" }
        };

        var query = new GetReportsQuery
        {
            UserId = userId,
            Schedule = "Monthly",
            PageNumber = 1,
            PageSize = 50
        };

        var filtered = reports.Where(r => r.Schedule == "Monthly").ToList();
        _mockReportRepo.Setup(r => r.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Report, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(filtered);

        var handler = new GetReportsQueryHandler(_mapper, _mockLogger.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetDashboardsQuery_WithValidPagination_ShouldReturnDashboards()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dashboards = Enumerable.Range(1, 50)
            .Select(i => new Dashboard
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = $"Dashboard {i}",
                IsDefault = i == 1
            })
            .ToList();

        var query = new GetDashboardsQuery
        {
            UserId = userId,
            PageNumber = 1,
            PageSize = 20
        };

        _mockDashboardRepo.Setup(r => r.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Dashboard, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(dashboards.Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize).ToList());

        var handler = new GetDashboardsQueryHandler(_mapper, _mockLogger.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetDashboardsQuery_FindDefault_ShouldReturnDefault()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var defaultDashboard = new Dashboard
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Default Dashboard",
            IsDefault = true
        };

        var query = new GetDashboardsQuery { UserId = userId, PageNumber = 1, PageSize = 50 };

        _mockDashboardRepo.Setup(r => r.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Dashboard, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Dashboard> { defaultDashboard });

        var handler = new GetDashboardsQueryHandler(_mapper, _mockLogger.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().IsDefault.Should().BeTrue();
    }
}

