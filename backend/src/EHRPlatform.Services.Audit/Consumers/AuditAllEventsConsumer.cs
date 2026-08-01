using MassTransit;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using EHRPlatform.BuildingBlocks.Contracts.DTOs;
using EHRPlatform.Services.Audit.Data;
using EHRPlatform.Services.Audit.Domain.Entities;

namespace EHRPlatform.Services.Audit.Consumers
{
    /// <summary>
    /// Generic consumer that logs ALL events from the event bus.
    /// Implements HIPAA audit trail requirement: all data modifications must be logged.
    /// </summary>
    public abstract class AuditEventConsumer<T> : IConsumer<T> where T : class
    {
        private readonly AuditContext _context;
        private readonly ILogger<AuditEventConsumer<T>> _logger;

        protected AuditEventConsumer(AuditContext context, ILogger<AuditEventConsumer<T>> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Consume(ConsumeContext<T> context)
        {
            var @event = context.Message;
            
            _logger.LogInformation(
                "Auditing event: {EventType}, CorrelationId={CorrelationId}",
                typeof(T).Name,
                GetCorrelationId(@event)
            );

            try
            {
                var auditLog = CreateAuditLog(@event, context);
                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Audit logged: {Action} for {ResourceId}",
                    auditLog.Action,
                    auditLog.ResourceId
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to audit event {EventType}",
                    typeof(T).Name
                );
                throw; // Trigger retry - auditing is critical
            }
        }

        protected abstract AuditLog CreateAuditLog(T @event, ConsumeContext<T> context);

        protected Guid GetCorrelationId(T @event)
        {
            var prop = typeof(T).GetProperty("CorrelationId");
            if (prop?.GetValue(@event) is Guid correlationId)
                return correlationId;
            return Guid.Empty;
        }

        protected string GetInitiatedBy(T @event)
        {
            var prop = typeof(T).GetProperty("InitiatedBy");
            return prop?.GetValue(@event) as string ?? "system";
        }

        protected DateTime GetTimestamp(T @event)
        {
            var prop = typeof(T).GetProperty("Timestamp");
            if (prop?.GetValue(@event) is DateTime timestamp)
                return timestamp;
            return DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Audits PatientCreatedEvent - new patient registration.
    /// </summary>
    public class AuditPatientCreatedConsumer : AuditEventConsumer<PatientCreatedEvent>
    {
        public AuditPatientCreatedConsumer(AuditContext context, ILogger<AuditEventConsumer<PatientCreatedEvent>> logger)
            : base(context, logger)
        {
        }

        protected override AuditLog CreateAuditLog(PatientCreatedEvent @event, ConsumeContext<PatientCreatedEvent> context)
        {
            return new AuditLog
            {
                Action = "PatientCreated",
                ResourceType = "Patient",
                ResourceId = @event.PatientId.ToString(),
                OldValues = "{}",
                NewValues = JsonSerializer.Serialize(new
                {
                    @event.MRN,
                    @event.FirstName,
                    @event.LastName,
                    @event.DateOfBirth
                }),
                CorrelationId = @event.CorrelationId,
                ActorId = GetInitiatedBy(@event),
                Timestamp = GetTimestamp(@event),
                IpAddress = context.SourceAddress?.Host ?? "unknown",
                Details = $"New patient registered: MRN={@event.MRN}"
            };
        }
    }

    /// <summary>
    /// Audits UserCreatedEvent - new user account creation.
    /// </summary>
    public class AuditUserCreatedConsumer : AuditEventConsumer<UserCreatedEvent>
    {
        public AuditUserCreatedConsumer(AuditContext context, ILogger<AuditEventConsumer<UserCreatedEvent>> logger)
            : base(context, logger)
        {
        }

        protected override AuditLog CreateAuditLog(UserCreatedEvent @event, ConsumeContext<UserCreatedEvent> context)
        {
            return new AuditLog
            {
                Action = "UserCreated",
                ResourceType = "User",
                ResourceId = @event.UserId.ToString(),
                OldValues = "{}",
                NewValues = JsonSerializer.Serialize(new
                {
                    @event.Email,
                    @event.FirstName,
                    @event.LastName
                }),
                CorrelationId = @event.CorrelationId,
                ActorId = GetInitiatedBy(@event),
                Timestamp = GetTimestamp(@event),
                IpAddress = context.SourceAddress?.Host ?? "unknown",
                Details = $"New user account created: {@event.Email}"
            };
        }
    }

    /// <summary>
    /// Audits AppointmentScheduledEvent - appointment bookings.
    /// </summary>
    public class AuditAppointmentScheduledConsumer : AuditEventConsumer<AppointmentScheduledEvent>
    {
        public AuditAppointmentScheduledConsumer(AuditContext context, ILogger<AuditEventConsumer<AppointmentScheduledEvent>> logger)
            : base(context, logger)
        {
        }

        protected override AuditLog CreateAuditLog(AppointmentScheduledEvent @event, ConsumeContext<AppointmentScheduledEvent> context)
        {
            return new AuditLog
            {
                Action = "AppointmentScheduled",
                ResourceType = "Appointment",
                ResourceId = @event.AppointmentId.ToString(),
                OldValues = "{}",
                NewValues = JsonSerializer.Serialize(new
                {
                    @event.PatientId,
                    @event.ProviderId,
                    @event.ScheduledDateTime
                }),
                CorrelationId = @event.CorrelationId,
                ActorId = GetInitiatedBy(@event),
                Timestamp = GetTimestamp(@event),
                IpAddress = context.SourceAddress?.Host ?? "unknown",
                Details = $"Appointment scheduled: {@event.AppointmentType}"
            };
        }
    }

    /// <summary>
    /// Audits InvoiceGeneratedEvent - billing events.
    /// </summary>
    public class AuditInvoiceGeneratedConsumer : AuditEventConsumer<InvoiceGeneratedEvent>
    {
        public AuditInvoiceGeneratedConsumer(AuditContext context, ILogger<AuditEventConsumer<InvoiceGeneratedEvent>> logger)
            : base(context, logger)
        {
        }

        protected override AuditLog CreateAuditLog(InvoiceGeneratedEvent @event, ConsumeContext<InvoiceGeneratedEvent> context)
        {
            return new AuditLog
            {
                Action = "InvoiceGenerated",
                ResourceType = "Invoice",
                ResourceId = @event.InvoiceId.ToString(),
                OldValues = "{}",
                NewValues = JsonSerializer.Serialize(new
                {
                    @event.PatientId,
                    @event.Amount,
                    @event.DueDate
                }),
                CorrelationId = @event.CorrelationId,
                ActorId = GetInitiatedBy(@event),
                Timestamp = GetTimestamp(@event),
                IpAddress = context.SourceAddress?.Host ?? "unknown",
                Details = $"Invoice generated: ${@event.Amount}"
            };
        }
    }

    /// <summary>
    /// Audits NotificationFailedEvent - notification failures.
    /// </summary>
    public class AuditNotificationFailedConsumer : AuditEventConsumer<NotificationFailedEvent>
    {
        public AuditNotificationFailedConsumer(AuditContext context, ILogger<AuditEventConsumer<NotificationFailedEvent>> logger)
            : base(context, logger)
        {
        }

        protected override AuditLog CreateAuditLog(NotificationFailedEvent @event, ConsumeContext<NotificationFailedEvent> context)
        {
            return new AuditLog
            {
                Action = "NotificationFailed",
                ResourceType = "Notification",
                ResourceId = @event.NotificationId.ToString(),
                OldValues = "{}",
                NewValues = JsonSerializer.Serialize(new { @event.Reason }),
                CorrelationId = @event.CorrelationId,
                ActorId = "system",
                Timestamp = GetTimestamp(@event),
                IpAddress = context.SourceAddress?.Host ?? "unknown",
                Details = $"Notification failed: {(@event.Reason ?? "Unknown reason")}",
                Severity = "Warning"
            };
        }
    }
}

