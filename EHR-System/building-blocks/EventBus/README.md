# EventBus Package

Event publishing, messaging, and outbox patterns.

## Contents (44 files)

### Core Events (2 files)
- `IntegrationEvent.cs` - Cross-service events
- `DomainEvent.cs` - Domain events

### Event Handler (1 file)
- `IntegrationEventHandler.cs` - Handler contract

### Message Broker (4 files)
- `IEventBusPublisher.cs` - Event publishing
- `IEventBusSubscriber.cs` - Event subscription
- `IMessageBroker.cs` - Broker abstraction
- `BrokerHealthStatus.cs` - Health status

### Domain Events (15 files)
- **Patient**: PatientCreatedEvent, PatientUpdatedEvent, PatientDeletedEvent
- **Appointment**: AppointmentScheduledEvent, AppointmentCancelledEvent, AppointmentRescheduledEvent
- **Clinical**: DiagnosisRecordedEvent, PrescriptionIssuedEvent, MedicalRecordUpdatedEvent
- **Billing**: InvoiceGeneratedEvent, PaymentProcessedEvent, BillingCycleClosedEvent
- **Notification**: NotificationSentEvent, ReminderScheduledEvent, AlertRaisedEvent

### Outbox Pattern (13 files)
- `IOutboxService.cs` - Outbox management
- `IOutboxPoller.cs` - Message polling
- `OutboxPollerStats.cs` - Polling statistics
- `IOutboxMessagePublisher.cs` - Message publishing
- `PublisherHealthStatus.cs` - Publisher health
- `IOutboxEventStore.cs` - Event persistence
- `OutboxEventData.cs` - Event data
- `OutboxStoreStats.cs` - Store statistics
- `OutboxEventStatus.cs` - Event status
- `IOutboxProcessor.cs` - Processing contract
- `OutboxMessage.cs` - Message wrapper
- `OutboxMessageState.cs` - Message state
- `OutboxProcessor.cs` - Processor implementation

---

## Usage

```csharp
using EHRPlatform.EventBus.Events;
using EHRPlatform.EventBus.Broker;
using EHRPlatform.EventBus.Outbox;
```

## Parent

[← Building Blocks](../README.md)
