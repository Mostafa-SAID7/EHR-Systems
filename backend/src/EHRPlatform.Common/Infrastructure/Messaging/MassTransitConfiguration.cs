using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace EHRPlatform.Common.Infrastructure.Messaging
{
    /// <summary>
    /// Centralized MassTransit configuration for all microservices.
    /// Configures Kafka as the message bus with retry, circuit breaker, and observability.
    /// </summary>
    public static class MassTransitConfiguration
    {
        /// <summary>
        /// Registers MassTransit with Kafka for the specified service.
        /// Auto-discovers and registers all consumers in the provided assembly.
        /// </summary>
        public static IServiceCollection AddMicroserviceMessaging(
            this IServiceCollection services,
            IConfiguration configuration,
            Assembly consumerAssembly)
        {
            var kafkaSettings = configuration.GetSection("Kafka");
            var brokers = kafkaSettings["Brokers"] ?? "localhost:9092";
            var consumerGroup = kafkaSettings["ConsumerGroup"] ?? $"{consumerAssembly.GetName().Name}";

            services.AddMassTransit(x =>
            {
                // Auto-register all consumers in the service's assembly
                x.AddConsumers(consumerAssembly);

                x.UsingKafka((context, cfg) =>
                {
                    // Configure Kafka broker connection
                    cfg.Host(brokers);

                    // Configure consumer group (enables parallel processing)
                    cfg.AutoStart = true;

                    // Global retry policy - exponential backoff
                    cfg.UseMessageRetry(r =>
                    {
                        r.Exponential(
                            retryLimit: 3,
                            initialInterval: TimeSpan.FromSeconds(1),
                            intervalIncrement: TimeSpan.FromSeconds(1),
                            intervalMultiplier: 2.0
                        );

                        // Don't retry on validation or operation cancellation
                        r.Ignore(typeof(ValidationException));
                        r.Ignore(typeof(OperationCanceledException));
                    });

                    // Circuit breaker - prevent cascading failures
                    cfg.UseCircuitBreaker(cb =>
                    {
                        cb.TripThreshold = 5;                    // Trip after 5 failures
                        cb.ActiveThreshold = 10;                 // Active after 10 successes
                        cb.ResetInterval = TimeSpan.FromSeconds(30);
                    });

                    // Dead letter queue for unprocessable messages
                    cfg.UseDeadLetterQueue(dlq =>
                    {
                        dlq.PrefixQueueNameWithProcessName();
                    });

                    // Rate limiting - prevent overwhelming the service
                    cfg.UseRateLimit(r =>
                    {
                        r.RateInterval = TimeSpan.FromSeconds(10);
                        r.SetRateLimit(1000); // 1000 messages per 10 seconds
                    });

                    // Configure consumer group - each service listens independently
                    cfg.GroupId = consumerGroup;

                    // Auto-configure receive endpoints for all consumers
                    cfg.ConfigureConsumers(context);
                });
            });

            services.Configure<MassTransitHostOptions>(options =>
            {
                options.WaitUntilStarted = TimeSpan.FromSeconds(30);
                options.StartTimeout = TimeSpan.FromSeconds(60);
                options.StopTimeout = TimeSpan.FromSeconds(30);
            });

            return services;
        }

        /// <summary>
        /// Alternative configuration for services using RabbitMQ.
        /// Can be used during development if Kafka is not available.
        /// </summary>
        public static IServiceCollection AddMicroserviceMessagingRabbitMq(
            this IServiceCollection services,
            IConfiguration configuration,
            Assembly consumerAssembly)
        {
            var rabbitSettings = configuration.GetSection("RabbitMQ");
            var host = rabbitSettings["Host"] ?? "localhost";
            var port = rabbitSettings.GetValue<ushort>("Port", 5672);
            var username = rabbitSettings["Username"] ?? "guest";
            var password = rabbitSettings["Password"] ?? "guest";
            var virtualHost = rabbitSettings["VirtualHost"] ?? "/";

            services.AddMassTransit(x =>
            {
                x.AddConsumers(consumerAssembly);

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(host, port, virtualHost, h =>
                    {
                        h.Username(username);
                        h.Password(password);
                    });

                    cfg.UseMessageRetry(r =>
                    {
                        r.Exponential(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10), 2.0);
                        r.Ignore(typeof(ValidationException));
                    });

                    cfg.UseCircuitBreaker(cb =>
                    {
                        cb.TripThreshold = 5;
                        cb.ActiveThreshold = 10;
                        cb.ResetInterval = TimeSpan.FromSeconds(30);
                    });

                    cfg.UseDeadLetterQueue(dlq =>
                    {
                        dlq.PrefixQueueNameWithProcessName();
                    });

                    cfg.ConfigureConsumers(context);
                });
            });

            return services;
        }
    }

    /// <summary>
    /// Configuration for MassTransit endpoint topology.
    /// Defines how topics and consumer groups are organized.
    /// </summary>
    public class KafkaTopicNameFormatter : IMessageTopologyConfigurator
    {
        public void Probe(ProbeContext context)
        {
            // No-op
        }

        public static string FormatTopicName(string messageType)
        {
            // Format: {domain}-events (e.g., patient-events, appointment-events)
            return messageType switch
            {
                "UserCreatedEvent" => "user-events",
                "UserUpdatedEvent" => "user-events",
                "PatientCreatedEvent" => "patient-events",
                "PatientUpdatedEvent" => "patient-events",
                "AppointmentScheduledEvent" => "appointment-events",
                "InvoiceGeneratedEvent" => "billing-events",
                "PrescriptionCreatedEvent" => "prescription-events",
                "ClinicalNoteCreatedEvent" => "clinical-events",
                "EmailNotificationSentEvent" => "notification-events",
                "DataAccessLoggedEvent" => "audit-events",
                _ => "system-events"
            };
        }
    }

    /// <summary>
    /// Publisher helper for publishing events with correlation tracking.
    /// </summary>
    public interface IEventPublisher
    {
        Task PublishEventAsync<T>(T @event, Guid correlationId, CancellationToken cancellationToken = default)
            where T : class;
    }

    public class EventPublisher : IEventPublisher
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public EventPublisher(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        }

        public async Task PublishEventAsync<T>(T @event, Guid correlationId, CancellationToken cancellationToken = default)
            where T : class
        {
            var correlationProperty = typeof(T).GetProperty("CorrelationId");
            if (correlationProperty?.CanWrite == true)
            {
                correlationProperty.SetValue(@event, correlationId);
            }

            await _publishEndpoint.Publish(@event, cancellationToken);
        }
    }

    /// <summary>
    /// Consumer configuration helper.
    /// Provides base class for all consumers with common error handling.
    /// </summary>
    public abstract class BaseConsumer<T> : IConsumer<T> where T : class
    {
        protected abstract Task ConsumeMessage(ConsumeContext<T> context);

        public async Task Consume(ConsumeContext<T> context)
        {
            try
            {
                await ConsumeMessage(context);
            }
            catch (Exception ex)
            {
                // Log error and trigger retry
                throw new ConsumerFaultException($"Failed to consume message of type {typeof(T).Name}", ex);
            }
        }
    }
}
