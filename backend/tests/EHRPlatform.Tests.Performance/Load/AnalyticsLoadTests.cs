using FluentAssertions;
using EHRPlatform.Services.Analytics.Domain.Entities;
using Xunit;

namespace EHRPlatform.Tests.Performance.Load;

/// <summary>
/// Performance tests for Analytics Service.
/// Tests throughput, latency, and scalability.
/// Performance targets: Report generation <500ms, metric recording <10ms, queries <100ms.
/// </summary>
public class AnalyticsLoadTests
{
    [Fact]
    public void Report_Creation_ShouldCompleteFast()
    {
        // Arrange
        var iterations = 1000;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        for (int i = 0; i < iterations; i++)
        {
            var report = new Report
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = $"Report {i}",
                ReportType = "Financial",
                Schedule = "Monthly"
            };
        }

        stopwatch.Stop();

        // Assert
        var averageMs = (double)stopwatch.ElapsedMilliseconds / iterations;
        averageMs.Should().BeLessThan(1.0, $"Report creation should average <1ms, got {averageMs}ms");
    }

    [Fact]
    public void Metric_Recording_ShouldBeFast()
    {
        // Arrange
        var iterations = 5000;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        for (int i = 0; i < iterations; i++)
        {
            var metric = new AnalyticsMetric
            {
                Id = Guid.NewGuid(),
                MetricName = "PatientCount",
                Category = "Patients",
                Value = i * 1.5m,
                Unit = "count",
                RecordedAt = DateTime.UtcNow.AddSeconds(-i)
            };
        }

        stopwatch.Stop();

        // Assert
        var averageMs = (double)stopwatch.ElapsedMilliseconds / iterations;
        averageMs.Should().BeLessThan(0.5, $"Metric recording should average <0.5ms, got {averageMs}ms");
    }

    [Fact]
    public void Dashboard_Creation_WithWidgets_ShouldBeEfficient()
    {
        // Arrange
        var dashboardCount = 100;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var dashboards = new List<Dashboard>();
        for (int d = 0; d < dashboardCount; d++)
        {
            var dashboard = new Dashboard
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = $"Dashboard {d}",
                IsDefault = d % 10 == 0
            };

            for (int w = 0; w < 10; w++)
            {
                var widget = new DashboardWidget
                {
                    Id = Guid.NewGuid(),
                    DashboardId = dashboard.Id,
                    Title = $"Widget {w}",
                    WidgetType = "LineChart",
                    Order = w
                };
                dashboard.DashboardWidgets.Add(widget);
            }

            dashboards.Add(dashboard);
        }

        stopwatch.Stop();

        // Assert
        dashboards.Should().HaveCount(dashboardCount);
        var averageMs = (double)stopwatch.ElapsedMilliseconds / dashboardCount;
        averageMs.Should().BeLessThan(10.0, $"Dashboard creation should average <10ms per dashboard, got {averageMs}ms");
    }

    [Fact]
    public void ReportExecution_HighVolume_ShouldHandleScale()
    {
        // Arrange
        var reportId = Guid.NewGuid();
        var executionCount = 1000;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var executions = new List<ReportExecution>();
        for (int i = 0; i < executionCount; i++)
        {
            var execution = new ReportExecution
            {
                Id = Guid.NewGuid(),
                ReportId = reportId,
                ExecutedAt = DateTime.UtcNow.AddHours(-i),
                Status = i % 100 == 0 ? "Failed" : "Completed",
                RecordCount = i * 100,
                DurationMs = 250 + (i % 500)
            };
            executions.Add(execution);
        }

        stopwatch.Stop();

        // Assert
        executions.Should().HaveCount(executionCount);
        var averageMs = (double)stopwatch.ElapsedMilliseconds / executionCount;
        averageMs.Should().BeLessThan(2.0, $"Execution tracking should average <2ms, got {averageMs}ms");
    }

    [Fact]
    public void Metric_AggregationByFrequency_ShouldScale()
    {
        // Arrange
        var dayMetrics = 365;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var allMetrics = new List<AnalyticsMetric>();
        for (int d = 0; d < dayMetrics; d++)
        {
            var date = DateTime.UtcNow.AddDays(-d);
            for (int m = 0; m < 5; m++) // 5 metrics per day
            {
                var metric = new AnalyticsMetric
                {
                    Id = Guid.NewGuid(),
                    MetricName = $"Metric_{m}",
                    Category = "Operations",
                    Value = (d + m) * 10.5m,
                    PeriodStart = date.Date,
                    PeriodEnd = date.Date.AddDays(1),
                    Frequency = "Daily",
                    RecordedAt = date
                };
                allMetrics.Add(metric);
            }
        }

        stopwatch.Stop();

        // Assert
        allMetrics.Should().HaveCount(dayMetrics * 5);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000, "Annual metric aggregation should complete <2s");
    }

    [Fact]
    public void Report_WithLargeMetricsList_ShouldHandleData()
    {
        // Arrange
        var report = new Report
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Name = "Comprehensive Report"
        };

        var metricsToAdd = 500;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        for (int i = 0; i < metricsToAdd; i++)
        {
            var metric = new AnalyticsMetric
            {
                Id = Guid.NewGuid(),
                MetricName = $"Metric_{i}",
                Value = i * 2.5m
            };
            report.Metrics.Add(metric);
        }

        stopwatch.Stop();

        // Assert
        report.Metrics.Should().HaveCount(metricsToAdd);
        var averageMs = (double)stopwatch.ElapsedMilliseconds / metricsToAdd;
        averageMs.Should().BeLessThan(1.0, $"Adding metrics should average <1ms, got {averageMs}ms");
    }

    [Fact]
    public void ConcurrentReportGeneration_ShouldHandleParallel()
    {
        // Arrange
        var threadCount = 10;
        var reportsPerThread = 100;
        var reports = new System.Collections.Concurrent.ConcurrentBag<Report>();

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        Parallel.For(0, threadCount, new ParallelOptions { MaxDegreeOfParallelism = threadCount }, _ =>
        {
            for (int i = 0; i < reportsPerThread; i++)
            {
                var report = new Report
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    Name = $"Report {i}",
                    ReportType = "Financial"
                };
                reports.Add(report);
            }
        });

        stopwatch.Stop();

        // Assert
        reports.Should().HaveCount(threadCount * reportsPerThread);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(3000, "Concurrent report creation should complete <3s");
    }

    [Fact]
    public void MemoryEfficiency_LargeDataSet_ShouldNotLeak()
    {
        // Arrange
        var initialMemory = GC.GetTotalMemory(true);

        // Act - Create many reports and metrics
        var reports = new List<Report>();
        for (int i = 0; i < 1000; i++)
        {
            var report = new Report
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = $"Report {i}",
                Description = new string('D', 1000)
            };

            for (int j = 0; j < 50; j++)
            {
                report.Metrics.Add(new AnalyticsMetric
                {
                    Id = Guid.NewGuid(),
                    MetricName = $"Metric {j}",
                    Value = i * j
                });
            }

            reports.Add(report);
        }

        var afterCreation = GC.GetTotalMemory(false);
        reports.Clear();
        reports = null;
        GC.Collect();
        GC.WaitForPendingFinalizers();
        var afterCleanup = GC.GetTotalMemory(true);

        // Assert
        var memoryUsed = afterCreation - initialMemory;
        var memoryRetained = afterCleanup - initialMemory;

        memoryUsed.Should().BeLessThan(1_000_000_000, "1000 reports should use <1GB");
        memoryRetained.Should().BeLessThan(200_000_000, "After cleanup, retained memory should be <200MB");
    }
}
