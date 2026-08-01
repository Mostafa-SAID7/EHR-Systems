namespace EHRPlatform.Services.Audit.Infrastructure.Kafka;

using MassTransit;
using EHRPlatform.Services.Audit.Application.Features.Audit.Commands;
using MediatR;
using Microsoft.Extensions.Logging;

/// <summary>
/// Kafka consumer that listens to domain events from all services.
/// Records audit entries for each event.
/// </summary>
public class AuditEventConsumer :
    IConsumer<DomainEventOccurred>
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuditEventConsumer> _logger;

    public AuditEventConsumer(IMediator mediator, ILogger<AuditEventConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<DomainEventOccurred> context)
    {
        _logger.LogInformation("Consuming domain event: {EventType} for {ResourceType}/{ResourceId}",
            context.Message.EventType, context.Message.ResourceType, context.Message.ResourceId);

        try
        {
            // Map domain event to audit entry
            var auditCommand = new RecordAuditEntryCommand
            {
                UserId = context.Message.UserId,
                UserEmail = context.Message.UserEmail,
                UserFullName = context.Message.UserFullName,
                Action = MapEventTypeToAction(context.Message.EventType),
                ResourceType = context.Message.ResourceType,
                ResourceId = context.Message.ResourceId,
                IpAddress = context.Message.IpAddress ?? "Unknown",
                UserAgent = context.Message.UserAgent ?? "Unknown",
                HttpMethod = context.Message.HttpMethod ?? "Internal",
                Endpoint = context.Message.Endpoint ?? "/internal",
                ContainsSsn = context.Message.ContainsSsn,
                ContainsDob = context.Message.ContainsDob,
                ContainsMrn = context.Message.ContainsMrn,
                ContainsPhoneNumber = context.Message.ContainsPhoneNumber,
                AccessLevel = DetermineAccessLevel(context.Message.ResourceType, context.Message.ContainsSsn, context.Message.ContainsMrn),
                ChangeDetails = context.Message.ChangeDetails
            };

            var result = await _mediator.Send(auditCommand);

            if (!result.Success)
            {
                _logger.LogError("Failed to record audit entry for event {EventType}", context.Message.EventType);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error consuming audit event");
        }
    }

    private string MapEventTypeToAction(string eventType)
    {
        return eventType switch
        {
            "PatientCreated" => "Create",
            "PatientUpdated" => "Update",
            "PatientDeleted" => "Delete",
            "AppointmentScheduled" => "Create",
            "AppointmentCompleted" => "Update",
            "AppointmentCancelled" => "Delete",
            "InvoiceCreated" => "Create",
            "InvoicePaid" => "Update",
            "ClinicalNoteCreated" => "Create",
            "UserLoggedIn" => "Login",
            "UserLoggedOut" => "Logout",
            _ => "Other"
        };
    }

    private string DetermineAccessLevel(string resourceType, bool containsSsn, bool containsMrn)
    {
        // Restricted: Contains SSN
        if (containsSsn) return "Restricted";
        
        // Confidential: Patient/Clinical data
        if (resourceType == "Patient" || resourceType == "Clinical" || resourceType == "Appointment")
            return "Confidential";

        // Internal: Billing, Notification, etc.
        return "Internal";
    }
}

/// <summary>
/// Domain event message contract for Kafka.
/// </summary>
public interface DomainEventOccurred
{
    Guid EventId { get; }
    string EventType { get; }
    Guid UserId { get; }
    string UserEmail { get; }
    string UserFullName { get; }
    string ResourceType { get; }
    Guid ResourceId { get; }
    string? IpAddress { get; }
    string? UserAgent { get; }
    string? HttpMethod { get; }
    string? Endpoint { get; }
    bool ContainsSsn { get; }
    bool ContainsDob { get; }
    bool ContainsMrn { get; }
    bool ContainsPhoneNumber { get; }
    string? ChangeDetails { get; }
    DateTime OccurredAt { get; }
}
