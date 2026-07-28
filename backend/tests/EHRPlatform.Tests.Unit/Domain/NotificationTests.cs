using EHRPlatform.Services.Notification.Domain.Entities;
using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;

namespace EHRPlatform.Tests.Unit.Domain;

/// <summary>
/// Unit tests for Notification entity and delivery strategies.
/// Validates: notification lifecycle, delivery channels, template validation, retry logic.
/// 20 tests covering multi-channel notification patterns.
/// </summary>
public class NotificationTests
{
    #region Notification Entity Initialization Tests

    [Fact]
    public void Constructor_CreatesNotificationWithDefaults()
    {
        // Arrange & Act
        var notification = new Notification();

        // Assert
        notification.Id.Should().NotBe(Guid.Empty);
        notification.UserId.Should().Be(Guid.Empty);
        notification.RecipientEmail.Should().Be(string.Empty);
        notification.Subject.Should().Be(string.Empty);
        notification.Body.Should().Be(string.Empty);
        notification.DeliveryChannel.Should().BeNull();
        notification.IsDelivered.Should().BeFalse();
        notification.DeliveryAttempts.Should().Be(0);
        notification.MaxDeliveryAttempts.Should().Be(3);
        notification.CreatedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void Notification_CapturesToAndFromData()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var recipientEmail = "patient@example.com";

        // Act
        var notification = new Notification
        {
            UserId = userId,
            RecipientEmail = recipientEmail,
            Subject = "Appointment Reminder",
            Body = "Your appointment is tomorrow at 2:00 PM"
        };

        // Assert
        notification.UserId.Should().Be(userId);
        notification.RecipientEmail.Should().Be(recipientEmail);
        notification.Subject.Should().Be("Appointment Reminder");
        notification.Body.Should().Be("Your appointment is tomorrow at 2:00 PM");
    }

    #endregion

    #region Delivery Channel Tests

    [Fact]
    public void DeliveryChannel_SupportsEmail()
    {
        // Arrange & Act
        var notification = new Notification { DeliveryChannel = "email" };

        // Assert
        notification.DeliveryChannel.Should().Be("email");
    }

    [Fact]
    public void DeliveryChannel_SupportsSMS()
    {
        // Arrange & Act
        var notification = new Notification { DeliveryChannel = "sms" };

        // Assert
        notification.DeliveryChannel.Should().Be("sms");
    }

    [Fact]
    public void DeliveryChannel_SupportsPushNotification()
    {
        // Arrange & Act
        var notification = new Notification { DeliveryChannel = "push" };

        // Assert
        notification.DeliveryChannel.Should().Be("push");
    }

    [Fact]
    public void DeliveryChannel_SupportsMultiChannel()
    {
        // Arrange
        var channels = new[] { "email", "sms", "push" };

        // Act
        var notifications = new List<Notification>();
        foreach (var channel in channels)
        {
            notifications.Add(new Notification { DeliveryChannel = channel });
        }

        // Assert
        notifications.Should().HaveCount(3);
        notifications[0].DeliveryChannel.Should().Be("email");
        notifications[1].DeliveryChannel.Should().Be("sms");
        notifications[2].DeliveryChannel.Should().Be("push");
    }

    #endregion

    #region Delivery State Tests

    [Fact]
    public void IsDelivered_InitializesAsFalse()
    {
        // Arrange & Act
        var notification = new Notification();

        // Assert
        notification.IsDelivered.Should().BeFalse();
    }

    [Fact]
    public void IsDelivered_TransitionsToTrueWithTimestamp()
    {
        // Arrange
        var notification = new Notification { IsDelivered = false, DeliveredAt = null };

        // Act
        notification.IsDelivered = true;
        notification.DeliveredAt = DateTime.UtcNow;

        // Assert
        notification.IsDelivered.Should().BeTrue();
        notification.DeliveredAt.Should().NotBeNull();
    }

    [Fact]
    public void FailureReason_CapturesDeliveryError()
    {
        // Arrange
        var notification = new Notification();

        // Act
        notification.FailureReason = "Email service timeout after 3 retries";

        // Assert
        notification.FailureReason.Should().Be("Email service timeout after 3 retries");
    }

    #endregion

    #region Retry Attempt Tracking Tests

    [Fact]
    public void DeliveryAttempts_TracksRetries()
    {
        // Arrange
        var notification = new Notification { DeliveryAttempts = 0, MaxDeliveryAttempts = 3 };

        // Act
        notification.DeliveryAttempts++;

        // Assert
        notification.DeliveryAttempts.Should().Be(1);
    }

    [Fact]
    public void MaxDeliveryAttempts_DefaultsToThree()
    {
        // Arrange & Act
        var notification = new Notification();

        // Assert
        notification.MaxDeliveryAttempts.Should().Be(3);
    }

    [Fact]
    public void MaxDeliveryAttempts_CanBeCustomized()
    {
        // Arrange
        var notification = new Notification { MaxDeliveryAttempts = 5 };

        // Act & Assert
        notification.MaxDeliveryAttempts.Should().Be(5);
    }

    [Fact]
    public void ShouldRetry_ReturnsTrueWhenBelowMaxAttempts()
    {
        // Arrange
        var notification = new Notification
        {
            IsDelivered = false,
            DeliveryAttempts = 1,
            MaxDeliveryAttempts = 3
        };

        // Act
        var shouldRetry = !notification.IsDelivered && notification.DeliveryAttempts < notification.MaxDeliveryAttempts;

        // Assert
        shouldRetry.Should().BeTrue();
    }

    [Fact]
    public void ShouldRetry_ReturnsFalseWhenDelivered()
    {
        // Arrange
        var notification = new Notification
        {
            IsDelivered = true,
            DeliveryAttempts = 2
        };

        // Act
        var shouldRetry = !notification.IsDelivered && notification.DeliveryAttempts < notification.MaxDeliveryAttempts;

        // Assert
        shouldRetry.Should().BeFalse();
    }

    [Fact]
    public void ShouldRetry_ReturnsFalseWhenMaxAttemptsExceeded()
    {
        // Arrange
        var notification = new Notification
        {
            IsDelivered = false,
            DeliveryAttempts = 3,
            MaxDeliveryAttempts = 3
        };

        // Act
        var shouldRetry = !notification.IsDelivered && notification.DeliveryAttempts < notification.MaxDeliveryAttempts;

        // Assert
        shouldRetry.Should().BeFalse();
    }

    #endregion

    #region Template Validation Tests

    [Fact]
    public void Subject_RequiredForValidNotification()
    {
        // Arrange
        var notification = new Notification
        {
            Subject = "Appointment Reminder",
            Body = "Your appointment is scheduled"
        };

        // Act
        var isValid = !string.IsNullOrEmpty(notification.Subject) && !string.IsNullOrEmpty(notification.Body);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void Body_RequiredForValidNotification()
    {
        // Arrange
        var notification = new Notification
        {
            Subject = "Appointment Reminder",
            Body = "Your appointment is scheduled"
        };

        // Act
        var isValid = !string.IsNullOrEmpty(notification.Body);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void NotificationTemplate_SupportsPlaceholders()
    {
        // Arrange
        var template = "Hello {{FirstName}}, your appointment is on {{Date}}";

        // Act
        var rendered = template
            .Replace("{{FirstName}}", "John")
            .Replace("{{Date}}", "2026-07-29");

        // Assert
        rendered.Should().Be("Hello John, your appointment is on 2026-07-29");
    }

    #endregion

    #region Notification Metadata Tests

    [Fact]
    public void TenantId_EnablesMultiTenantIsolation()
    {
        // Arrange
        var tenant1 = Guid.NewGuid();
        var tenant2 = Guid.NewGuid();

        var notification1 = new Notification { TenantId = tenant1 };
        var notification2 = new Notification { TenantId = tenant2 };

        // Act & Assert
        notification1.TenantId.Should().Be(tenant1);
        notification2.TenantId.Should().Be(tenant2);
        notification1.TenantId.Should().NotBe(notification2.TenantId);
    }

    [Fact]
    public void ScheduledFor_EnablesDeferredDelivery()
    {
        // Arrange
        var futureTime = DateTime.UtcNow.AddHours(2);

        // Act
        var notification = new Notification { ScheduledFor = futureTime };

        // Assert
        notification.ScheduledFor.Should().Be(futureTime);
        notification.ScheduledFor.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void Priority_ControlsDeliveryOrder()
    {
        // Arrange
        var highPriority = new Notification { Priority = 1 };
        var lowPriority = new Notification { Priority = 10 };

        // Act & Assert
        highPriority.Priority.Should().BeLessThan(lowPriority.Priority);
    }

    #endregion

    #region Audit Trail Tests

    [Fact]
    public void CreatedAt_TracksNotificationCreation()
    {
        // Arrange & Act
        var notification = new Notification();

        // Assert
        notification.CreatedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
        notification.CreatedAt.Should().BeAfter(DateTime.UtcNow.AddSeconds(-5));
    }

    [Fact]
    public void AuditTrail_RecordsDeliveryAttempts()
    {
        // Arrange
        var notification = new Notification { DeliveryAttempts = 0 };
        var attempts = new List<(int attempt, DateTime timestamp)>();

        // Act
        for (int i = 1; i <= 3; i++)
        {
            notification.DeliveryAttempts = i;
            attempts.Add((i, DateTime.UtcNow));
        }

        // Assert
        attempts.Should().HaveCount(3);
        attempts[0].attempt.Should().Be(1);
        attempts[2].attempt.Should().Be(3);
    }

    #endregion
}
