using FluentAssertions;
using EHRPlatform.Services.Analytics.Domain.Entities;
using Xunit;

namespace EHRPlatform.Tests.Unit.Domain;

/// <summary>
/// Unit tests for Dashboard aggregate.
/// Tests dashboard lifecycle, widgets, and user ownership.
/// </summary>
public class DashboardTests
{
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public void Dashboard_WhenCreated_ShouldInitializeWithDefaults()
    {
        // Arrange
        var dashboardId = Guid.NewGuid();

        // Act
        var dashboard = new Dashboard
        {
            Id = dashboardId,
            UserId = _userId,
            Name = "Executive Dashboard",
            Description = "KPI overview for leadership",
            IsDefault = true,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        dashboard.Id.Should().Equal(dashboardId);
        dashboard.UserId.Should().Equal(_userId);
        dashboard.Name.Should().Equal("Executive Dashboard");
        dashboard.IsDefault.Should().BeTrue();
    }

    [Fact]
    public void Dashboard_WithWidgets_ShouldMaintainCollection()
    {
        // Arrange
        var dashboard = new Dashboard
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            Name = "Analytics Dashboard"
        };

        // Act
        var widget1 = new DashboardWidget
        {
            Id = Guid.NewGuid(),
            DashboardId = dashboard.Id,
            Title = "Revenue Trend",
            WidgetType = "LineChart",
            MetricName = "Revenue",
            Order = 1
        };

        var widget2 = new DashboardWidget
        {
            Id = Guid.NewGuid(),
            DashboardId = dashboard.Id,
            Title = "Patient Count",
            WidgetType = "NumberCard",
            MetricName = "PatientCount",
            Order = 2
        };

        dashboard.DashboardWidgets.Add(widget1);
        dashboard.DashboardWidgets.Add(widget2);

        // Assert
        dashboard.DashboardWidgets.Should().HaveCount(2);
        dashboard.DashboardWidgets.First().Title.Should().Equal("Revenue Trend");
        dashboard.DashboardWidgets.Last().Title.Should().Equal("Patient Count");
    }

    [Fact]
    public void Dashboard_WithMultipleWidgetTypes_ShouldAccommodate()
    {
        // Arrange
        var dashboard = new Dashboard
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            Name = "Comprehensive Dashboard"
        };

        var widgetTypes = new[] { "LineChart", "BarChart", "PieChart", "NumberCard", "Table", "Gauge" };

        // Act
        foreach (var type in widgetTypes)
        {
            dashboard.DashboardWidgets.Add(new DashboardWidget
            {
                Id = Guid.NewGuid(),
                DashboardId = dashboard.Id,
                WidgetType = type,
                Title = $"{type} Widget"
            });
        }

        // Assert
        dashboard.DashboardWidgets.Should().HaveCount(widgetTypes.Length);
        dashboard.DashboardWidgets.Select(w => w.WidgetType).Should().Equal(widgetTypes);
    }

    [Fact]
    public void Dashboard_UpdatedAt_ShouldTrackModification()
    {
        // Arrange
        var dashboard = new Dashboard
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            Name = "Dashboard",
            UpdatedAt = null
        };

        // Act
        dashboard.UpdatedAt = DateTime.UtcNow;

        // Assert
        dashboard.UpdatedAt.Should().NotBeNull();
        dashboard.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Dashboard_NonDefaultCount_ShouldBeGreater()
    {
        // Arrange
        var dashboard1 = new Dashboard { Id = Guid.NewGuid(), UserId = _userId, Name = "Default", IsDefault = true };
        var dashboard2 = new Dashboard { Id = Guid.NewGuid(), UserId = _userId, Name = "Custom1", IsDefault = false };
        var dashboard3 = new Dashboard { Id = Guid.NewGuid(), UserId = _userId, Name = "Custom2", IsDefault = false };

        var dashboards = new[] { dashboard1, dashboard2, dashboard3 };

        // Act
        var defaultCount = dashboards.Count(d => d.IsDefault);
        var customCount = dashboards.Count(d => !d.IsDefault);

        // Assert
        defaultCount.Should().Equal(1);
        customCount.Should().Equal(2);
    }

    [Fact]
    public void Dashboard_WidgetOrder_ShouldBePreserved()
    {
        // Arrange
        var dashboard = new Dashboard
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            Name = "Ordered Dashboard"
        };

        // Act
        for (int i = 1; i <= 5; i++)
        {
            dashboard.DashboardWidgets.Add(new DashboardWidget
            {
                Id = Guid.NewGuid(),
                DashboardId = dashboard.Id,
                Order = i
            });
        }

        // Assert
        var orderedWidgets = dashboard.DashboardWidgets.OrderBy(w => w.Order).ToList();
        orderedWidgets[0].Order.Should().Equal(1);
        orderedWidgets[4].Order.Should().Equal(5);
    }

    [Fact]
    public void Widget_WithMetricBinding_ShouldReference()
    {
        // Arrange
        var widget = new DashboardWidget
        {
            Id = Guid.NewGuid(),
            DashboardId = Guid.NewGuid(),
            Title = "Patient Count",
            MetricName = "PatientCount"
        };

        // Act & Assert
        widget.MetricName.Should().Equal("PatientCount");
    }

    [Fact]
    public void Widget_WithConfig_ShouldStoreSettings()
    {
        // Arrange
        var config = @"{""colors"": [""#FF5733"", ""#33FF57""], ""threshold"": 1000}";
        var widget = new DashboardWidget
        {
            Id = Guid.NewGuid(),
            DashboardId = Guid.NewGuid(),
            Config = config
        };

        // Act & Assert
        widget.Config.Should().Equal(config);
    }

    [Fact]
    public void Dashboard_WhenCloned_ShouldCreateNewId()
    {
        // Arrange
        var original = new Dashboard
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            Name = "Original Dashboard"
        };

        // Act
        var clone = new Dashboard
        {
            Id = Guid.NewGuid(),
            UserId = original.UserId,
            Name = $"{original.Name} (Copy)"
        };

        // Assert
        clone.Id.Should().NotEqual(original.Id);
        clone.UserId.Should().Equal(original.UserId);
    }
}
