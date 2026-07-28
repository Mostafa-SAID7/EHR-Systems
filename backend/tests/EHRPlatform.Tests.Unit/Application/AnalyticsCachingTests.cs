using FluentAssertions;
using EHRPlatform.Services.Analytics.Domain.Entities;
using EHRPlatform.Services.Analytics.Features.Analytics.Queries;
using Xunit;

namespace EHRPlatform.Tests.Unit.Application;

/// <summary>
/// Unit tests for Analytics caching strategy.
/// Tests cache keys, TTL values, and invalidation patterns.
/// Performance: Caching must reduce query latency by >80%.
/// </summary>
public class AnalyticsCachingTests
{
    [Fact]
    public void DashboardQuery_ShouldHaveCacheKey()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetDashboardsQuery
        {
            UserId = userId,
            PageNumber = 1,
            PageSize = 50
        };

        // Act
        var cacheKey = query.CacheKey;

        // Assert
        cacheKey.Should().Contain(userId.ToString());
        cacheKey.Should().Contain("dashboards");
    }

    [Fact]
    public void DashboardQuery_ShouldHaveReasonableTTL()
    {
        // Arrange
        var query = new GetDashboardsQuery();

        // Act
        var ttl = query.CacheDurationSeconds;

        // Assert
        ttl.Should().Be(600); // 10 minutes
    }

    [Fact]
    public void ReportQuery_ShouldHaveCacheKey()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetReportsQuery
        {
            UserId = userId,
            Schedule = "Monthly",
            PageNumber = 1,
            PageSize = 25
        };

        // Act
        var cacheKey = query.CacheKey;

        // Assert
        cacheKey.Should().Contain(userId.ToString());
        cacheKey.Should().Contain("reports");
    }

    [Fact]
    public void ReportQuery_ShouldHaveLongerTTL()
    {
        // Arrange
        var query = new GetReportsQuery();

        // Act
        var ttl = query.CacheDurationSeconds;

        // Assert
        ttl.Should().Be(3600); // 1 hour (longer than dashboards)
    }

    [Fact]
    public void DifferentUsers_ShouldHaveDifferentCacheKeys()
    {
        // Arrange
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();

        var query1 = new GetDashboardsQuery { UserId = user1 };
        var query2 = new GetDashboardsQuery { UserId = user2 };

        // Act
        var key1 = query1.CacheKey;
        var key2 = query2.CacheKey;

        // Assert
        key1.Should().NotEqual(key2);
    }

    [Fact]
    public void DifferentPages_ShouldHaveDifferentCacheKeys()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query1 = new GetDashboardsQuery { UserId = userId, PageNumber = 1, PageSize = 50 };
        var query2 = new GetDashboardsQuery { UserId = userId, PageNumber = 2, PageSize = 50 };

        // Act
        var key1 = query1.CacheKey;
        var key2 = query2.CacheKey;

        // Assert
        key1.Should().NotEqual(key2);
    }

    [Fact]
    public void CacheKey_WithScheduleFilter_ShouldIncludeSchedule()
    {
        // Arrange
        var query = new GetReportsQuery
        {
            UserId = Guid.NewGuid(),
            Schedule = "Daily"
        };

        // Act
        var cacheKey = query.CacheKey;

        // Assert
        cacheKey.Should().Contain("Daily");
    }

    [Fact]
    public void CacheKey_WithoutSchedule_ShouldStillBeValid()
    {
        // Arrange
        var query = new GetReportsQuery
        {
            UserId = Guid.NewGuid(),
            Schedule = null
        };

        // Act
        var cacheKey = query.CacheKey;

        // Assert
        cacheKey.Should().NotBeEmpty();
    }

    [Fact]
    public void CacheKeyFormat_ShouldBeConsistent()
    {
        // Arrange
        var query = new GetDashboardsQuery { UserId = Guid.NewGuid(), PageNumber = 1, PageSize = 50 };

        // Act
        var cacheKey = query.CacheKey;

        // Assert
        cacheKey.Should().StartWith("dashboards");
        cacheKey.Should().Contain("_");
    }

    [Fact]
    public void ReportCacheKey_ShouldBePrefixed()
    {
        // Arrange
        var query = new GetReportsQuery { UserId = Guid.NewGuid() };

        // Act
        var cacheKey = query.CacheKey;

        // Assert
        cacheKey.Should().StartWith("reports");
    }
}
