using MassTransit;
using Microsoft.Extensions.Logging;
using EHRPlatform.BuildingBlocks.EventBus.Events;

namespace EHRPlatform.Services.Clinical.Infrastructure.Consumers;

/// <summary>
/// Consumes AppointmentCreatedIntegrationEvent from the event bus.
/// Prepares a clinical note stub when an appointment is booked.
/// </summary>
public class AppointmentCreatedConsumer : IConsumer<AppointmentCreatedIntegrationEvent>
{
    private readonly ILogger<AppointmentCreatedConsumer> _logger;

    public AppointmentCreatedConsumer(ILogger<AppointmentCreatedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AppointmentCreatedIntegrationEvent> context)
    {
        var evt = context.Message;

        _logger.LogInformation(
            "Clinical service received AppointmentCreated — PatientId: {PatientId}, ProviderId: {ProviderId}, ScheduledAt: {At}",
            evt.PatientId, evt.ProviderId, evt.ScheduledAt);

        // Future: auto-create a draft clinical note for the appointment
        // so providers can pre-fill SOAP before the visit.

        await Task.CompletedTask;
    }
}
