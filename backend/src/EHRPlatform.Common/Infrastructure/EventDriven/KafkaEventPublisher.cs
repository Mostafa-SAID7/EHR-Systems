using System.Text.Json;
using Confluent.Kafka;
using EHRPlatform.Common.Events;
using EHRPlatform.Common.Shared.Utilities;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Common.Messaging;

/// <summary>
/// Kafka implementation of <see cref="IEventPublisher"/>.
///
/// Topic naming: {eventType}.{environment}
/// Example: patient-created-event.production
///
/// Partitioning: by EventId (override <see cref="IntegrationEvent.GetPartitionKey"/> for aggregate ordering).
/// Retries: configured in <see cref="KafkaConfigBuilder.CreateProducerConfig"/>.
/// </summary>
public sealed class KafkaEventPublisher : IEventPublisher
{
    private readonly IProducer<string, string> _producer;
    private readonly string _environment;
    private readonly ILogger<KafkaEventPublisher> _logger;

    public KafkaEventPublisher(
        IProducer<string, string> producer,
        string environment,
        ILogger<KafkaEventPublisher> logger)
    {
        _producer = producer ?? throw new ArgumentNullException(nameof(producer));
        _environment = environment ?? "production";
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task PublishAsync(IntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        ArgumentGuard.NotNull(@event, nameof(@event));

        var topicName = GetTopicName(@event.EventType);

        try
        {
            var message = new Message<string, string>
            {
                Key = @event.GetPartitionKey(),
                Value = JsonSerializer.Serialize(@event, @event.GetType()),
                Timestamp = new Timestamp(DateTime.UtcNow)
            };

            var deliveryReport = await _producer.ProduceAsync(topicName, message, cancellationToken);

            if (deliveryReport.Status != PersistenceStatus.Persisted)
                throw new InvalidOperationException(
                    $"Event delivery to '{topicName}' was not persisted (status: {deliveryReport.Status}).");

            _logger.LogInformation(
                "Event {EventId} ({EventType}) published to {Topic}",
                @event.EventId, @event.EventType, topicName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event {EventId} to {Topic}", @event.EventId, topicName);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task PublishBatchAsync(
        IEnumerable<IntegrationEvent> events,
        CancellationToken cancellationToken = default)
    {
        ArgumentGuard.NotNull(events, nameof(events));

        var eventList = events.ToList();
        if (eventList.Count == 0)
            return;

        var tasks = eventList.Select(e => PublishAsync(e, cancellationToken));
        await Task.WhenAll(tasks);

        _logger.LogInformation("Published {Count} events in batch", eventList.Count);
    }

    private string GetTopicName(string eventType) =>
        $"{eventType}.{_environment}".ToLower();
}

/// <summary>
/// Base class for Kafka consumers. Handles deserialization and offset management.
/// </summary>
public abstract class KafkaConsumerBase<TEvent> : Microsoft.Extensions.Hosting.BackgroundService
    where TEvent : IntegrationEvent
{
    private readonly IConsumer<string, string> _consumer;
    private readonly ILogger _logger;
    private readonly string _topicName;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    protected KafkaConsumerBase(
        IConsumer<string, string> consumer,
        string topicName,
        ILogger logger)
    {
        _consumer = consumer;
        _topicName = topicName;
        _logger = logger;
    }

    /// <summary>Override to process each received event.</summary>
    protected abstract Task HandleEventAsync(TEvent @event, CancellationToken cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(_topicName);
        _logger.LogInformation("Kafka consumer started for topic {Topic}", _topicName);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var consumeResult = _consumer.Consume(stoppingToken);
                if (consumeResult == null) continue;

                try
                {
                    var @event = JsonSerializer.Deserialize<TEvent>(consumeResult.Message.Value, _jsonOptions);
                    if (@event != null)
                        await HandleEventAsync(@event, stoppingToken);

                    _consumer.Commit(consumeResult);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message from {Topic}", _topicName);
                }
            }
        }
        catch (OperationCanceledException) { /* expected on shutdown */ }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kafka consumer fatal error");
        }
        finally
        {
            _consumer.Close();
            _consumer.Dispose();
        }
    }
}

/// <summary>Factory for standard Kafka producer/consumer configurations.</summary>
public static class KafkaConfigBuilder
{
    public static ProducerConfig CreateProducerConfig(string bootstrapServers) =>
        new()
        {
            BootstrapServers = bootstrapServers,
            ClientId = $"{Environment.MachineName}-producer",
            Acks = Acks.All,
            RetryBackoffMs = 100,
            MessageSendMaxRetries = 3,
            EnableDeliveryReports = true,
            CompressionType = CompressionType.Snappy
        };

    public static ConsumerConfig CreateConsumerConfig(string bootstrapServers, string groupId) =>
        new()
        {
            BootstrapServers = bootstrapServers,
            GroupId = groupId,
            ClientId = $"{Environment.MachineName}-consumer-{groupId}",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            AllowAutoCreateTopics = true
        };
}

