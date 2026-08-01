using EHRPlatform.BuildingBlocks.Observability.Telemetry;
using EHRPlatform.Services.Patient.Messaging.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Patient.Messaging.Consumers;

/// <summary>
/// RabbitMQ consumer: handles the <see cref="SendWelcomeNotificationMessage"/> background job.
///
/// In a real system this would call an email/SMS provider (SendGrid, Twilio).
/// Here it logs the notification and simulates dispatch, ready for real integration.
///
/// Dead-letter: if this consumer throws after all MassTransit retries, the message
/// moves to the "ehr.patient.welcome-notification_error" dead-letter queue for
/// manual inspection or replay.
///
/// HIPAA: logs contain only non-PII identifiers (PatientId, MRN, not the email).
/// </summary>
public sealed class WelcomeNotificationConsumer : IConsumer<SendWelcomeNotificationMessage>
{
    private readonly ILogger<WelcomeNotificationConsumer> _logger;

    public WelcomeNotificationConsumer(ILogger<WelcomeNotificationConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SendWelcomeNotificationMessage> context)
    {
        var msg = context.Message;

        using var activity = EHRTelemetry.StartActivity(
            "WelcomeNotificationConsumer.Consume",
            correlationId: msg.CorrelationId);

        activity?.SetTag(EHRTelemetry.TagPatientId, msg.PatientId.ToString());

        _logger.LogInformation(
            "Sending welcome notification for PatientId={PatientId} MRN={MRN}",
            msg.PatientId, msg.MRN);

        try
        {
            // ── Simulate: replace with real email/SMS provider ─────────────────
            await Task.Delay(50, context.CancellationToken); // simulate I/O

            // TODO: inject IEmailService / ISmsService and call:
            //   await _emailService.SendWelcomeAsync(msg.Email, msg.FirstName, msg.MRN);

            _logger.LogInformation(
                "Welcome notification dispatched for PatientId={PatientId} (simulated)",
                msg.PatientId);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning(
                "Welcome notification cancelled for PatientId={PatientId}",
                msg.PatientId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send welcome notification for PatientId={PatientId}",
                msg.PatientId);
            activity.RecordException(ex);
            throw; // triggers MassTransit retry then dead-letter
        }
    }
}


