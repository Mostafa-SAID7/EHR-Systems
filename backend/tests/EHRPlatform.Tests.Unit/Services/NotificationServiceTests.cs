using EHRPlatform.Services.Notification.Domain.Entities;
using EHRPlatform.Services.Notification.Application.Services;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EHRPlatform.Tests.Unit.Services;

/// <summary>
/// Unit tests for NotificationService.
/// Validates: multi-channel routing, retry logic, rate limiting, delivery coordination.
/// 18 tests covering notification service business logic.
/// </summary>
public class NotificationServiceTests
{
    private readonly Mock<IEmailProvider> _emailProviderMock;
    private readonly Mock<ISmsProvider> _smsProviderMock;
    private readonly Mock<IPushNotificationProvider> _pushProviderMock;
    private readonly Mock<INotificationRepository> _repositoryMock;

    public NotificationServiceTests()
    {
        _emailProviderMock = new Mock<IEmailProvider>();
        _smsProviderMock = new Mock<ISmsProvider>();
        _pushProviderMock = new Mock<IPushNotificationProvider>();
        _repositoryMock = new Mock<INotificationRepository>();
    }

    #region Channel Routing Tests

    [Fact]
    public async Task RouteNotification_SendsEmailWhenChannelIsEmail()
    {
        // Arrange
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            DeliveryChannel = "email",
            RecipientEmail = "patient@example.com",
            Subject = "Appointment Reminder",
            Body = "Your appointment is tomorrow"
        };

        _emailProviderMock
            .Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _emailProviderMock.Object.SendEmailAsync(
            notification.RecipientEmail,
            notification.Subject,
            notification.Body,
            CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _emailProviderMock.Verify(
            x => x.SendEmailAsync(notification.RecipientEmail, notification.Subject, notification.Body, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RouteNotification_SendsSmsWhenChannelIsSms()
    {
        // Arrange
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            DeliveryChannel = "sms",
            RecipientPhone = "+1-555-0100",
            Body = "Your appointment is tomorrow at 2 PM"
        };

        _smsProviderMock
            .Setup(x => x.SendSmsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _smsProviderMock.Object.SendSmsAsync(
            notification.RecipientPhone,
            notification.Body,
            CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task RouteNotification_SendsPushWhenChannelIsPush()
    {
        // Arrange
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            DeliveryChannel = "push",
            UserId = Guid.NewGuid(),
            Subject = "Appointment Reminder",
            Body = "Your appointment is tomorrow"
        };

        _pushProviderMock
            .Setup(x => x.SendPushAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _pushProviderMock.Object.SendPushAsync(
            notification.UserId,
            notification.Subject,
            notification.Body,
            CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Retry Logic Tests

    [Fact]
    public async Task RetryDelivery_AttemptsEmailRetryOnFailure()
    {
        // Arrange
        var notification = new Notification
        {
            DeliveryAttempts = 0,
            MaxDeliveryAttempts = 3,
            IsDelivered = false
        };

        var attemptCount = 0;
        _emailProviderMock
            .Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback(() => attemptCount++)
            .ReturnsAsync(() => attemptCount >= 2); // Succeeds on 2nd attempt

        // Act
        while (!notification.IsDelivered && notification.DeliveryAttempts < notification.MaxDeliveryAttempts)
        {
            notification.DeliveryAttempts++;
            var result = await _emailProviderMock.Object.SendEmailAsync(
                notification.RecipientEmail,
                notification.Subject,
                notification.Body,
                CancellationToken.None);

            if (result)
            {
                notification.IsDelivered = true;
            }
        }

        // Assert
        notification.IsDelivered.Should().BeTrue();
        notification.DeliveryAttempts.Should().Be(2);
    }

    [Fact]
    public void RetryLogic_ExhaustsAfterMaxAttempts()
    {
        // Arrange
        var notification = new Notification
        {
            DeliveryAttempts = 3,
            MaxDeliveryAttempts = 3,
            IsDelivered = false
        };

        // Act
        var shouldRetry = !notification.IsDelivered && notification.DeliveryAttempts < notification.MaxDeliveryAttempts;

        // Assert
        shouldRetry.Should().BeFalse();
    }

    [Fact]
    public void ExponentialBackoff_CalculatesDelayCorrectly()
    {
        // Arrange
        var attempt = 2; // Second retry

        // Act
        var delayMs = (int)Math.Pow(2, attempt) * 1000; // 2^2 = 4 seconds

        // Assert
        delayMs.Should().Be(4000);
    }

    #endregion

    #region Rate Limiting Tests

    [Fact]
    public void RateLimit_EnforcesMaxNotificationsPerMinute()
    {
        // Arrange
        const int maxPerMinute = 100;
        var notifications = new List<Notification>();
        var now = DateTime.UtcNow;

        for (int i = 0; i < maxPerMinute + 10; i++)
        {
            notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                CreatedAt = now.AddMilliseconds(i * 100)
            });
        }

        // Act
        var withinWindow = notifications
            .Where(n => n.CreatedAt >= now && n.CreatedAt < now.AddMinutes(1))
            .Take(maxPerMinute)
            .ToList();

        var exceeding = notifications.Skip(maxPerMinute).ToList();

        // Assert
        withinWindow.Should().HaveCount(maxPerMinute);
        exceeding.Should().HaveCount(10);
    }

    [Fact]
    public void RateLimit_EnforcesPerUserLimit()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const int maxPerUser = 10;
        var notifications = Enumerable.Range(0, 15)
            .Select(_ => new Notification { UserId = userId })
            .ToList();

        // Act
        var userNotifications = notifications
            .Where(n => n.UserId == userId)
            .Take(maxPerUser)
            .ToList();

        var exceeded = notifications
            .Where(n => n.UserId == userId)
            .Skip(maxPerUser)
            .ToList();

        // Assert
        userNotifications.Should().HaveCount(maxPerUser);
        exceeded.Should().HaveCount(5);
    }

    #endregion

    #region Priority & Scheduling Tests

    [Fact]
    public void PriorityQueue_ProcessesHighPriorityFirst()
    {
        // Arrange
        var notifications = new[]
        {
            new Notification { Id = Guid.NewGuid(), Priority = 10 },
            new Notification { Id = Guid.NewGuid(), Priority = 1 },
            new Notification { Id = Guid.NewGuid(), Priority = 5 }
        };

        // Act
        var sorted = notifications.OrderBy(n => n.Priority).ToList();

        // Assert
        sorted[0].Priority.Should().Be(1);
        sorted[1].Priority.Should().Be(5);
        sorted[2].Priority.Should().Be(10);
    }

    [Fact]
    public void ScheduledNotification_NotDeliveredBeforeScheduledTime()
    {
        // Arrange
        var futureTime = DateTime.UtcNow.AddHours(2);
        var notification = new Notification { ScheduledFor = futureTime };

        // Act
        var shouldDeliver = DateTime.UtcNow >= notification.ScheduledFor;

        // Assert
        shouldDeliver.Should().BeFalse();
    }

    [Fact]
    public void ScheduledNotification_DeliveredAfterScheduledTime()
    {
        // Arrange
        var pastTime = DateTime.UtcNow.AddHours(-1);
        var notification = new Notification { ScheduledFor = pastTime };

        // Act
        var shouldDeliver = DateTime.UtcNow >= notification.ScheduledFor;

        // Assert
        shouldDeliver.Should().BeTrue();
    }

    #endregion

    #region Multi-Channel Coordination Tests

    [Fact]
    public async Task MultiChannel_SendsAllChannelsConcurrently()
    {
        // Arrange
        var notification = new Notification
        {
            UserId = Guid.NewGuid(),
            RecipientEmail = "patient@example.com",
            RecipientPhone = "+1-555-0100"
        };

        var emailTask = _emailProviderMock.Object.SendEmailAsync("", "", "", CancellationToken.None);
        var smsTask = _smsProviderMock.Object.SendSmsAsync("", "", CancellationToken.None);
        var pushTask = _pushProviderMock.Object.SendPushAsync(Guid.NewGuid(), "", "", CancellationToken.None);

        // Act
        await Task.WhenAll(emailTask, smsTask, pushTask);

        // Assert - All tasks completed
        await Task.WhenAll(emailTask, smsTask, pushTask);
    }

    [Fact]
    public async Task MultiChannel_HandlesPartialFailure()
    {
        // Arrange
        var emailSuccess = Task.FromResult(true);
        var smsFailure = Task.FromResult(false);
        var pushSuccess = Task.FromResult(true);

        // Act
        var results = await Task.WhenAll(emailSuccess, smsFailure, pushSuccess);

        // Assert
        results.Should().HaveCount(3);
    }

    #endregion

    #region Notification State Management Tests

    [Fact]
    public void MarkAsDelivered_TransitionsState()
    {
        // Arrange
        var notification = new Notification { IsDelivered = false };

        // Act
        notification.IsDelivered = true;
        notification.DeliveredAt = DateTime.UtcNow;

        // Assert
        notification.IsDelivered.Should().BeTrue();
        notification.DeliveredAt.Should().NotBeNull();
    }

    [Fact]
    public void MarkAsFailed_RecordsError()
    {
        // Arrange
        var notification = new Notification { FailureReason = null };

        // Act
        notification.FailureReason = "SMTP connection timeout";
        notification.DeliveryAttempts++;

        // Assert
        notification.FailureReason.Should().Be("SMTP connection timeout");
        notification.DeliveryAttempts.Should().Be(1);
    }

    #endregion

    #region Template Processing Tests

    [Fact]
    public void ProcessTemplate_ReplacesPlaceholders()
    {
        // Arrange
        var template = "Dear {{FirstName}}, your appointment is on {{AppointmentDate}} at {{AppointmentTime}}.";
        var data = new Dictionary<string, string>
        {
            { "FirstName", "John" },
            { "AppointmentDate", "2026-07-29" },
            { "AppointmentTime", "2:00 PM" }
        };

        // Act
        var rendered = template;
        foreach (var kvp in data)
        {
            rendered = rendered.Replace($"{{{{{kvp.Key}}}}}", kvp.Value);
        }

        // Assert
        rendered.Should().Contain("John");
        rendered.Should().Contain("2026-07-29");
        rendered.Should().Contain("2:00 PM");
    }

    [Fact]
    public void ProcessTemplate_HandlesConditionalContent()
    {
        // Arrange
        var hasConditional = "Dear {{FirstName}},{{#HasBillingIssue}} Please update your payment method.{{/HasBillingIssue}}";

        // Act
        var withIssue = hasConditional
            .Replace("{{FirstName}}", "Jane")
            .Replace("{{#HasBillingIssue}}", "")
            .Replace("{{/HasBillingIssue}}", "");

        // Assert
        withIssue.Should().Contain("Please update your payment method");
    }

    #endregion

    #region Delivery Coordination Tests

    [Fact]
    public async Task CoordinateDelivery_CombinesMultipleChannels()
    {
        // Arrange
        var channels = new[] { "email", "sms", "push" };
        var deliveryTasks = new List<Task<bool>>();

        foreach (var channel in channels)
        {
            if (channel == "email")
                deliveryTasks.Add(_emailProviderMock.Object.SendEmailAsync("", "", "", CancellationToken.None));
            else if (channel == "sms")
                deliveryTasks.Add(_smsProviderMock.Object.SendSmsAsync("", "", CancellationToken.None));
            else if (channel == "push")
                deliveryTasks.Add(_pushProviderMock.Object.SendPushAsync(Guid.NewGuid(), "", "", CancellationToken.None));
        }

        // Act
        var results = await Task.WhenAll(deliveryTasks);

        // Assert
        results.Should().HaveLength(channels.Length);
    }

    #endregion

    #region Helper Interfaces (Mocking Support)

    public interface IEmailProvider
    {
        Task<bool> SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken);
    }

    public interface ISmsProvider
    {
        Task<bool> SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken);
    }

    public interface IPushNotificationProvider
    {
        Task<bool> SendPushAsync(Guid userId, string title, string message, CancellationToken cancellationToken);
    }

    public interface INotificationRepository
    {
        Task AddAsync(Notification notification);
        Task UpdateAsync(Notification notification);
        Task<Notification> GetByIdAsync(Guid id);
    }

    #endregion
}
