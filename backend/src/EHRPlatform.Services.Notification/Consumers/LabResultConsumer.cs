using EHRPlatform.BuildingBlocks.Observability.Telemetry;
using EHRPlatform.Services.Notification.Hubs;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Notification.Consumers;

/// <summary>
/// Kafka consumer: real-time bridge for new lab result events.
///
/// When a Clinical Service publishes a LabResultReadyEvent to Kafka,
/// this consumer immediately pushes it to the doctor's SignalR room so
/// the Angular dashboard updates without polling.
///
/// Transport: Kafka topic "lab-result-ready-event.{environment}"
/// </summary>
public sealed class LabResultConsumer : IConsumer<LabResultReadyEvent>
{
    private readonly IHubContext<EHRNotificationHub> _hub;
    private readonly ILogger<LabResultConsumer> _logger;

    public LabResultConsumer(
        IHubContext<EHRNotificationHub> hub,
        ILogger<LabResultConsumer> logger)
    {
        _hub = hub;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<LabResultReadyEvent> context)
    {
        var evt = context.Message;

        using var activity = EHRTelemetry.StartActivity(
            "Notification.LabResult",
            correlationId: evt.CorrelationId);

        activity?.SetTag(EHRTelemetry.TagPatientId, evt.PatientId.ToString());

        _logger.LogInformation(
            "Lab result ready for PatientId={PatientId} ResultId={ResultId}",
            evt.PatientId, evt.ResultId);

        // Push to patient room (all clinicians currently viewing this patient)
        await _hub.Clients.Group($"patient:{evt.PatientId}").SendAsync(
            "LabResultReady",
            new
            {
                resultId    = evt.ResultId,
                patientId   = evt.PatientId,
                labName     = evt.LabName,
                status      = evt.Status,
                receivedAt  = evt.ReceivedAt
            },
            context.CancellationToken);

        // Push to the ordering doctor's personal room
        if (evt.OrderingDoctorId.HasValue)
        {
            await _hub.Clients.Group($"doctor:{evt.OrderingDoctorId}").SendAsync(
                "LabResultReady",
                new
                {
                    resultId  = evt.ResultId,
                    patientId = evt.PatientId,
                    labName   = evt.LabName,
                    status    = evt.Status
                },
                context.CancellationToken);
        }
    }
}

/// <summary>Kafka domain event published by Clinical Service when a lab result arrives.</summary>
public record LabResultReadyEvent
{
    public Guid ResultId { get; init; }
    public Guid PatientId { get; init; }
    public string LabName { get; init; } = string.Empty;
    public string Status { get; init; } = "Ready";
    public Guid? OrderingDoctorId { get; init; }
    public DateTime ReceivedAt { get; init; } = DateTime.UtcNow;
    public string? CorrelationId { get; init; }
}


