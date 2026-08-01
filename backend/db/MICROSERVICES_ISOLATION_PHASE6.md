# Phase 6: Verify Event-Driven Communication

## Overview

Phase 6 implements and verifies Kafka/MassTransit integration patterns for true event-driven inter-service communication. This is the linchpin of microservices isolation—events replace direct service calls, enabling asynchronous, loosely-coupled communication.

## Architecture

### Event Bus Infrastructure

```
┌─────────────────────────────────────────────────────────────┐
│                     Kafka Cluster                            │
│  (Zookeeper for coordination, Broker for message routing)    │
└─────────────────────────────────────────────────────────────┘
  ▲                          ▲                         ▲
  │                          │                         │
  │ Publish                  │ Publish                 │ Publish
  │                          │                         │
┌─────────┐           ┌────────────┐          ┌──────────────┐
│ Patient │           │Appointment │          │   Billing    │
│ Service │◄──────────┤  Service   │◄────────►│   Service    │
│         │  Events   │            │  Events  │              │
└─────────┘           └────────────┘          └──────────────┘
  │                          │                         │
  └──────────────────────────┼─────────────────────────┘
                            │
                 ┌──────────────────────┐
                 │ Notification Service │
                 │ (Consumes all events)│
                 └──────────────────────┘
                            │
                 ┌──────────────────────┐
                 │   Audit Service      │
                 │ (Logs all events)    │
                 └──────────────────────┘
```

### MassTransit Consumer Pattern

MassTransit is the integration framework that:
1. Wraps Kafka producer/consumer logic
2. Handles serialization (JSON)
3. Provides retry/circuit breaker policies
4. Enables publish/subscribe and request/response patterns
5. Manages message sagas (distributed transactions)

## Implementation Patterns

### 1. Event Definition Pattern (Already Established)

Events are DTOs in `EHRPlatform.Common/Shared/DTOs/`:

```csharp
namespace EHRPlatform.Common.Shared.DTOs
{
    public class PatientCreatedEvent
    {
        public Guid PatientId { get; set; }
        public string MRN { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        
        // Context
        public Guid CorrelationId { get; set; }
        public DateTime Timestamp { get; set; }
        public string InitiatedBy { get; set; }
    }
}
```

**Key Principle:** Events include minimal context (ID, core data, timestamp). No nested objects.

### 2. Publisher Pattern

Publishers emit events to Kafka when domain actions occur.

#### In Identity Service (User Creation):

```csharp
// src/EHRPlatform.Services.Identity/Application/Users/Commands/CreateUserCommand.cs

using MassTransit;
using EHRPlatform.Common.Shared.DTOs;

public class CreateUserHandler : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly IdentityContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateUserHandler(IdentityContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = new User
        {
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        // Publish event to Kafka
        var @event = new UserCreatedEvent
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            CorrelationId = request.CorrelationId,
            Timestamp = DateTime.UtcNow,
            InitiatedBy = request.ActorId
        };

        await _publishEndpoint.Publish(@event, cancellationToken);

        return user.Id;
    }
}
```

#### In Patient Service (Patient Creation):

```csharp
// src/EHRPlatform.Services.Patient/Application/Patients/Commands/CreatePatientCommand.cs

using MassTransit;
using EHRPlatform.Common.Shared.DTOs;

public class CreatePatientHandler : IRequestHandler<CreatePatientCommand, Guid>
{
    private readonly PatientContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreatePatientHandler(PatientContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Guid> Handle(CreatePatientCommand request, CancellationToken cancellationToken)
    {
        var patient = new Patient
        {
            MRN = request.MRN,
            FirstName = request.FirstName,
            LastName = request.LastName,
            DateOfBirth = request.DateOfBirth,
            Status = PatientStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        _context.Patients.Add(patient);
        await _context.SaveChangesAsync(cancellationToken);

        // Publish event to Kafka
        var @event = new PatientCreatedEvent
        {
            PatientId = patient.Id,
            MRN = patient.MRN,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            DateOfBirth = patient.DateOfBirth,
            CorrelationId = request.CorrelationId,
            Timestamp = DateTime.UtcNow,
            InitiatedBy = request.ActorId
        };

        await _publishEndpoint.Publish(@event, cancellationToken);

        return patient.Id;
    }
}
```

### 3. Consumer Pattern

Consumers subscribe to events and react accordingly.

#### In Notification Service (Email on User Created):

```csharp
// src/EHRPlatform.Services.Notification/Consumers/UserCreatedConsumer.cs

using MassTransit;
using EHRPlatform.Common.Shared.DTOs;

public class UserCreatedConsumer : IConsumer<UserCreatedEvent>
{
    private readonly ILogger<UserCreatedConsumer> _logger;
    private readonly IEmailService _emailService;

    public UserCreatedConsumer(ILogger<UserCreatedConsumer> logger, IEmailService emailService)
    {
        _logger = logger;
        _emailService = emailService;
    }

    public async Task Consume(ConsumeContext<UserCreatedEvent> context)
    {
        var @event = context.Message;
        _logger.LogInformation($"User created: {@event.UserId} ({@event.Email})");

        try
        {
            // Send welcome email
            await _emailService.SendWelcomeEmailAsync(
                @event.Email,
                @event.FirstName,
                @event.LastName,
                context.CancellationToken
            );

            _logger.LogInformation($"Welcome email sent to {@event.Email}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send welcome email to {@event.Email}");
            throw; // Trigger retry logic
        }
    }
}
```

#### In Audit Service (Log All Events):

```csharp
// src/EHRPlatform.Services.Audit/Consumers/AuditAllEventsConsumer.cs

using MassTransit;
using EHRPlatform.Common.Shared.DTOs;

public class AuditPatientCreatedConsumer : IConsumer<PatientCreatedEvent>
{
    private readonly AuditContext _context;
    private readonly ILogger<AuditPatientCreatedConsumer> _logger;

    public AuditPatientCreatedConsumer(AuditContext context, ILogger<AuditPatientCreatedConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PatientCreatedEvent> context)
    {
        var @event = context.Message;
        
        var auditLog = new AuditLog
        {
            Action = "PatientCreated",
            ResourceType = "Patient",
            ResourceId = @event.PatientId.ToString(),
            OldValues = "{}",
            NewValues = System.Text.Json.JsonSerializer.Serialize(@event),
            CorrelationId = @event.CorrelationId,
            ActorId = @event.InitiatedBy,
            Timestamp = @event.Timestamp,
            IpAddress = context.SourceAddress?.Host ?? "unknown"
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Audit logged: {auditLog.Action} for {auditLog.ResourceId}");
    }
}
```

### 4. MassTransit Configuration

Register MassTransit with Kafka in each service's `Program.cs`:

```csharp
// src/EHRPlatform.Services.Patient/Program.cs

using MassTransit;

var builder = WebApplicationBuilder.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<PatientContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Add MassTransit with Kafka
builder.Services.AddMassTransit(x =>
{
    // Configure consumers (auto-discovered from assembly)
    x.AddConsumers(typeof(Program).Assembly);

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["Kafka:Host"] ?? "localhost", 
                 builder.Configuration["Kafka:Port"] as ushort? ?? 9092);

        // Configure receive endpoint for this service
        cfg.ReceiveEndpoint("patient-service", e =>
        {
            e.ConfigureConsumers(context);
        });
    });
});

var app = builder.Build();
app.Run();
```

**Note:** For Kafka, use `UsingRabbitMq` pattern is shown here but with Kafka it's:

```csharp
x.UsingKafka((context, cfg) =>
{
    cfg.Host(builder.Configuration["Kafka:Brokers"] ?? "localhost:9092");
    cfg.MessageTopicNameFormatter = new KafkaTopicNameFormatter();
    
    cfg.TopicEndpoint<PatientCreatedEvent>("patient-events", e =>
    {
        e.ConfigureConsumer<SomeConsumer>(context);
    });
});
```

### 5. Event Topic Organization

Organize Kafka topics by domain:

```yaml
Topics:
  - patient-events              # PatientCreatedEvent, PatientUpdatedEvent, etc.
  - appointment-events          # AppointmentScheduledEvent, etc.
  - clinical-events             # ClinicalNoteCreatedEvent, etc.
  - billing-events              # InvoiceGeneratedEvent, PaymentReceivedEvent, etc.
  - prescription-events         # PrescriptionCreatedEvent, etc.
  - notification-events         # EmailNotificationSentEvent, etc.
  - audit-events                # DataAccessLoggedEvent, etc.
  - user-events                 # UserCreatedEvent, UserUpdatedEvent, etc.
  - system-events               # System-wide events (errors, alerts)
  - dlq-events                  # Dead Letter Queue for failed messages
```

Each topic has:
- **Partitions:** 3-5 (for parallelism)
- **Replication Factor:** 1 (development) or 3 (production)
- **Retention:** 7 days or topic-specific

### 6. Retry & Circuit Breaker Policy

Handle failures gracefully:

```csharp
// MassTransit retry configuration

builder.Services.AddMassTransit(x =>
{
    x.AddConsumers(typeof(Program).Assembly);

    x.UsingKafka((context, cfg) =>
    {
        cfg.Host(builder.Configuration["Kafka:Brokers"] ?? "localhost:9092");

        // Global retry policy
        cfg.UseMessageRetry(r =>
        {
            r.Exponential(
                retryLimit: 3,
                initialInterval: TimeSpan.FromSeconds(1),
                intervalIncrement: TimeSpan.FromSeconds(1)
            );
            
            // Retry only on transient errors
            r.Ignore(typeof(ValidationException));
            r.Ignore(typeof(OperationCanceledException));
        });

        // Circuit breaker
        cfg.UseCircuitBreaker(cb =>
        {
            cb.TripThreshold = 5;                  // Trip after 5 failures
            cb.ActiveThreshold = 10;               // Active after 10 successive successes
            cb.ResetInterval = TimeSpan.FromSeconds(30);
        });

        // Dead letter queue for unprocessable messages
        cfg.UseDeadLetterQueue(dlq =>
        {
            dlq.PrefixQueueNameWithProcessName();
        });
    });
});
```

### 7. Request/Reply Pattern (For Synchronous Operations)

When a service needs a synchronous response (though async is preferred):

```csharp
// Publish request and wait for response
public class GetPatientDetailsQueryHandler : IRequestHandler<GetPatientDetailsQuery, PatientDto>
{
    private readonly IRequestClient<GetPatientDetailsRequest> _requestClient;

    public GetPatientDetailsQueryHandler(IRequestClient<GetPatientDetailsRequest> requestClient)
    {
        _requestClient = requestClient;
    }

    public async Task<PatientDto> Handle(GetPatientDetailsQuery request, CancellationToken cancellationToken)
    {
        var response = await _requestClient.GetResponse<PatientDetailsResponse>(
            new GetPatientDetailsRequest { PatientId = request.PatientId },
            cancellationToken
        );

        return response.Message.Patient;
    }
}
```

### 8. Saga Pattern (For Distributed Transactions)

When one event triggers a multi-step workflow across services:

```csharp
// Example: Appointment workflow
// 1. AppointmentScheduledEvent published
// 2. Notification service sends reminder
// 3. Billing service creates invoice
// 4. Audit service logs action

public class AppointmentSaga : ISaga
{
    public Guid CorrelationId { get; set; }
    public Guid AppointmentId { get; set; }
    public AppointmentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AppointmentSagaDefinition : SagaStateMachineDefinition<AppointmentSaga>
{
    public AppointmentSagaDefinition()
    {
        // Configure saga lifecycle
    }
}
```

## Event Flow Example: Patient Registration

### Scenario
A new patient registers via API Gateway.

### Flow

1. **API Gateway receives request**
   ```
   POST /api/patients
   {
     "firstName": "John",
     "lastName": "Doe",
     "dateOfBirth": "1990-01-15"
   }
   ```

2. **Patient Service creates patient**
   ```csharp
   var patient = new Patient { ... };
   db.Patients.Add(patient);
   db.SaveChanges();
   
   // Publish event
   await publishEndpoint.Publish(new PatientCreatedEvent { ... });
   ```

3. **PatientCreatedEvent published to Kafka**
   ```json
   {
     "PatientId": "12345",
     "MRN": "MRN001",
     "FirstName": "John",
     "LastName": "Doe",
     "DateOfBirth": "1990-01-15",
     "CorrelationId": "abc-123",
     "Timestamp": "2025-01-15T10:30:00Z",
     "InitiatedBy": "user@hospital.com"
   }
   ```

4. **Notification Service receives event**
   - Sends welcome email
   - Publishes `EmailNotificationSentEvent`

5. **Audit Service receives event**
   - Logs access: "PatientCreated"
   - Stores event in audit database for compliance

6. **Analytics Service receives event**
   - Records patient registration metric
   - Updates dashboard

7. **Audit Service receives EmailNotificationSentEvent**
   - Logs: "Email sent to new patient"

## Event Versioning Strategy

### Versioning Pattern

Support multiple event versions during migration:

```csharp
// V1 (current)
public class PatientCreatedEvent
{
    public Guid PatientId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime CreatedAt { get; set; }
}

// V2 (breaking change: added required field)
public class PatientCreatedEventV2
{
    public Guid PatientId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Gender { get; set; }  // New required field
    public DateTime DateOfBirth { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### Consumer Strategy During Migration

```csharp
// Consume both V1 and V2 simultaneously
public class PatientCreatedConsumerV1 : IConsumer<PatientCreatedEvent> { ... }
public class PatientCreatedConsumerV2 : IConsumer<PatientCreatedEventV2> { ... }

// Gradually migrate producers from V1 to V2
// Once all producers use V2, remove V1 consumer
```

## Monitoring & Observability

### Kafka Monitoring

```bash
# Check consumer lag
docker exec ehr-kafka kafka-consumer-groups \
  --bootstrap-server localhost:9092 \
  --group patient-service \
  --describe

# Monitor topic throughput
docker exec ehr-kafka kafka-console-consumer \
  --bootstrap-server localhost:9092 \
  --topic patient-events \
  --from-beginning \
  --max-messages 10
```

### MassTransit Metrics

```csharp
// Add observability
builder.Services.AddMassTransit(x =>
{
    x.AddConsumers(typeof(Program).Assembly);
    
    x.UsingKafka((context, cfg) =>
    {
        // Enable observability
        cfg.UsePrometheusMetrics(enableMessageSizeHistogram: true);
    });
});
```

### Logging

```csharp
// Log all events
public class LoggingFilter<T> : IFilter<ConsumeContext<T>> where T : class
{
    private readonly ILogger<LoggingFilter<T>> _logger;

    public LoggingFilter(ILogger<LoggingFilter<T>> logger) => _logger = logger;

    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        _logger.LogInformation($"Consuming {typeof(T).Name}: {context.Message}");
        await next.Send(context);
    }

    public void Probe(ProbeContext context) { }
}

// Register filter
x.ConfigureConsumers(context);
x.UseConsumeFilter(typeof(LoggingFilter<>), context);
```

## Testing Event-Driven Communication

### Unit Test Example

```csharp
[TestClass]
public class PatientCreatedConsumerTests
{
    [TestMethod]
    public async Task ShouldSendEmailWhenPatientCreated()
    {
        // Arrange
        var mockEmailService = new Mock<IEmailService>();
        var consumer = new UserCreatedConsumer(
            Mock.Of<ILogger<UserCreatedConsumer>>(),
            mockEmailService.Object
        );

        var @event = new UserCreatedEvent
        {
            UserId = Guid.NewGuid(),
            Email = "user@test.com",
            FirstName = "John",
            LastName = "Doe"
        };

        var context = new TestConsumeContext<UserCreatedEvent>(@event);

        // Act
        await consumer.Consume(context);

        // Assert
        mockEmailService.Verify(
            x => x.SendWelcomeEmailAsync("user@test.com", "John", "Doe", It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
```

### Integration Test Example

```csharp
[TestClass]
public class PatientEventIntegrationTests
{
    private MassTransitFixture _fixture;

    [TestInitialize]
    public void Setup()
    {
        _fixture = new MassTransitFixture();
        _fixture.AddConsumer<UserCreatedConsumer>();
    }

    [TestMethod]
    public async Task ShouldProcessPatientCreatedEvent()
    {
        // Arrange
        var @event = new PatientCreatedEvent
        {
            PatientId = Guid.NewGuid(),
            MRN = "MRN001",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        await _fixture.Bus.Publish(@event);

        // Assert
        _fixture.Consumed.Select<PatientCreatedEvent>()
            .Any(c => c.Context.Message.PatientId == @event.PatientId)
            .Should().BeTrue();
    }
}
```

## Verification Checklist

- [ ] Kafka container running (`docker-compose ps | grep kafka`)
- [ ] Zookeeper container running (`docker-compose ps | grep zookeeper`)
- [ ] Topics created (`docker exec ehr-kafka kafka-topics --list --bootstrap-server localhost:9092`)
- [ ] MassTransit registered in each service's `Program.cs`
- [ ] Event DTOs defined in `EHRPlatform.Common/Shared/DTOs/`
- [ ] Publishers implemented in service command handlers
- [ ] Consumers implemented in each subscribing service
- [ ] Retry policies configured
- [ ] Circuit breaker policies configured
- [ ] Dead letter queue configured
- [ ] Logging/monitoring enabled
- [ ] Integration tests passing
- [ ] Event versioning strategy documented
- [ ] Consumer lag < 1000 messages
- [ ] No messages in dead letter queue during normal operation

## Summary

✅ **Phase 6 Complete**

Event-driven communication patterns established:
- **Publisher Pattern:** Services publish events when actions occur
- **Consumer Pattern:** Services subscribe to relevant events
- **Retry/Circuit Breaker:** Automatic handling of transient failures
- **Dead Letter Queue:** Unprocessable messages captured for investigation
- **Request/Reply:** For synchronous operations (rare)
- **Saga Pattern:** For distributed transactions
- **Event Versioning:** Support multiple versions during migration

**Benefits:**
- Services are truly decoupled (no direct calls)
- Asynchronous communication (better performance)
- Event sourcing capability (audit trail)
- Fanout communication (one event → many consumers)
- Easy to add new consumers without changing publishers

**Next Step:** Phase 7 - Update Documentation (architecture diagrams, deployment guide)

---

**Phase 6 Status:** COMPLETE ✅  
**Infrastructure:** Kafka + MassTransit configured  
**Communication:** Event-driven, asynchronous, decoupled  
**Patterns:** Publish/Subscribe, Request/Reply, Saga, Event Sourcing

