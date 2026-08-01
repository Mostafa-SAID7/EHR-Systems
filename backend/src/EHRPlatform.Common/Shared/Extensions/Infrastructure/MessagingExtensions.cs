using Confluent.Kafka;
using EHRPlatform.Common.Domain.Events;
using EHRPlatform.Common.Application.Features.EventDriven.Handlers;
using EHRPlatform.Common.Application.Features.EventDriven.Publishing;
using EHRPlatform.Common.Messaging;
using EHRPlatform.Common.Shared.Utilities.Guards;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Common.Infrastructure.EventDriven;

/// <summary>
/// DI extensions for Kafka messaging and the outbox pattern.
/// </summary>
public static class MessagingExtensions
{
    /// <summary>
    /// Register Kafka event publisher and outbox background processor.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="bootstrapServers">Kafka bootstrap servers, e.g. "localhost:9092".</param>
    /// <param name="environment">Deployment environment appended to topic names (default: "production").</param>
    public static IServiceCollection AddKafkaMessaging(
        this IServiceCollection services,
        string bootstrapServers,
        string environment = "production")
    {
        ArgumentGuard.NotNullOrEmpty(bootstrapServers, nameof(bootstrapServers));

        var producerConfig = KafkaConfigBuilder.CreateProducerConfig(bootstrapServers);
        var producer       = new ProducerBuilder<string, string>(producerConfig).Build();
        services.AddSingleton(producer);

        services.AddSingleton<IEventPublisher>(sp =>
            new KafkaEventPublisher(
                producer,
                environment,
                sp.GetRequiredService<ILogger<KafkaEventPublisher>>()));

        // Register outbox processing components
        services.AddScoped<IRetryPolicy, ExponentialBackoffRetryPolicy>();
        services.AddScoped<IOutboxCleanupStrategy>(sp => 
            new AgeBasedCleanupStrategy(retentionDays: 30, sp.GetRequiredService<ILogger<AgeBasedCleanupStrategy>>()));
        services.AddScoped<IOutboxEventPublisher, OutboxEventPublisher>();
        services.AddScoped<OutboxProcessingService>();
        services.AddHostedService<OutboxProcessorBackgroundService>();

        return services;
    }

    /// <summary>
    /// Register a Kafka consumer as a hosted service.
    /// </summary>
    /// <typeparam name="TConsumer">Consumer implementation (must inherit <see cref="KafkaConsumerBase{TEvent}"/>).</typeparam>
    /// <typeparam name="TEvent">Integration event type the consumer handles.</typeparam>
    public static IServiceCollection AddKafkaConsumer<TConsumer, TEvent>(
        this IServiceCollection services,
        string bootstrapServers,
        string groupId,
        string topicName)
        where TConsumer : KafkaConsumerBase<TEvent>
        where TEvent    : IntegrationEvent
    {
        ArgumentGuard.NotNullOrEmpty(bootstrapServers, nameof(bootstrapServers));
        ArgumentGuard.NotNullOrEmpty(groupId,          nameof(groupId));
        ArgumentGuard.NotNullOrEmpty(topicName,        nameof(topicName));

        var config   = KafkaConfigBuilder.CreateConsumerConfig(bootstrapServers, groupId);
        var consumer = new ConsumerBuilder<string, string>(config).Build();
        services.AddSingleton(consumer);
        services.AddHostedService<TConsumer>();

        return services;
    }
}

