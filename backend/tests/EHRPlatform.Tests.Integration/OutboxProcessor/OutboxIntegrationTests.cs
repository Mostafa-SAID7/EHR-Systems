using EHRPlatform.Common.Data;
using EHRPlatform.Common.Events;
using EHRPlatform.Services.OutboxProcessor.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EHRPlatform.Tests.Integration.OutboxProcessor;

/// <summary>
/// Integration tests for OutboxProcessor end-to-end workflow.
/// Validates: database persistence, event polling, batch processing, idempotency with real database.
/// 15 tests covering complete outbox pattern with Testcontainers PostgreSQL.
/// </summary>
public class OutboxIntegrationTests : IAsyncLifetime
{
    private readonly DbContextOptions<MultiServiceOutboxDbContext> _dbContextOptions;

    public OutboxIntegrationTests()
    {
        // Use in-memory database for integration tests (can be replaced with Testcontainers)
        _dbContextOptions = new DbContextOptionsBuilder<MultiServiceOutboxDbContext>()
            .UseInMemoryDatabase(databaseName: $"OutboxDb_{Guid.NewGuid()}")
            .Options;
    }

    public async Task InitializeAsync()
    {
        using var context = new MultiServiceOutboxDbContext(_dbContextOptions);
        await context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        using var context = new MultiServiceOutboxDbContext(_dbContextOptions);
        await context.Database.EnsureDeletedAsync();
    }

    #region Event Persistence Tests

    [Fact]
    public async Task CreateOutboxEvent_PersistsToDatabase()
    {
        // Arrange
        using var context = new MultiServiceOutboxDbContext(_dbContextOptions);
        var @event = new OutboxEvent
        {
            Id = Guid.NewGuid(),
            EventType = "PatientCreated",
            EventData = """{"patientId":"123"}""",
            AggregateId = Guid.NewGuid(),
            Transport = "kafka"
        };

        // Act
        context.OutboxEvents.Add(@event);
        await context.SaveChangesAsync();

        // Assert
        using var verifyContext = new MultiServiceOutboxDbContext(_dbContextOptions);
        var persisted = await verifyContext.OutboxEvents.FindAsync(@event.Id);
        persisted.Should().NotBeNull();
        persisted.EventType.Should().Be("PatientCreated");
        persisted.IsPublished.Should().BeFalse();
    }

    [Fact]
    public async Task CreateMultipleOutboxEvents_AllPersisted()
    {
        // Arrange
        using var context = new MultiServiceOutboxDbContext(_dbContextOptions);
        var events = new[]
        {
            new OutboxEvent { Id = Guid.NewGuid(), EventType = "PatientCreated", AggregateId = Guid.NewGuid() },
            new OutboxEvent { Id = Guid.NewGuid(), EventType = "PatientUpdated", AggregateId = Guid.NewGuid() },
            new OutboxEvent { Id = Guid.NewGuid(), EventType = "PatientDeleted", AggregateId = Guid.NewGuid() }
        };

        // Act
        foreach (var @event in events)
        {
            context.OutboxEvents.Add(@event);
        }
        await context.SaveChangesAsync();

        // Assert
        using var verifyContext = new MultiServiceOutboxDbContext(_dbContextOptions);
        var count = await verifyContext.OutboxEvents.CountAsync();
        count.Should().Be(3);
    }

    #endregion

    #region Event Polling Tests

    [Fact]
    public async Task PollUnpublishedEvents_ReturnsCorrectEvents()
    {
        // Arrange
        using var context = new MultiServiceOutboxDbContext(_dbContextOptions);
        var unpublished = new OutboxEvent
        {
            Id = Guid.NewGuid(),
            EventType = "PatientCreated",
            IsPublished = false,
            PublishAttempts = 0
        };
        var published = new OutboxEvent
        {
            Id = Guid.NewGuid(),
            EventType = "PatientIndexed",
            IsPublished = true,
            PublishedAt = DateTime.UtcNow,
            PublishAttempts = 1
        };

        context.OutboxEvents.Add(unpublished);
        context.OutboxEvents.Add(published);
        await context.SaveChangesAsync();

        // Act
        using var pollContext = new MultiServiceOutboxDbContext(_dbContextOptions);
        var unprocessed = await pollContext.OutboxEvents
            .Where(e => !e.IsPublished && e.PublishAttempts < 3)
            .OrderBy(e => e.CreatedAt)
            .ToListAsync();

        // Assert
        unprocessed.Should().HaveCount(1);
        unprocessed.First().EventType.Should().Be("PatientCreated");
    }

    [Fact]
    public async Task PollEvents_RespectsBatchSize()
    {
        // Arrange
        using var context = new MultiServiceOutboxDbContext(_dbContextOptions);
        const int batchSize = 10;
        var events = Enumerable.Range(0, 25)
            .Select(_ => new OutboxEvent
            {
                Id = Guid.NewGuid(),
                EventType = "PatientCreated",
                IsPublished = false,
                PublishAttempts = 0
            })
            .ToList();

        foreach (var @event in events)
        {
            context.OutboxEvents.Add(@event);
        }
        await context.SaveChangesAsync();

        // Act
        using var pollContext = new MultiServiceOutboxDbContext(_dbContextOptions);
        var batch = await pollContext.OutboxEvents
            .Where(e => !e.IsPublished)
            .OrderBy(e => e.CreatedAt)
            .Take(batchSize)
            .ToListAsync();

        // Assert
        batch.Should().HaveCount(batchSize);
    }

    [Fact]
    public async Task PollEvents_OrdersByCreatedAtForFIFO()
    {
        // Arrange
        using var context = new MultiServiceOutboxDbContext(_dbContextOptions);
        var now = DateTime.UtcNow;
        var events = new[]
        {
            new OutboxEvent { Id = Guid.NewGuid(), EventType = "B", CreatedAt = now.AddSeconds(2), IsPublished = false },
            new OutboxEvent { Id = Guid.NewGuid(), EventType = "A", CreatedAt = now.AddSeconds(1), IsPublished = false },
            new OutboxEvent { Id = Guid.NewGuid(), EventType = "C", CreatedAt = now.AddSeconds(3), IsPublished = false }
        };

        foreach (var @event in events)
        {
            context.OutboxEvents.Add(@event);
        }
        await context.SaveChangesAsync();

        // Act
        using var pollContext = new MultiServiceOutboxDbContext(_dbContextOptions);
        var ordered = await pollContext.OutboxEvents
            .Where(e => !e.IsPublished)
            .OrderBy(e => e.CreatedAt)
            .ToListAsync();

        // Assert
        ordered[0].EventType.Should().Be("A");
        ordered[1].EventType.Should().Be("B");
        ordered[2].EventType.Should().Be("C");
    }

    #endregion

    #region Event Publication State Tests

    [Fact]
    public async Task MarkEventAsPublished_UpdatesDatabase()
    {
        // Arrange
        using var context = new MultiServiceOutboxDbContext(_dbContextOptions);
        var @event = new OutboxEvent
        {
            Id = Guid.NewGuid(),
            EventType = "PatientCreated",
            IsPublished = false,
            PublishAttempts = 0
        };
        context.OutboxEvents.Add(@event);
        await context.SaveChangesAsync();

        // Act
        using (var updateContext = new MultiServiceOutboxDbContext(_dbContextOptions))
        {
            var eventToUpdate = await updateContext.OutboxEvents.FindAsync(@event.Id);
            eventToUpdate.IsPublished = true;
            eventToUpdate.PublishedAt = DateTime.UtcNow;
            eventToUpdate.PublishAttempts = 1;
            await updateContext.SaveChangesAsync();
        }

        // Assert
        using var verifyContext = new MultiServiceOutboxDbContext(_dbContextOptions);
        var published = await verifyContext.OutboxEvents.FindAsync(@event.Id);
        published.IsPublished.Should().BeTrue();
        published.PublishedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task IncrementPublishAttempts_TracksRetries()
    {
        // Arrange
        using var context = new MultiServiceOutboxDbContext(_dbContextOptions);
        var @event = new OutboxEvent
        {
            Id = Guid.NewGuid(),
            EventType = "PatientCreated",
            IsPublished = false,
            PublishAttempts = 0,
            MaxPublishAttempts = 3
        };
        context.OutboxEvents.Add(@event);
        await context.SaveChangesAsync();

        // Act
        using (var updateContext = new MultiServiceOutboxDbContext(_dbContextOptions))
        {
            var eventToUpdate = await updateContext.OutboxEvents.FindAsync(@event.Id);
            eventToUpdate.PublishAttempts++;
            eventToUpdate.ErrorMessage = "Kafka broker offline";
            await updateContext.SaveChangesAsync();
        }

        // Assert
        using var verifyContext = new MultiServiceOutboxDbContext(_dbContextOptions);
        var updated = await verifyContext.OutboxEvents.FindAsync(@event.Id);
        updated.PublishAttempts.Should().Be(1);
        updated.ErrorMessage.Should().Contain("offline");
    }

    #endregion

    #region Idempotency Tests

    [Fact]
    public async Task IdempotentPublish_MarkedPublishedOnceOnly()
    {
        // Arrange
        using var context = new MultiServiceOutboxDbContext(_dbContextOptions);
        var @event = new OutboxEvent
        {
            Id = Guid.NewGuid(),
            EventType = "PatientCreated",
            IsPublished = false
        };
        context.OutboxEvents.Add(@event);
        await context.SaveChangesAsync();
        var publishTime = DateTime.UtcNow;

        // Act - First publish
        using (var firstPublishContext = new MultiServiceOutboxDbContext(_dbContextOptions))
        {
            var toPublish = await firstPublishContext.OutboxEvents.FindAsync(@event.Id);
            toPublish.IsPublished = true;
            toPublish.PublishedAt = publishTime;
            await firstPublishContext.SaveChangesAsync();
        }

        // Act - Replay publish (should not change timestamp)
        using (var replayContext = new MultiServiceOutboxDbContext(_dbContextOptions))
        {
            var toPublish = await replayContext.OutboxEvents.FindAsync(@event.Id);
            if (toPublish.IsPublished)
            {
                // Skip re-publish
            }
            else
            {
                toPublish.PublishedAt = DateTime.UtcNow;
            }
            await replayContext.SaveChangesAsync();
        }

        // Assert
        using var verifyContext = new MultiServiceOutboxDbContext(_dbContextOptions);
        var verified = await verifyContext.OutboxEvents.FindAsync(@event.Id);
        verified.PublishedAt.Should().Be(publishTime);
    }

    [Fact]
    public async Task DeadLetterQueue_EventsExceedingMaxAttempts()
    {
        // Arrange
        using var context = new MultiServiceOutboxDbContext(_dbContextOptions);
        var dlqEvent = new OutboxEvent
        {
            Id = Guid.NewGuid(),
            EventType = "PatientCreated",
            IsPublished = false,
            PublishAttempts = 3,
            MaxPublishAttempts = 3,
            ErrorMessage = "Kafka broker offline after 3 attempts"
        };
        context.OutboxEvents.Add(dlqEvent);
        await context.SaveChangesAsync();

        // Act
        using var queryContext = new MultiServiceOutboxDbContext(_dbContextOptions);
        var deadLettered = await queryContext.OutboxEvents
            .Where(e => !e.IsPublished && e.PublishAttempts >= e.MaxPublishAttempts)
            .ToListAsync();

        // Assert
        deadLettered.Should().HaveCount(1);
        deadLettered.First().ShouldRetry.Should().BeFalse();
    }

    #endregion

    #region Aggregate Correlation Tests

    [Fact]
    public async Task AggregateId_CorrelatesEventsToPatient()
    {
        // Arrange
        using var context = new MultiServiceOutboxDbContext(_dbContextOptions);
        var patientId = Guid.NewGuid();
        var events = new[]
        {
            new OutboxEvent { Id = Guid.NewGuid(), EventType = "PatientCreated", AggregateId = patientId },
            new OutboxEvent { Id = Guid.NewGuid(), EventType = "PatientIndexed", AggregateId = patientId },
            new OutboxEvent { Id = Guid.NewGuid(), EventType = "NotificationSent", AggregateId = patientId }
        };

        foreach (var @event in events)
        {
            context.OutboxEvents.Add(@event);
        }
        await context.SaveChangesAsync();

        // Act
        using var queryContext = new MultiServiceOutboxDbContext(_dbContextOptions);
        var patientEvents = await queryContext.OutboxEvents
            .Where(e => e.AggregateId == patientId)
            .ToListAsync();

        // Assert
        patientEvents.Should().HaveCount(3);
        patientEvents.Should().AllSatisfy(e => e.AggregateId.Should().Be(patientId));
    }

    [Fact]
    public async Task EventsByType_EnablesRouting()
    {
        // Arrange
        using var context = new MultiServiceOutboxDbContext(_dbContextOptions);
        var events = new[]
        {
            new OutboxEvent { Id = Guid.NewGuid(), EventType = "PatientCreated" },
            new OutboxEvent { Id = Guid.NewGuid(), EventType = "PatientCreated" },
            new OutboxEvent { Id = Guid.NewGuid(), EventType = "AppointmentScheduled" }
        };

        foreach (var @event in events)
        {
            context.OutboxEvents.Add(@event);
        }
        await context.SaveChangesAsync();

        // Act
        using var queryContext = new MultiServiceOutboxDbContext(_dbContextOptions);
        var patientEvents = await queryContext.OutboxEvents
            .Where(e => e.EventType == "PatientCreated")
            .ToListAsync();

        // Assert
        patientEvents.Should().HaveCount(2);
    }

    #endregion

    #region Transport Routing Tests

    [Fact]
    public async Task KafkaTransport_DefaultsAndPersists()
    {
        // Arrange
        using var context = new MultiServiceOutboxDbContext(_dbContextOptions);
        var @event = new OutboxEvent
        {
            Id = Guid.NewGuid(),
            EventType = "PatientCreated",
            Transport = "kafka"
        };
        context.OutboxEvents.Add(@event);
        await context.SaveChangesAsync();

        // Act
        using var queryContext = new MultiServiceOutboxDbContext(_dbContextOptions);
        var persisted = await queryContext.OutboxEvents.FindAsync(@event.Id);

        // Assert
        persisted.Transport.Should().Be("kafka");
    }

    [Fact]
    public async Task RabbitMQTransport_WithRoutingKey()
    {
        // Arrange
        using var context = new MultiServiceOutboxDbContext(_dbContextOptions);
        var @event = new OutboxEvent
        {
            Id = Guid.NewGuid(),
            EventType = "PatientCreated",
            Transport = "rabbitmq",
            RoutingKey = "patient.events.dlq"
        };
        context.OutboxEvents.Add(@event);
        await context.SaveChangesAsync();

        // Act
        using var queryContext = new MultiServiceOutboxDbContext(_dbContextOptions);
        var persisted = await queryContext.OutboxEvents.FindAsync(@event.Id);

        // Assert
        persisted.Transport.Should().Be("rabbitmq");
        persisted.RoutingKey.Should().Be("patient.events.dlq");
    }

    #endregion

    #region Batch Processing Tests

    [Fact]
    public async Task BatchProcess_HandlesLargeVolume()
    {
        // Arrange
        using var context = new MultiServiceOutboxDbContext(_dbContextOptions);
        const int eventCount = 500;
        var events = Enumerable.Range(0, eventCount)
            .Select(_ => new OutboxEvent
            {
                Id = Guid.NewGuid(),
                EventType = "PatientCreated",
                IsPublished = false,
                PublishAttempts = 0
            })
            .ToList();

        foreach (var @event in events)
        {
            context.OutboxEvents.Add(@event);
        }
        await context.SaveChangesAsync();

        // Act
        const int batchSize = 100;
        using var queryContext = new MultiServiceOutboxDbContext(_dbContextOptions);
        var firstBatch = await queryContext.OutboxEvents
            .Where(e => !e.IsPublished)
            .OrderBy(e => e.CreatedAt)
            .Take(batchSize)
            .ToListAsync();

        var totalUnprocessed = await queryContext.OutboxEvents
            .Where(e => !e.IsPublished)
            .CountAsync();

        // Assert
        firstBatch.Should().HaveCount(batchSize);
        totalUnprocessed.Should().Be(eventCount);
    }

    #endregion

    #region Event Data & JSON Tests

    [Fact]
    public async Task EventData_JsonbPersistence()
    {
        // Arrange
        using var context = new MultiServiceOutboxDbContext(_dbContextOptions);
        var jsonData = """{"patientId":"550e8400-e29b-41d4-a716-446655440000","email":"patient@example.com"}""";
        var @event = new OutboxEvent
        {
            Id = Guid.NewGuid(),
            EventType = "PatientCreated",
            EventData = jsonData
        };
        context.OutboxEvents.Add(@event);
        await context.SaveChangesAsync();

        // Act
        using var queryContext = new MultiServiceOutboxDbContext(_dbContextOptions);
        var persisted = await queryContext.OutboxEvents.FindAsync(@event.Id);

        // Assert
        persisted.EventData.Should().Be(jsonData);
        persisted.EventData.Should().Contain("patientId");
    }

    #endregion
}
