# Phase 2 Task Breakdown: Appointment Reminders Sprint

## Sprint 1: Reminder Rules & Scheduling (8 tasks)

### Task 1.1: Create ReminderRule Entity & Database Migration
**Objective**: Add reminder rules to appointment service domain.
- [ ] Create `ReminderRule.cs` entity in `EHRPlatform.Services.Appointment/Domain/Entities/`
- [ ] Create `AppointmentReminderRuleConfiguration.cs` EF Core configuration
- [ ] Generate migration: `Add-Migration CreateReminderRulesTable`
- [ ] Seed default rules (Email 24h before, SMS 1h before)
- [ ] Create `IReminderRuleRepository` interface
- [ ] Implement `ReminderRuleRepository` with query methods
- [ ] Write unit tests for repository queries
- [ ] Verify migration runs clean on fresh database

**Files**: 
- New: `Domain/Entities/ReminderRule.cs`
- New: `Data/Migrations/[timestamp]_CreateReminderRulesTable.cs`
- New: `Data/Repositories/IReminderRuleRepository.cs`
- New: `Data/Repositories/ReminderRuleRepository.cs`

---

### Task 1.2: Create UserPreference Entity & Repository
**Objective**: Store user reminder preferences (channels, quiet hours, timezone).
- [ ] Create `UserPreference.cs` entity in `EHRPlatform.Services.Appointment/Domain/Entities/`
- [ ] Create `UserPreferenceConfiguration.cs` EF Core configuration
- [ ] Generate migration: `Add-Migration CreateUserPreferencesTable`
- [ ] Create `IUserPreferenceRepository` interface
- [ ] Implement `UserPreferenceRepository` with CRUD + query methods
- [ ] Add caching layer (Redis) with 5-minute TTL
- [ ] Write unit tests for repository + cache layer
- [ ] Add integration tests for cache invalidation

**Files**:
- New: `Domain/Entities/UserPreference.cs`
- New: `Data/Migrations/[timestamp]_CreateUserPreferencesTable.cs`
- New: `Data/Repositories/IUserPreferenceRepository.cs`
- New: `Data/Repositories/UserPreferenceRepository.cs`
- New: `Services/Caching/UserPreferenceCacheService.cs`

---

### Task 1.3: Design & Implement Reminder Rule Engine
**Objective**: Evaluate rules and determine which reminders to schedule.
- [ ] Create `IReminderRuleEngine` interface
- [ ] Implement `ReminderRuleEngine.cs` with rule evaluation logic
- [ ] Support conditions: appointment type, user preferences, timezone
- [ ] Handle timezone-aware scheduling (convert user time to UTC)
- [ ] Implement rule matching algorithm (priority: user override → default)
- [ ] Add unit tests for all rule combinations
- [ ] Add integration tests with real appointment + user data
- [ ] Performance test with 10K rules loaded

**Files**:
- New: `Services/Scheduling/IReminderRuleEngine.cs`
- New: `Services/Scheduling/ReminderRuleEngine.cs`

---

### Task 1.4: Create AppointmentScheduledEventConsumer
**Objective**: Listen for appointment creation events and schedule reminders.
- [ ] Create Kafka consumer: `AppointmentScheduledEventConsumer.cs`
- [ ] Register consumer in Notification Service `Program.cs`
- [ ] On event received: fetch user preferences + appointment details
- [ ] Evaluate reminder rules based on appointment type
- [ ] Create `AppointmentReminder` records for scheduled reminders
- [ ] Publish `ReminderScheduledEvent` to Kafka
- [ ] Add error handling + retry logic
- [ ] Write integration tests (event → reminders created)

**Files**:
- New: `Services/Consumers/AppointmentScheduledEventConsumer.cs`
- Modify: `Program.cs` (register consumer)
- New: `Domain/Events/ReminderScheduledEvent.cs`

---

### Task 1.5: Create Reminder Scheduling Query Handler
**Objective**: Provide API to manually schedule reminders for an appointment.
- [ ] Create `ScheduleReminderCommand.cs` CQRS command
- [ ] Create `ScheduleReminderCommandHandler.cs` handler
- [ ] Validate: Appointment exists, time is before appointment
- [ ] Create `AppointmentReminder` with status = Scheduled
- [ ] Publish `ReminderScheduledEvent` to Kafka
- [ ] Add unit tests for validation logic
- [ ] Add integration tests for full flow

**Files**:
- New: `Features/Reminders/Commands/ScheduleReminderCommand.cs`
- New: `Features/Reminders/Commands/ScheduleReminderCommandHandler.cs`

---

### Task 1.6: Update ReminderBackgroundService with Rule Engine
**Objective**: Integrate rule engine into background service.
- [ ] Inject `ReminderRuleEngine` into `ReminderBackgroundService`
- [ ] On app startup: load all active `ReminderRule`s into memory
- [ ] When processing reminders: evaluate rules for new appointments
- [ ] Schedule default reminders if not explicitly set
- [ ] Add monitoring: Log reminders scheduled, time taken
- [ ] Add performance test: Process 1000 appointments in < 5 seconds

**Files**:
- Modify: `Services/ReminderBackgroundService.cs`

---

### Task 1.7: Create GetUserPreferencesQuery Handler
**Objective**: API endpoint to fetch user reminder preferences.
- [ ] Create `GetUserPreferencesQuery.cs`
- [ ] Create `GetUserPreferencesQueryHandler.cs`
- [ ] Return user preferences (channels, quiet hours, timezone)
- [ ] Use cache layer for performance
- [ ] Add error handling for missing user
- [ ] Write integration tests

**Files**:
- New: `Features/Preferences/Queries/GetUserPreferencesQuery.cs`
- New: `Features/Preferences/Queries/GetUserPreferencesQueryHandler.cs`
- New: `Features/Preferences/Queries/GetUserPreferencesQueryResponse.cs`

---

### Task 1.8: Create UpdateUserPreferencesCommand Handler
**Objective**: API endpoint to update reminder preferences.
- [ ] Create `UpdateUserPreferencesCommand.cs`
- [ ] Create `UpdateUserPreferencesCommandHandler.cs`
- [ ] Validate: timezone is valid IANA ID
- [ ] Update `UserPreference` in database
- [ ] Invalidate cache for user
- [ ] Publish `UserPreferencesChangedEvent`
- [ ] Add unit tests for validation
- [ ] Write integration tests

**Files**:
- New: `Features/Preferences/Commands/UpdateUserPreferencesCommand.cs`
- New: `Features/Preferences/Commands/UpdateUserPreferencesCommandHandler.cs`
- New: `Domain/Events/UserPreferencesChangedEvent.cs`

---

## Sprint 2: Notification Persistence & Templates (6 tasks)

### Task 2.1: Create NotificationTemplate Entity
**Objective**: Store reminder templates for all channels.
- [ ] Create `NotificationTemplate.cs` entity in Notification Service
- [ ] Create `NotificationTemplateConfiguration.cs` EF Core config
- [ ] Generate migration: `Add-Migration CreateNotificationTemplatesTable`
- [ ] Seed templates:
   - Email: "Appointment Reminder - {AppointmentDate}"
   - SMS: "Reminder: {AppointmentType} at {AppointmentTime}"
   - Push: Title: "Appointment Reminder", Body: "At {AppointmentTime}"
   - InApp: Full HTML template with appointment details
- [ ] Create `INotificationTemplateRepository` interface
- [ ] Implement repository

**Files**:
- New: `Domain/Entities/NotificationTemplate.cs`
- New: `Data/Migrations/[timestamp]_CreateNotificationTemplatesTable.cs`
- New: `Data/Repositories/INotificationTemplateRepository.cs`
- New: `Data/Repositories/NotificationTemplateRepository.cs`

---

### Task 2.2: Create Template Variable Substitution Service
**Objective**: Render templates with appointment/user data.
- [ ] Create `ITemplateRenderer` interface
- [ ] Implement `TemplateRenderer.cs` with variable substitution
- [ ] Support variables: PatientName, ProviderName, AppointmentDate, AppointmentTime, AppointmentType, Location
- [ ] Handle formatting: dates in user timezone, time in 12h/24h format
- [ ] Add HTML escaping for HTML templates
- [ ] Add SMS length validation (160 chars per segment)
- [ ] Write comprehensive unit tests for all variable combinations

**Files**:
- New: `Services/Templates/ITemplateRenderer.cs`
- New: `Services/Templates/TemplateRenderer.cs`

---

### Task 2.3: Enhance Notification Entity with Retry Logic
**Objective**: Add retry tracking to Notification entity.
- [ ] Add to `Notification.cs`: RetryCount, MaxRetries, LastRetryAt, FailureReason
- [ ] Add method: `CalculateNextRetryTime()` with exponential backoff
- [ ] Add method: `CanRetry()` checks if retries remaining
- [ ] Generate migration: `Add-Migration EnhanceNotificationWithRetries`
- [ ] Write unit tests for retry calculation
- [ ] Verify backward compatibility with existing notifications

**Files**:
- Modify: `Domain/Entities/Notification.cs`
- New: `Data/Migrations/[timestamp]_EnhanceNotificationWithRetries.cs`

---

### Task 2.4: Create NotificationEventPublisher
**Objective**: Publish events when notification status changes.
- [ ] Create `INotificationEventPublisher` interface
- [ ] Implement `NotificationEventPublisher.cs` to publish:
   - `NotificationSentEvent`
   - `NotificationFailedEvent`
   - `NotificationBouncedEvent`
   - `NotificationRetryScheduledEvent`
- [ ] Integrate into notification handlers
- [ ] Add unit tests for event publishing

**Files**:
- New: `Services/Events/INotificationEventPublisher.cs`
- New: `Services/Events/NotificationEventPublisher.cs`
- New: `Domain/Events/NotificationEvents.cs` (all event classes)

---

### Task 2.5: Create DeadLetterQueueHandler
**Objective**: Handle permanently failed reminders.
- [ ] Create `DeadLetterQueueHandler.cs` as BackgroundService
- [ ] Query notifications with status = "Failed" and MaxRetries exceeded
- [ ] Move to dead-letter queue table
- [ ] Log failure details for manual review
- [ ] Create admin API to re-send from DLQ
- [ ] Write integration tests

**Files**:
- New: `Services/Background/DeadLetterQueueHandler.cs`
- New: `Data/Migrations/[timestamp]_CreateDeadLetterQueueTable.cs`
- Modify: `Program.cs` (register service)

---

### Task 2.6: Create Template Management API
**Objective**: Admin endpoints for template CRUD.
- [ ] Create `GetNotificationTemplatesQuery.cs`
- [ ] Create `GetNotificationTemplateByIdQuery.cs`
- [ ] Create `CreateNotificationTemplateCommand.cs`
- [ ] Create `UpdateNotificationTemplateCommand.cs`
- [ ] Create `DeleteNotificationTemplateCommand.cs`
- [ ] Create `PreviewTemplateCommand.cs` (render with sample data)
- [ ] Add authorization (Admin only)
- [ ] Write integration tests

**Files**:
- New: `Features/Templates/Queries/*.cs`
- New: `Features/Templates/Commands/*.cs`

---

## Sprint 3: Delivery & Retries (5 tasks)

### Task 3.1: Enhance ReminderBackgroundService with Batch Processing
**Objective**: Improve reminder processing with batching and circuit breaker.
- [ ] Modify `ProcessRemindersAsync()` to batch by channel
- [ ] Add `BatchSize` configuration (default: 100 per batch)
- [ ] Implement `ICircuitBreaker` pattern for provider failures
- [ ] On provider failure: increment circuit breaker, retry with fallback channel
- [ ] Add metrics: Sent count, failed count, latency per channel
- [ ] Write integration tests for circuit breaker logic

**Files**:
- Modify: `Services/ReminderBackgroundService.cs`
- New: `Services/CircuitBreaker/ICircuitBreaker.cs`
- New: `Services/CircuitBreaker/CircuitBreakerImpl.cs`

---

### Task 3.2: Implement Exponential Backoff Retry Handler
**Objective**: Retry failed reminders with exponential backoff.
- [ ] Create `IRetryHandler` interface
- [ ] Implement `ExponentialBackoffRetryHandler.cs`
- [ ] Delay formula: 2^attempt seconds (1s, 2s, 4s max)
- [ ] Stop retrying if MaxRetries exceeded
- [ ] Move to dead-letter queue on final failure
- [ ] Write unit tests for backoff calculation

**Files**:
- New: `Services/Retry/IRetryHandler.cs`
- New: `Services/Retry/ExponentialBackoffRetryHandler.cs`

---

### Task 3.3: Create Provider Fallback Logic
**Objective**: Degrade gracefully when primary provider fails.
- [ ] Implement fallback chain: Email → SMS → InApp
- [ ] Track failed providers per reminder
- [ ] Skip failed providers on next attempt
- [ ] Log all fallback decisions
- [ ] Write integration tests for all fallback scenarios

**Files**:
- New: `Services/Providers/ProviderFallbackHandler.cs`
- Modify: `Services/Notifications/NotificationOrchestrator.cs`

---

### Task 3.4: Create Prometheus Metrics Exporter
**Objective**: Export reminder delivery metrics for monitoring.
- [ ] Create `ReminderMetricsCollector.cs`
- [ ] Export metrics:
   - `reminders_sent_total` (counter by channel)
   - `reminders_failed_total` (counter by channel)
   - `reminders_delivery_latency_seconds` (histogram)
   - `circuit_breaker_state` (gauge: open/closed/half-open)
- [ ] Integrate with Prometheus endpoint
- [ ] Create Grafana dashboard
- [ ] Write tests for metric updates

**Files**:
- New: `Services/Metrics/ReminderMetricsCollector.cs`
- New: `Monitoring/reminder-metrics-dashboard.json` (Grafana)

---

### Task 3.5: Create Admin Re-send API
**Objective**: Manual reminder re-send for operations/support.
- [ ] Create `ResendReminderCommand.cs`
- [ ] Create `ResendReminderCommandHandler.cs`
- [ ] Validate: Reminder exists, not already sent
- [ ] Re-send immediately (bypass scheduling)
- [ ] Update `SentAt` and `MessageId`
- [ ] Log audit trail (who, when, reason)
- [ ] Add authorization (Admin only)
- [ ] Write integration tests

**Files**:
- New: `Features/Reminders/Commands/ResendReminderCommand.cs`
- New: `Features/Reminders/Commands/ResendReminderCommandHandler.cs`

---

## Sprint 4: Real-Time & User-Facing APIs (5 tasks)

### Task 4.1: Create Reminder Management Query Handler
**Objective**: List upcoming reminders for a user.
- [ ] Create `GetUpcomingRemindersQuery.cs`
- [ ] Create `GetUpcomingRemindersQueryHandler.cs`
- [ ] Query pending reminders for user's appointments
- [ ] Include: AppointmentDate, ReminderTime, Channel, Status
- [ ] Support pagination (page, pageSize)
- [ ] Support filtering (channel, status)
- [ ] Write integration tests

**Files**:
- New: `Features/Reminders/Queries/GetUpcomingRemindersQuery.cs`
- New: `Features/Reminders/Queries/GetUpcomingRemindersQueryHandler.cs`
- New: `Features/Reminders/Queries/GetUpcomingRemindersResponse.cs`

---

### Task 4.2: Create Opt-Out/In Commands
**Objective**: User control over reminder channels.
- [ ] Create `OptOutReminderChannelCommand.cs`
- [ ] Create `OptInReminderChannelCommand.cs`
- [ ] Update `UserPreference` entity to track opt-outs
- [ ] Validate: Can't opt-out of all channels
- [ ] Persist opt-out decision
- [ ] Write integration tests

**Files**:
- New: `Features/Reminders/Commands/OptOutReminderChannelCommand.cs`
- New: `Features/Reminders/Commands/OptInReminderChannelCommand.cs`

---

### Task 4.3: Create SignalR Hub for In-App Notifications
**Objective**: Real-time in-app notification delivery.
- [ ] Create `NotificationHub.cs` SignalR hub
- [ ] Register hub in `Program.cs`
- [ ] Implement `OnReminderSent` method
- [ ] Implement `OnReminderFailed` method
- [ ] Track connected clients (store userId → connectionId)
- [ ] Broadcast to specific user when reminder sent
- [ ] Add connection/disconnection logging
- [ ] Write integration tests

**Files**:
- New: `Hubs/NotificationHub.cs`
- Modify: `Program.cs` (configure SignalR)

---

### Task 4.4: Create Reminder Status Controller
**Objective**: HTTP API for reminder management.
- [ ] Create `RemindersController.cs` in Appointment Service
- [ ] Endpoints:
   - GET `/reminders` — List upcoming reminders
   - GET `/reminders/{id}` — Get reminder details
   - POST `/reminders/{id}/preferences` — Update preferences
   - DELETE `/reminders/{id}` — Opt-out
   - POST `/reminders/{id}/send-now` — Manual send (admin)
- [ ] Add authorization (user can only view own reminders, admin special access)
- [ ] Write integration tests

**Files**:
- New: `Controllers/RemindersController.cs`

---

### Task 4.5: Create Notification History Query
**Objective**: User-visible reminder history.
- [ ] Create `GetReminderHistoryQuery.cs`
- [ ] Create `GetReminderHistoryQueryHandler.cs`
- [ ] Query sent reminders for past 30 days
- [ ] Include: Channel, Sent time, Status, Error details (if failed)
- [ ] Support filtering (channel, date range)
- [ ] Support pagination
- [ ] Add caching (1-hour TTL)
- [ ] Write integration tests

**Files**:
- New: `Features/Reminders/Queries/GetReminderHistoryQuery.cs`
- New: `Features/Reminders/Queries/GetReminderHistoryQueryHandler.cs`
- New: `Features/Reminders/Queries/GetReminderHistoryResponse.cs`

---

## Testing Summary

| Task | Unit Tests | Integration Tests | E2E Tests |
|------|-----------|------------------|----------|
| 1.1-1.2 | 8 | 6 | — |
| 1.3-1.5 | 12 | 8 | 4 |
| 1.6-1.8 | 10 | 10 | 2 |
| 2.1-2.6 | 15 | 12 | 2 |
| 3.1-3.5 | 14 | 10 | 4 |
| 4.1-4.5 | 12 | 12 | 4 |
| **Total** | **71** | **58** | **16** |

---

## Deliverables by Sprint

### Sprint 1 Deliverables
- [ ] ReminderRule entity + repository
- [ ] UserPreference entity + repository + cache layer
- [ ] ReminderRuleEngine implementation
- [ ] AppointmentScheduledEventConsumer
- [ ] Reminder scheduling command + handler
- [ ] Enhanced ReminderBackgroundService
- [ ] User preference query + command handlers
- [ ] 25+ unit tests
- [ ] 10+ integration tests

### Sprint 2 Deliverables
- [ ] NotificationTemplate entity + seeder
- [ ] TemplateRenderer service
- [ ] Enhanced Notification entity with retries
- [ ] NotificationEventPublisher
- [ ] DeadLetterQueueHandler
- [ ] Template management API
- [ ] 20+ unit tests
- [ ] 12+ integration tests

### Sprint 3 Deliverables
- [ ] Enhanced ReminderBackgroundService with batching
- [ ] ExponentialBackoffRetryHandler
- [ ] ProviderFallbackHandler
- [ ] Prometheus metrics exporter
- [ ] Admin re-send API
- [ ] 14+ unit tests
- [ ] 10+ integration tests
- [ ] Grafana dashboard

### Sprint 4 Deliverables
- [ ] Reminder management query handlers
- [ ] Opt-out/in commands
- [ ] SignalR NotificationHub
- [ ] RemindersController with 5 endpoints
- [ ] Reminder history query handler
- [ ] 12+ unit tests
- [ ] 12+ integration tests
- [ ] 4 E2E tests

---

## Definition of Done (Per Task)
- [ ] Code written and peer reviewed
- [ ] Unit tests written and passing (min 80% coverage)
- [ ] Integration tests written and passing
- [ ] Migrations tested on clean database
- [ ] Documentation updated (code comments, API docs)
- [ ] No breaking changes to existing Phase 1 code
- [ ] Builds successfully (`dotnet build` zero errors)
- [ ] Performance acceptable (if applicable, benchmarked)
