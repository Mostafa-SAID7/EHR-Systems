using EHRPlatform.Common.Infrastructure.Caching;
using EHRPlatform.Common.Messaging;
using EHRPlatform.Common.Infrastructure.Telemetry;
using EHRPlatform.Services.Patient.Domain.Events;
using EHRPlatform.Services.Patient.Messaging.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Patient.Messaging.Consumers;

/// <summary>
/// Kafka consumer: reacts to <see cref="PatientCreatedEvent"/> domain events.
///
/// Responsibilities (fan-out side effects):
///   1. Invalidate Redis cache for patient list queries.
///   2. Dispatch a <see cref="PatientIndexMessage"/> to RabbitMQ for ES indexing.
///   3. Dispatch a <see cref="SendWelcomeNotificationMessage"/> to RabbitMQ.
///
/// Idempotency: all operations are safe to re-run (cache invalidation is a no-op
/// if key is absent, ES upsert uses the patient ID as the document ID).
///
/// Transport: Kafka topic "patient-created-event.{environment}"
/// </summary>
public sealed class PatientCreatedKafkaConsumer : IConsumer<PatientCreatedEvent>
{
    private readonly ICacheService _cache;
    private readonly IMessageBus _messageBus;
    private readonly ILogger<PatientCreatedKafkaConsumer> _logger;

    public PatientCreatedKafkaConsumer(
        ICacheService cache,
        IMessageBus messageBus,
        ILogger<PatientCreatedKafkaConsumer> logger)
    {
        _cache = cache;
        _messageBus = messageBus;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PatientCreatedEvent> context)
    {
        var evt = context.Message;
        using var activity = EHRTelemetry.StartActivity(
            "PatientCreatedKafkaConsumer.Consume",
            correlationId: evt.CorrelationId);

        activity?.SetTag(EHRTelemetry.TagPatientId, evt.PatientId.ToString());

        _logger.LogInformation(
            "Kafka consumer received PatientCreatedEvent: PatientId={PatientId} MRN={MRN}",
            evt.PatientId, evt.MRN);

        try
        {
            // 1. Invalidate patient list cache so next query fetches fresh data
            var cacheKey = CacheKeyGenerator.PatientsListKey;
            await _cache.RemoveAsync(cacheKey);

            _logger.LogDebug("Invalidated cache key {CacheKey}", cacheKey);

            // 2. Queue Elasticsearch indexing as a RabbitMQ background job
            await _messageBus.SendBackgroundJobAsync(new PatientIndexMessage
            {
                PatientId     = evt.PatientId,
                FirstName     = evt.FirstName,
                LastName      = evt.LastName,
                Email         = evt.Email,
                MRN           = evt.MRN,
                CorrelationId = evt.CorrelationId
            }, context.CancellationToken);

            // 3. Queue welcome notification as a RabbitMQ background job
            await _messageBus.SendBackgroundJobAsync(new SendWelcomeNotificationMessage
            {
                PatientId     = evt.PatientId,
                FirstName     = evt.FirstName,
                LastName      = evt.LastName,
                Email         = evt.Email,
                MRN           = evt.MRN,
                CorrelationId = evt.CorrelationId,
                TenantId      = evt.TenantId,
                RegisteredAt  = evt.OccurredAt
            }, context.CancellationToken);

            _logger.LogInformation(
                "PatientCreatedEvent side-effects dispatched for PatientId={PatientId}",
                evt.PatientId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing PatientCreatedEvent for PatientId={PatientId}",
                evt.PatientId);
            activity.RecordException(ex);
            throw; // MassTransit will retry per configured policy
        }
    }
}

