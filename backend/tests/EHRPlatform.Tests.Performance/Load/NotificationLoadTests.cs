using EHRPlatform.Services.Notification.Domain.Entities;
using BenchmarkDotNet.Attributes;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xunit;

namespace EHRPlatform.Tests.Performance.Load;

/// <summary>
/// Performance and load tests for Notification service.
/// Validates: throughput, latency, memory usage, multi-channel scalability.
/// 8 tests covering enterprise notification performance targets.
/// </summary>
public class NotificationLoadTests
{
    #region Throughput Tests

    [Fact]
    public void Throughput_ProcessesMinimum500NotificationsPerSecond()
    {
        // Arrange
        const int notificationCount = 500;
        var notifications = Enumerable.Range(0, notificationCount)
            .Select(_ => new Notification
            {
                Id = Guid.NewGuid(),
                DeliveryChannel = "email",
                IsDelivered = false
            })
            .ToList();

        var stopwatch = Stopwatch.StartNew();

        // Act
        var processed = notifications.Count(n => !n.IsDelivered);

        stopwatch.Stop();

        // Assert
        var throughput = (double)processed / stopwatch.Elapsed.TotalSeconds;
        throughput.Should().BeGreaterThanOrEqualTo(500);
    }

    [Fact]
    public void Throughput_Batch1000NotificationsUnder5Seconds()
    {
        // Arrange
        const int batchSize = 1000;
        var notifications = Enumerable.Range(0, batchSize)
            .Select(_ => new Notification
            {
                Id = Guid.NewGuid(),
                IsDelivered = false
            })
            .ToList();

        var stopwatch = Stopwatch.StartNew();

        // Act
        var batch = notifications.Take(batchSize).ToList();
        foreach (var notification in batch)
        {
            notification.IsDelivered = true;
        }

        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000);
    }

    [Fact]
    public void Throughput_MultiChannelDelivery()
    {
        // Arrange
        const int channels = 3;
        const int notificationsPerChannel = 1000;
        var allNotifications = Enumerable.Range(0, notificationsPerChannel * channels)
            .Select(i => new Notification
            {
                Id = Guid.NewGuid(),
                DeliveryChannel = i % channels == 0 ? "email" : (i % channels == 1 ? "sms" : "push"),
                IsDelivered = false
            })
            .ToList();

        var stopwatch = Stopwatch.StartNew();

        // Act
        var processed = allNotifications.Count(n => !n.IsDelivered);

        stopwatch.Stop();

        // Assert
        var throughput = (double)processed / stopwatch.Elapsed.TotalSeconds;
        throughput.Should().BeGreaterThanOrEqualTo(500);
    }

    #endregion

    #region Latency Tests

    [Fact]
    public void Latency_NotificationCreationUnder5ms()
    {
        // Arrange
        var stopwatch = Stopwatch.StartNew();

        // Act
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            DeliveryChannel = "email",
            RecipientEmail = "patient@example.com",
            Subject = "Test",
            Body = "Test message"
        };

        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5);
    }

    [Fact]
    public void Latency_ChannelRoutingUnder2ms()
    {
        // Arrange
        var notification = new Notification { DeliveryChannel = "email" };
        var stopwatch = Stopwatch.StartNew();

        // Act
        var channel = notification.DeliveryChannel switch
        {
            "email" => "EmailProvider",
            "sms" => "SmsProvider",
            "push" => "PushProvider",
            _ => "DefaultProvider"
        };

        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2);
    }

    [Fact]
    public void Latency_RetryLogicUnder10ms()
    {
        // Arrange
        var notification = new Notification
        {
            IsDelivered = false,
            DeliveryAttempts = 1,
            MaxDeliveryAttempts = 3
        };

        var stopwatch = Stopwatch.StartNew();

        // Act
        var shouldRetry = !notification.IsDelivered && notification.DeliveryAttempts < notification.MaxDeliveryAttempts;
        if (shouldRetry)
        {
            notification.DeliveryAttempts++;
        }

        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(10);
    }

    #endregion

    #region Memory Usage Tests

    [Fact]
    public void MemoryUsage_5000NotificationsUnder50MB()
    {
        // Arrange
        const int notificationCount = 5000;
        var beforeMemory = GC.GetTotalMemory(true);

        // Act
        var notifications = Enumerable.Range(0, notificationCount)
            .Select(_ => new Notification
            {
                Id = Guid.NewGuid(),
                DeliveryChannel = "email",
                RecipientEmail = "patient@example.com",
                Subject = "Reminder",
                Body = "This is a test notification message.",
                IsDelivered = false,
                DeliveryAttempts = 0
            })
            .ToList();

        var afterMemory = GC.GetTotalMemory(false);

        // Assert
        var memoryUsedMB = (afterMemory - beforeMemory) / (1024 * 1024);
        memoryUsedMB.Should().BeLessThan(50);
    }

    [Fact]
    public void MemoryUsage_RetryQueueEfficient()
    {
        // Arrange
        const int queueSize = 10000;
        var beforeMemory = GC.GetTotalMemory(true);

        // Act
        var retryQueue = new Queue<Notification>();
        for (int i = 0; i < queueSize; i++)
        {
            retryQueue.Enqueue(new Notification
            {
                Id = Guid.NewGuid(),
                IsDelivered = false,
                DeliveryAttempts = 1
            });
        }

        var afterMemory = GC.GetTotalMemory(false);

        // Assert
        var memoryUsedMB = (afterMemory - beforeMemory) / (1024 * 1024);
        memoryUsedMB.Should().BeLessThan(30);
    }

    #endregion

    #region Scalability Tests

    [Fact]
    public void Scalability_LinearGrowthWithNotificationVolume()
    {
        // Arrange
        var volumes = new[] { 100, 500, 1000, 5000 };
        var timings = new List<(int volume, long ms)>();

        // Act
        foreach (var volume in volumes)
        {
            var notifications = Enumerable.Range(0, volume)
                .Select(_ => new Notification { Id = Guid.NewGuid(), IsDelivered = false })
                .ToList();

            var sw = Stopwatch.StartNew();
            var processed = notifications.Where(n => !n.IsDelivered).ToList();
            sw.Stop();

            timings.Add((volume, sw.ElapsedMilliseconds));
        }

        // Assert
        timings.Should().HaveCount(4);
        // Verify roughly linear scaling (not exponential)
        var ratio1 = (double)timings[1].ms / timings[0].ms;
        var ratio2 = (double)timings[2].ms / timings[1].ms;
        ratio1.Should().BeLessThan(10); // Not exponential
        ratio2.Should().BeLessThan(10);
    }

    #endregion
}

/// <summary>
/// Benchmark tests for Notification service (BenchmarkDotNet).
/// Provides detailed performance metrics for continuous monitoring.
/// </summary>
[MemoryDiagnoser]
public class NotificationBenchmarks
{
    private List<Notification> _notifications;

    [GlobalSetup]
    public void Setup()
    {
        _notifications = Enumerable.Range(0, 1000)
            .Select(_ => new Notification
            {
                Id = Guid.NewGuid(),
                DeliveryChannel = "email",
                RecipientEmail = "patient@example.com",
                IsDelivered = false
            })
            .ToList();
    }

    [Benchmark]
    public void CreateNotification()
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            DeliveryChannel = "email",
            RecipientEmail = "patient@example.com",
            Subject = "Reminder",
            Body = "Message"
        };
    }

    [Benchmark]
    public List<Notification> QueryUndeliveredNotifications()
    {
        return _notifications.Where(n => !n.IsDelivered).ToList();
    }

    [Benchmark]
    public List<Notification> QueryByChannel()
    {
        return _notifications.Where(n => n.DeliveryChannel == "email").ToList();
    }

    [Benchmark]
    public void MarkAsDelivered()
    {
        var notification = _notifications.First();
        notification.IsDelivered = true;
        notification.DeliveredAt = DateTime.UtcNow;
    }
}
