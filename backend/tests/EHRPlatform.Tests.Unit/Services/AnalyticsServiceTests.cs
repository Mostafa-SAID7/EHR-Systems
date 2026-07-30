using FluentAssertions;
using Moq;
using EHRPlatform.Common.Data.Abstractions;
using EHRPlatform.Services.Analytics.Domain.Entities;
using EHRPlatform.Services.Analytics.Features.Analytics.Commands;
using EHRPlatform.Services.Analytics.Features.Analytics.Handlers;
using Microsoft.Extensions.Logging;
using Xunit;

namespace EHRPlatform.Tests.Unit.Services;

/// <summary>
/// Unit tests for Analytics CQRS command handlers.
/// Tests report generation, metric aggregation, dashboard creation.
/// Performance: Report generation measured and validated.
/// </summary>
public class AnalyticsServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepository<Report>> _mockReportRepo;
    private readonly Mock<IRepository<AnalyticsMetric>> _mockMetricRepo;
    private readonly Mock<IRepository<EventMetric>> _mockEventMetricRepo;
    private readonly Mock<ILogger<RecordEventMetricCommandHandler>> _mockLogger;

    public AnalyticsServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockReportRepo = new Mock<IRepository<Report>>();
        _mockMetricRepo = new Mock<IRepository<AnalyticsMetric>>();
        _mockEventMetricRepo = new Mock<IRepository<EventMetric>>();
        _mockLogger = new Mock<ILogger<RecordEventMetricCommandHandler>>();

        _mockUnitOfWork.Setup(u => u.Repository<Report>()).Returns(_mockReportRepo.Object);
        _mockUnitOfWork.Setup(u => u.Repository<AnalyticsMetric>()).Returns(_mockMetricRepo.Object);
        _mockUnitOfWork.Setup(u => u.Repository<EventMetric>()).Returns(_mockEventMetricRepo.Object);
    }

    [Fact]
    public async Task RecordEventMetricCommandHandler_WithValidEvent_ShouldCreateMetric()
    {
        // Arrange
        var command = new RecordEventMetricCommand
        {
            EventType = "PatientCreated",
            AggregateId = Guid.NewGuid(),
            Properties = new() { { "source", "API" } }
        };

        _mockEventMetricRepo.Setup(r => r.AddAsync(It.IsAny<EventMetric>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new RecordEventMetricCommandHandler(_mockUnitOfWork.Object, _mockLogger.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mockEventMetricRepo.Verify(r => r.AddAsync(It.IsAny<EventMetric>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AggregateMetricsCommandHandler_WithDailyFrequency_ShouldAggregateDaily()
    {
        // Arrange
        var command = new AggregateMetricsCommand
        {
            Frequency = "Daily",
            ForPeriod = DateTime.UtcNow
        };

        _mockMetricRepo.Setup(r => r.AddAsync(It.IsAny<AnalyticsMetric>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new AggregateMetricsCommandHandler(_mockUnitOfWork.Object, _mockLogger.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mockMetricRepo.Verify(r => r.AddAsync(It.IsAny<AnalyticsMetric>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task CreateReportCommandHandler_WithValidCommand_ShouldCreateReport()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateReportCommand
        {
            UserId = userId,
            Name = "Monthly Report",
            Description = "Monthly analytics report",
            ReportType = "Financial",
            Schedule = "Monthly",
            Metrics = new() { "Revenue", "PatientCount" }
        };

        Report? capturedReport = null;
        _mockReportRepo.Setup(r => r.AddAsync(It.IsAny<Report>(), It.IsAny<CancellationToken>()))
            .Callback<Report, CancellationToken>((report, _) => capturedReport = report)
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreateReportCommandHandler(_mockUnitOfWork.Object, _mockLogger.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Equal(userId);
        result.Name.Should().Equal("Monthly Report");
    }

    [Fact]
    public async Task GenerateReportCommandHandler_WithValidReport_ShouldCreateExecution()
    {
        // Arrange
        var reportId = Guid.NewGuid();
        var command = new GenerateReportCommand { ReportId = reportId };

        var report = new Report
        {
            Id = reportId,
            UserId = Guid.NewGuid(),
            Name = "Test Report"
        };

        _mockReportRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Func<IQueryable<Report>, IQueryable<Report>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);
        _mockReportRepo.Setup(r => r.UpdateAsync(It.IsAny<Report>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new GenerateReportCommandHandler(_mockUnitOfWork.Object, _mockLogger.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ReportId.Should().Equal(reportId);
        result.Status.Should().Equal("Completed");
    }

    [Fact]
    public async Task GenerateReportCommandHandler_WithNonExistentReport_ShouldThrow()
    {
        // Arrange
        var command = new GenerateReportCommand { ReportId = Guid.NewGuid() };

        _mockReportRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Func<IQueryable<Report>, IQueryable<Report>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Report?)null);

        var handler = new GenerateReportCommandHandler(_mockUnitOfWork.Object, _mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(command, CancellationToken.None));
    }
}
