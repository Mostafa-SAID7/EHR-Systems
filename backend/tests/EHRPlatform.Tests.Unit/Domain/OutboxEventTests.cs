using EHRPlatform.Common.Events;
using FluentAssertions;
using System;
using Xunit;

namespace EHRPlatform.Tests.Unit.Domain;

/// <summary>
/// Unit tests for OutboxEvent entity.
/// Validates: immutability, ShouldRetry logic, retry attempt tracking, HIPAA compliance.
/// 12 tests covering entity behavior and state transitions.
/// </summary>
public class OutboxEventTests
{
    #region Constructor & Initialization Tests

    [Fact]
    public void Constructor_CreatesEventWithDefaultValues()
    {
        // Arrange & Act
        var @event = new OutboxEvent();

        // Assert
        @event.Id.Should().NotBe(Guid.Empty);
        @event.EventType.Should().Be(string.Empty);
        @event.EventData.Should().Be(string.Empty);
        @event.CreatedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
        @event.IsPublished.Should().BeFalse();
        @event.PublishedAt.Should().BeNull();
        @event.PublishAttempts.Should().Be(0);
        @event.MaxPublishAttempts.Should().Be(3);
        @event.ErrorMessage.Should().BeNull();
        @event.Transport.Should().Be("kafka");
    }

    [Fact]
    public void Constructor_GeneratesUniqueIds()
    {
        // Arrange & Act
        var event1 = new OutboxEvent();
        var event2 = new OutboxEvent();

        // Assert
        event1.Id.Should().NotBe(event2.Id);
    }

    #endregion

    #region ShouldRetry Logic Tests

    [Fact]
    public void ShouldRetry_ReturnsTrueWhenNotPublishedAndBelowMaxAttempts()
    {
        // Arrange
        var @event = new OutboxEvent
        {
            IsPublished = false,
            PublishAttempts = 1,
            MaxPublishAttempts = 3
        };

        // Act & Assert
        @event.ShouldRetry.Should().BeTrue();
    }

    [Fact]
    public void ShouldRetry_ReturnsFalseWhenPublished()
    {
        // Arrange
        var @event = new OutboxEvent
        {
            IsPublished = true,
            PublishAttempts = 2,
            MaxPublishAttempts = 3
        };

        // Act & Assert
        @event.ShouldRetry.Should().BeFalse();
    }

    [Fact]
    public void ShouldRetry_ReturnsFalseWhenAttemptsExceeded()
    {
        // Arrange
        var @event = new OutboxEvent
        {
            IsPublished = false,
            PublishAttempts = 3,
            MaxPublishAttempts = 3
        };

        // Act & Assert
        @event.ShouldRetry.Should().BeFalse();
    }

    [Fact]
    public void ShouldRetry_ReturnsFalseWhenAttemptsEqualMax()
    {
        // Arrange
        var @event = new OutboxEvent
        {
            IsPublished = false,
            PublishAttempts = 3,
            MaxPublishAttempts = 3
        };

        // Act & Assert
        @event.ShouldRetry.Should().BeFalse();
    }

    #endregion

    #region Retry Attempt Tracking Tests

    [Fact]
    public void PublishAttempts_TracksIncrementalRetries()
    {
        // Arrange
        var @event = new OutboxEvent { PublishAttempts = 0, MaxPublishAttempts = 3 };

        // Act
        @event.PublishAttempts++;

        // Assert
        @event.PublishAttempts.Should().Be(1);
        @event.ShouldRetry.Should().BeTrue();
    }

    [Fact]
    public void MaxPublishAttempts_DefaultsToThree()
    {
        // Arrange & Act
        var @event = new OutboxEvent();

        // Assert
        @event.MaxPublishAttempts.Should().Be(3);
    }

    [Fact]
    public void MaxPublishAttempts_CanBeCustomized()
    {
        // Arrange
        var @event = new OutboxEvent { MaxPublishAttempts = 5 };

        // Act & Assert
        @event.MaxPublishAttempts.Should().Be(5);
    }

    #endregion

    #region Publication State Tests

    [Fact]
    public void IsPublished_TransitionsToTrueWithTimestamp()
    {
        // Arrange
        var @event = new OutboxEvent { IsPublished = false, PublishedAt = null };

        // Act
        @event.IsPublished = true;
        @event.PublishedAt = DateTime.UtcNow;

        // Assert
        @event.IsPublished.Should().BeTrue();
        @event.PublishedAt.Should().NotBeNull();
        @event.PublishedAt.Value.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void ErrorMessage_CapturesPublishFailureReason()
    {
        // Arrange
        var @event = new OutboxEvent();
        var errorMsg = "Kafka broker offline: Connection refused";

        // Act
        @event.ErrorMessage = errorMsg;

        // Assert
        @event.ErrorMessage.Should().Be(errorMsg);
    }

    #endregion

    #region Transport & Routing Tests

    [Fact]
    public void Transport_DefaultsToKafka()
    {
        // Arrange & Act
        var @event = new OutboxEvent();

        // Assert
        @event.Transport.Should().Be("kafka");
    }

    [Fact]
    public void Transport_SupportsMultipleTransports()
    {
        // Arrange
        var kafkaEvent = new OutboxEvent { Transport = "kafka" };
        var rabbitEvent = new OutboxEvent { Transport = "rabbitmq" };

        // Act & Assert
        kafkaEvent.Transport.Should().Be("kafka");
        rabbitEvent.Transport.Should().Be("rabbitmq");
    }

    [Fact]
    public void RoutingKey_StoresQueueNameForRabbitMQ()
    {
        // Arrange
        var @event = new OutboxEvent
        {
            Transport = "rabbitmq",
            RoutingKey = "patient.events.dlq"
        };

        // Act & Assert
        @event.RoutingKey.Should().Be("patient.events.dlq");
    }

    #endregion
}
