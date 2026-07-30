using EHRPlatform.Common.Infrastructure.Telemetry;
using EHRPlatform.Services.Notification.Hubs;
using EHRPlatform.Services.Notification.Models;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Notification.Consumers;

/// <summary>
/// RabbitMQ consumer in the Notification Service.
/// Handles <see cref="SendWelcomeNotificationMessage"/> by:
///   1. Sending a welcome email (stub – replace with real provider).
///   2. Pushing a real-time notification via SignalR to the Angular frontend.
///   3. (Optionally) publishing WelcomeNotificationSentEvent back to Kafka so the saga advances.
/// </summary>
public sealed class SendWelcomeNotificationConsumer : IConsumer<SendWelcomeNotificationMessage>
{
    private readonly IHubContext<EHRNotificationHub> _hub;
    private readonly ILogger<SendWelcomeNotificationConsumer> _logger;

    public SendWelcomeNotificationConsumer(
        IHubContext<EHRNotificationHub> hub,
        ILogger<SendWelcomeNotificationConsumer> logger)
    {
        _hub = hub;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SendWelcomeNotificationMessage> context)
    {
        var msg = context.Message;

        using var activity = EHRTelemetry.StartActivity(
            "Notification.SendWelcome",
            correlationId: msg.CorrelationId);

        activity?.SetTag(EHRTelemetry.TagPatientId, msg.PatientId.ToString());

        _logger.LogInformation(
            "Processing welcome notification for PatientId={PatientId} MRN={MRN}",
            msg.PatientId, msg.MRN);

        try
        {
            // ── 1. Send email (stub) ──────────────────────────────────────────
            // TODO: inject IEmailService (SendGrid / MailKit) and call:
            //   await _emailService.SendAsync(msg.Email, "Welcome to EHR", templateId, data);
            await Task.Delay(30, context.CancellationToken); // simulate email I/O

            _logger.LogInformation(
                "Welcome email sent (simulated) for PatientId={PatientId}",
                msg.PatientId);

            // ── 2. Push real-time notification via SignalR ────────────────────
            var tenantGroup = msg.TenantId.HasValue
                ? $"tenant:{msg.TenantId}"
                : "tenant:default";

            await _hub.Clients.Group(tenantGroup).SendAsync(
                "PatientRegistered",
                new
                {
                    patientId    = msg.PatientId,
                    mrn          = msg.MRN,
                    registeredAt = msg.RegisteredAt
                },
                context.CancellationToken);

            _logger.LogInformation(
                "SignalR notification sent to group {Group} for PatientId={PatientId}",
                tenantGroup, msg.PatientId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to process welcome notification for PatientId={PatientId}",
                msg.PatientId);
            activity.RecordException(ex);
            throw; // MassTransit retry + dead-letter
        }
    }
}

/// <summary>Message contract (shared between producer and consumer).</summary>
public record SendWelcomeNotificationMessage
{
    public Guid PatientId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string MRN { get; init; } = string.Empty;
    public string? CorrelationId { get; init; }
    public Guid? TenantId { get; init; }
    public DateTime RegisteredAt { get; init; } = DateTime.UtcNow;
}

