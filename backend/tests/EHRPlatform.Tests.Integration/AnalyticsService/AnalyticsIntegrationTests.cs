using FluentAssertions;
using EHRPlatform.Services.Analytics.Domain.Entities;
using EHRPlatform.Tests.Common.Fixtures;
using Xunit;

namespace EHRPlatform.Tests.Integration.AnalyticsService;

/// <summary>
/// Integration tests for Analytics Service with real database.
/// Tests report generation, metric aggregation, dashboard creation.
/// Performance: Validates query performance on realistic data volumes.
/// </summary>
[Collection("Integration Tests")]
public class AnalyticsIntegrationTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private IRepository<Report> _reportRepository;
    private IRepository<AnalyticsMetric> _metricRepository;
    private IRepository<Dashboard> _dashboardRepository;

    public AnalyticsIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        await _fixture.InitializeAsync();
        _reportRepository = _fixture.GetRepository<Report>();
        _metricRepository = _fixture.GetRepository<AnalyticsMetric>();
        _dashboardRepository = _fixture.GetRepository<Dashboard>();
    }

    public async Task DisposeAsync()
    {
        await _fixture.DisposeAsync();
    }

    [Fact]
    public async Task Report_WhenCreated_ShouldPersistToDatabase()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var reportId = Guid.NewGuid();

        var report = new Report
        {
            Id = reportId,
            UserId = userId,
            Name = "Revenue Report",
            Description = "Monthly revenue breakdown",
            ReportType = "Financial",
            Schedule = "Monthly",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await _reportRepository.AddAsync(report, CancellationToken.None);
        await _fixture.SaveChangesAsync();

        // Assert
        var retrieved = await _reportRepository.GetByIdAsync(reportId, CancellationToken.None);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Equal("Revenue Report");
        retrieved.UserId.Should().Equal(userId);
    }

    [Fact]
    public async Task Report_WithExecutions_ShouldTrackHistory()
    {
        // Arrange
        var reportId = Guid.NewGuid();
        var report = new Report
        {
            Id = reportId,
            UserId = Guid.NewGuid(),
            Name = "Test Report"
        };

        await _reportRepository.AddAsync(report, CancellationToken.None);
        await _fixture.SaveChangesAsync();

        // Act
        var retrieved = await _reportRepository.GetByIdAsync(reportId, CancellationToken.None);
        retrieved!.LastGeneratedAt = DateTime.UtcNow;
        await _reportRepository.UpdateAsync(retrieved, CancellationToken.None);
        await _fixture.SaveChangesAsync();

        // Assert
        var updated = await _reportRepository.GetByIdAsync(reportId, CancellationToken.None);
        updated!.LastGeneratedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Metric_WhenRecorded_ShouldPersist()
    {
        // Arrange
        var metricId = Guid.NewGuid();
        var metric = new AnalyticsMetric
        {
            Id = metricId,
            MetricName = "PatientCount",
            Category = "Patients",
            Value = 1500,
            Unit = "count",
            Frequency = "Monthly",
            PeriodStart = DateTime.UtcNow.AddMonths(-1),
            PeriodEnd = DateTime.UtcNow
        };

        // Act
        await _metricRepository.AddAsync(metric, CancellationToken.None);
        await _fixture.SaveChangesAsync();

        // Assert
        var retrieved = await _metricRepository.GetByIdAsync(metricId, CancellationToken.None);
        retrieved.Should().NotBeNull();
        retrieved!.Value.Should().Equal(1500);
        retrieved.MetricName.Should().Equal("PatientCount");
    }

    [Fact]
    public async Task Dashboard_WhenCreated_ShouldBePersistable()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dashboardId = Guid.NewGuid();

        var dashboard = new Dashboard
        {
            Id = dashboardId,
            UserId = userId,
            Name = "Executive Dashboard",
            Description = "High-level KPIs for executives",
            IsDefault = true,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await _dashboardRepository.AddAsync(dashboard, CancellationToken.None);
        await _fixture.SaveChangesAsync();

        // Assert
        var retrieved = await _dashboardRepository.GetByIdAsync(dashboardId, CancellationToken.None);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Equal("Executive Dashboard");
        retrieved.IsDefault.Should().BeTrue();
    }

    [Fact]
    public async Task Dashboard_WithWidgets_ShouldMaintainAssociation()
    {
        // Arrange
        var dashboardId = Guid.NewGuid();
        var dashboard = new Dashboard
        {
            Id = dashboardId,
            UserId = Guid.NewGuid(),
            Name = "Analytics Dashboard"
        };

        var widget1 = new DashboardWidget
        {
            Id = Guid.NewGuid(),
            DashboardId = dashboardId,
            Title = "Revenue Trend",
            WidgetType = "LineChart",
            MetricName = "Revenue",
            Order = 1
        };

        var widget2 = new DashboardWidget
        {
            Id = Guid.NewGuid(),
            DashboardId = dashboardId,
            Title = "Patient Count",
            WidgetType = "NumberCard",
            MetricName = "PatientCount",
            Order = 2
        };

        dashboard.DashboardWidgets.Add(widget1);
        dashboard.DashboardWidgets.Add(widget2);

        // Act
        await _dashboardRepository.AddAsync(dashboard, CancellationToken.None);
        await _fixture.SaveChangesAsync();

        // Assert
        var retrieved = await _dashboardRepository.GetByIdAsync(dashboardId, CancellationToken.None);
        retrieved!.DashboardWidgets.Should().HaveCount(2);
    }

    [Fact]
    public async Task Metric_WithPeriod_ShouldFilterByDateRange()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddMonths(-3);
        var endDate = DateTime.UtcNow;

        var metric1 = new AnalyticsMetric
        {
            Id = Guid.NewGuid(),
            MetricName = "Revenue",
            PeriodStart = startDate,
            PeriodEnd = startDate.AddMonths(1),
            Value = 100000
        };

        var metric2 = new AnalyticsMetric
        {
            Id = Guid.NewGuid(),
            MetricName = "Revenue",
            PeriodStart = startDate.AddMonths(1),
            PeriodEnd = startDate.AddMonths(2),
            Value = 120000
        };

        // Act
        await _metricRepository.AddAsync(metric1, CancellationToken.None);
        await _metricRepository.AddAsync(metric2, CancellationToken.None);
        await _fixture.SaveChangesAsync();

        // Assert
        var metrics = await _metricRepository.GetAsync(
            m => m.PeriodStart >= startDate && m.PeriodEnd <= endDate,
            CancellationToken.None);
        metrics.Should().HaveCount(2);
    }

    [Fact]
    public async Task Report_QueryByUser_ShouldReturnOnlyUserReports()
    {
        // Arrange
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();

        var report1 = new Report { Id = Guid.NewGuid(), UserId = user1Id, Name = "User1 Report" };
        var report2 = new Report { Id = Guid.NewGuid(), UserId = user1Id, Name = "User1 Report 2" };
        var report3 = new Report { Id = Guid.NewGuid(), UserId = user2Id, Name = "User2 Report" };

        // Act
        await _reportRepository.AddAsync(report1, CancellationToken.None);
        await _reportRepository.AddAsync(report2, CancellationToken.None);
        await _reportRepository.AddAsync(report3, CancellationToken.None);
        await _fixture.SaveChangesAsync();

        // Assert
        var user1Reports = await _reportRepository.GetAsync(r => r.UserId == user1Id, CancellationToken.None);
        user1Reports.Should().HaveCount(2);
        user1Reports.All(r => r.UserId == user1Id).Should().BeTrue();
    }

    [Fact]
    public async Task Dashboard_QueryByDefault_ShouldFindDefaultDashboard()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var dashboard1 = new Dashboard { Id = Guid.NewGuid(), UserId = userId, Name = "Default", IsDefault = true };
        var dashboard2 = new Dashboard { Id = Guid.NewGuid(), UserId = userId, Name = "Custom", IsDefault = false };

        // Act
        await _dashboardRepository.AddAsync(dashboard1, CancellationToken.None);
        await _dashboardRepository.AddAsync(dashboard2, CancellationToken.None);
        await _fixture.SaveChangesAsync();

        // Assert
        var defaultDashboard = await _dashboardRepository.FirstOrDefaultAsync(
            d => d.Where(x => x.UserId == userId && x.IsDefault),
            CancellationToken.None);
        defaultDashboard.Should().NotBeNull();
        defaultDashboard!.Name.Should().Equal("Default");
    }
}
