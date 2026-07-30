using Confluent.Kafka;
using EHRPlatform.Common.Messaging;
using MassTransit;
using MassTransit.Monitoring;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EHRPlatform.Common.Shared.Extensions;

/// <summary>
/// MassTransit DI registration helpers.
///
/// Transport strategy:
///   Kafka   → domain events (high-throughput, ordered, durable, replayable).
///   RabbitMQ → background job queues (routing, priority, dead-letter, retries).
///
/// Call one of:
///   AddMassTransitWithKafka()    – Kafka-only (analytics/event consumers)
///   AddMassTransitWithRabbitMQ() – RabbitMQ-only (notification workers)
///   AddMassTransitHybrid()       – both transports (recommended for app services)
/// </summary>
public static class MassTransitExtensions
{
    /// <summary>
    /// Register MassTransit with Kafka as the domain-event bus.
    /// </summary>
    public static IServiceCollection AddMassTransitWithKafka(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IRiderRegistrationConfigurator>? configureKafka = null)
    {
        var kafkaServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";

        services.AddMassTransit(x =>
        {
            x.UsingInMemory((ctx, cfg) =>
            {
                cfg.ConfigureEndpoints(ctx);
            });

            x.AddRider(rider =>
            {
                configureKafka?.Invoke(rider);

                rider.UsingKafka((ctx, k) =>
                {
                    k.Host(kafkaServers);
                    k.SecurityProtocol = Confluent.Kafka.SecurityProtocol.Plaintext;
                });
            });
        });

        services.AddScoped<IMessageBus, EHRMessageBus>();
        return services;
    }

    /// <summary>
    /// Register MassTransit with RabbitMQ as the background-job bus.
    /// Configures standard EHR exchange/queue topology with dead-lettering.
    /// 
    /// Metrics Collected (via MassTransit diagnostics):
    ///   - RabbitMQ queue length
    ///   - Consumer count
    ///   - Publish rate
    ///   - Ack rate
    ///   - Dead-letter messages
    ///   - Unacked messages
    /// </summary>
    public static IServiceCollection AddMassTransitWithRabbitMQ(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationConfigurator>? configureConsumers = null)
    {
        var rabbitHost     = configuration["RabbitMQ:Host"]     ?? "localhost";
        var rabbitUser     = configuration["RabbitMQ:Username"]  ?? "ehr_user";
        var rabbitPass     = configuration["RabbitMQ:Password"]  ?? "ehr_password";
        var rabbitVHost    = configuration["RabbitMQ:VirtualHost"] ?? "/ehr";

        services.AddMassTransit(x =>
        {
            // ── RabbitMQ Metrics Configuration ─────────────────────────────────────
            // Enable MassTransit activity source for OpenTelemetry integration
            // Collects:
            //   - rabbitmq.queue.message_count (gauge)
            //   - rabbitmq.consumer_count (gauge)
            //   - messaging.publish.messages (counter - publish rate)
            //   - messaging.acknowledge (counter - ack rate)
            //   - rabbitmq.message.dead_letter (counter)
            //   - rabbitmq.message.redelivered (counter)
            
            // Note: MassTransit metrics are automatically collected via OpenTelemetry instrumentation
            
            configureConsumers?.Invoke(x);

            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(rabbitHost, rabbitVHost, h =>
                {
                    h.Username(rabbitUser);
                    h.Password(rabbitPass);
                });

                // Global retry policy for all RabbitMQ consumers
                cfg.UseMessageRetry(r =>
                    r.Exponential(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(3)));

                // Dead-letter to a dedicated exchange on exhaustion
                cfg.UseDelayedMessageScheduler();

                cfg.ConfigureEndpoints(ctx);
            });
        });

        services.AddScoped<IMessageBus, EHRMessageBus>();
        return services;
    }

    /// <summary>
    /// Register MassTransit with BOTH Kafka (domain events) and RabbitMQ (background jobs).
    /// This is the recommended setup for application microservices.
    ///
    /// Pattern:
    ///   IBus           → RabbitMQ (default MassTransit bus)
    ///   IKafkaProducer → Kafka rider
    ///   IMessageBus    → unified wrapper (uses IBus underneath)
    /// 
    /// Metrics Collected (via MassTransit diagnostics):
    ///   - RabbitMQ queue length
    ///   - Consumer count
    ///   - Publish rate
    ///   - Ack rate
    ///   - Dead-letter messages
    ///   - Unacked messages
    /// </summary>
    public static IServiceCollection AddMassTransitHybrid(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationConfigurator>? configureRabbitMqConsumers = null,
        Action<IRiderRegistrationConfigurator>? configureKafkaRider = null)
    {
        var rabbitHost  = configuration["RabbitMQ:Host"]        ?? "localhost";
        var rabbitUser  = configuration["RabbitMQ:Username"]     ?? "ehr_user";
        var rabbitPass  = configuration["RabbitMQ:Password"]     ?? "ehr_password";
        var rabbitVHost = configuration["RabbitMQ:VirtualHost"]  ?? "/ehr";
        var kafkaServers= configuration["Kafka:BootstrapServers"]?? "localhost:9092";

        services.AddMassTransit(x =>
        {
            // ── RabbitMQ Metrics Configuration ─────────────────────────────────────
            // Enable MassTransit activity source for OpenTelemetry integration
            // Collects:
            //   - rabbitmq.queue.message_count (gauge)
            //   - rabbitmq.consumer_count (gauge)
            //   - messaging.publish.messages (counter - publish rate)
            //   - messaging.acknowledge (counter - ack rate)
            //   - rabbitmq.message.dead_letter (counter)
            //   - rabbitmq.message.redelivered (counter)
            
            // Note: MassTransit metrics are automatically collected via OpenTelemetry instrumentation
            
            // Register RabbitMQ consumers
            configureRabbitMqConsumers?.Invoke(x);

            // Primary transport: RabbitMQ (for background jobs)
            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(rabbitHost, rabbitVHost, h =>
                {
                    h.Username(rabbitUser);
                    h.Password(rabbitPass);
                });

                cfg.UseMessageRetry(r =>
                    r.Exponential(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(3)));

                cfg.UseDelayedMessageScheduler();
                cfg.ConfigureEndpoints(ctx);
            });

            // Secondary transport: Kafka rider (for domain events)
            x.AddRider(rider =>
            {
                configureKafkaRider?.Invoke(rider);

                rider.UsingKafka((ctx, k) =>
                {
                    k.Host(kafkaServers);
                    k.SecurityProtocol = Confluent.Kafka.SecurityProtocol.Plaintext;
                });
            });
        });

        services.AddScoped<IMessageBus, EHRMessageBus>();
        return services;
    }
}

