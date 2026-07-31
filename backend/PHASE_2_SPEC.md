# Phase 2 Spec: Appointment Reminders & Notifications Sprint

## Overview
Expand Phase 1 appointment service with intelligent, multi-channel reminders and notifications. Build on existing infrastructure (ReminderBackgroundService, NotificationOrchestrator, outbox pattern) to deliver reliable, user-preference-aware reminders via Email, SMS, Push, and In-App channels.

**Timeline**: Week 2+ (Conservative approach: Phase 1 staging 24h → production, then Phase 2)

## Phase 2 Scope (4 Sprints)

### Sprint 1: Reminder Rules & Scheduling
**Goal**: Enhanced reminder scheduling with configurable rules, user preferences, and timezone support.

#### Features
1. **Reminder Rule Engine**
   - Create/Update/Delete reminder rules per appointment type
   - Rules: Auto-schedule reminders at defined intervals (24h, 4h, 1h before appointment)
   - Conditions: Appointment type, patient preferences, provider settings
   - Support timezone-aware scheduling (user's local time)
   - Allow opt-out/opt-in by channel per user

2. **User Preference Management**
   - Channel preferences (Email, SMS, Push, In-App)
   - Quiet hours (e.g., 9 PM - 8 AM no SMS/Push reminders)
   - Frequency caps (max reminders per day)
   - Optout management (persistent across appointments)

3. **Appointment Event Handlers**
   - Create consumer in Notification Service: `AppointmentScheduledEventConsumer`
   - Listen for `AppointmentScheduledEvent` from Kafka
   - Fetch user preferences + reminder rules
   - Schedule reminders with timezone conversion
   - Store reminder records in notification DB

#### Deliverables
- `ReminderRule` entity + repository
- `UserPreference` entity + repository
- `AppointmentScheduledEventConsumer` Kafka handler
- Reminder scheduling query handler
- Integration tests for rule evaluation

---

### Sprint 2: Notification Persistence & Templates
**Goal**: Centralized notification tracking and template management.

#### Features
1. **Notification Persistence**
   - Use existing `Notification` entity from Notification Service
   - Track all reminders: pending → sent/failed/bounced
   - Retry logic: exponential backoff (2^n seconds, max 3 retries)
   - Dead-letter handling for permanently failed reminders

2. **Template Management**
   - Email templates: HTML with variable substitution
   - SMS templates: Plain text (max 160 chars, multipart support)
   - Push templates: Title, body, action URL
   - In-App templates: Rich formatting
   - Variables: `{PatientName}`, `{ProviderName}`, `{AppointmentDate}`, `{AppointmentTime}`, `{AppointmentType}`, `{Location}`

3. **Template Provider Integration**
   - AWS SES for email
   - Twilio for SMS
   - Firebase Cloud Messaging for push
   - In-App via database (SignalR delivery in frontend)

#### Deliverables
- `NotificationTemplate` entity + seeder
- Template rendering service with variable substitution
- Notification repository enhancements
- Failed notification dead-letter queue handler
- E2E tests for each channel

---

### Sprint 3: Delivery & Retries
**Goal**: Robust reminder delivery with retry logic and monitoring.

#### Features
1. **Smart Delivery**
   - Batch send reminders every 5 minutes (tunable)
   - Circuit breaker for provider outages (SES, Twilio down)
   - Graceful degradation: if SMS fails, retry as email
   - Delivery confirmation: Message IDs tracked per provider

2. **Retry & Dead-Letter**
   - Exponential backoff: 1s, 2s, 4s retry delays
   - Max 3 retries per reminder (configurable)
   - Dead-letter queue for permanently failed reminders
   - Manual re-send capability via admin endpoint

3. **Monitoring & Analytics**
   - Metrics: sent, failed, bounced counts by channel
   - Latency: time from scheduled to delivered
   - Bounce tracking: soft bounces (retry), hard bounces (disable channel)
   - Admin dashboard: Reminder status overview

#### Deliverables
- Enhanced `ReminderBackgroundService` with batch processing
- Retry handler with exponential backoff
- Circuit breaker implementation
- Dead-letter processor
- Prometheus metrics exporter
- Admin API endpoints: View reminders, re-send, disable

---

### Sprint 4: Real-Time & User-Facing APIs
**Goal**: User-visible reminder management and real-time in-app notifications.

#### Features
1. **Reminder Management APIs**
   - GET `/reminders` — List upcoming reminders (paginated)
   - PUT `/reminders/{id}/preferences` — Update user preferences
   - PUT `/reminders/{id}/rules` — Customize reminder rules
   - DELETE `/reminders/{id}` — Opt-out of specific reminder
   - POST `/reminders/{id}/send-now` — Manually trigger reminder (admin)

2. **In-App Notifications**
   - SignalR real-time delivery of sent/failed reminders
   - Notification tray: Unread count, mark as read
   - Notification history: Last 30 days searchable
   - Notification detail: Show retry history, error details

3. **Frontend Integration**
   - Notification bell icon with count badge
   - Reminder preferences modal (Email, SMS, Push, InApp)
   - Quiet hours time picker
   - Opt-out by channel/appointment type
   - Resend reminder button (if manually triggered)

#### Deliverables
- Reminder management controllers + handlers
- SignalR hub for in-app notifications
- Notification tray UI component
- Preferences modal component
- Integration tests for all endpoints

---

## Architecture Decisions

### 1. Event-Driven Approach
- Appointment Service publishes `AppointmentScheduledEvent` to Kafka
- Notification Service consumes event
- Notification Service owns reminder storage + delivery
- Decouples appointment creation from reminder scheduling

### 2. User Preferences Service
- Centralized preferences authority
- Shared by all notification types (appointments, prescriptions, billing)
- Cached with TTL (5 minutes) to reduce DB queries
- Thread-safe with distributed locking (Redis)

### 3. Timezone Handling
- Store reminder times in UTC in database
- Convert to user's timezone for display
- Respect user's timezone when calculating "24h before"
- Use user's timezone from Identity Service

### 4. Graceful Degradation
- If email provider fails: try SMS, then in-app
- If SMS provider fails: try email, then in-app
- If all external channels fail: store in-app notification
- Never lose reminders — all failures are retried

### 5. Privacy & HIPAA
- Never log full email addresses or phone numbers
- Log only message IDs (provider-assigned)
- Encrypted at-rest notifications in database (sensitive data)
- Audit trail: Who sent, when, channel, success/failure

---

## Technical Dependencies

### New Services/Components
1. **Reminder Rule Engine** — Custom scheduling logic
2. **User Preference Service** — Centralized preference management
3. **Template Renderer** — Variable substitution engine
4. **Circuit Breaker** — Fallback provider logic
5. **SignalR Hub** — Real-time in-app notifications

### Existing Infrastructure Used
- Kafka (event streaming)
- Outbox pattern (event durability)
- BackgroundService (reminder polling)
- Entity Framework Core (persistence)
- Redis (preference caching)
- AWS SES, Twilio, Firebase (providers)

---

## Data Models

### ReminderRule
```csharp
public class ReminderRule : AuditableEntity
{
    public Guid AppointmentTypeId { get; set; }
    public int MinutesBefore { get; set; } // 1440 (24h), 240 (4h), 60 (1h)
    public ReminderType Channel { get; set; } // Email, SMS, Push, InApp
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; } // Apply to all users unless overridden
}
```

### UserPreference
```csharp
public class UserPreference : AuditableEntity
{
    public Guid UserId { get; set; }
    public bool EmailRemindersEnabled { get; set; }
    public bool SmsRemindersEnabled { get; set; }
    public bool PushRemindersEnabled { get; set; }
    public bool InAppRemindersEnabled { get; set; }
    public TimeSpan QuietHourStart { get; set; } // 21:00
    public TimeSpan QuietHourEnd { get; set; } // 08:00
    public int MaxRemindersPerDay { get; set; }
    public string? TimeZoneId { get; set; } // Asia/Kolkata, America/New_York
}
```

### NotificationTemplate
```csharp
public class NotificationTemplate : AuditableEntity
{
    public string Name { get; set; }
    public ReminderType Channel { get; set; }
    public string Subject { get; set; } // For email
    public string Body { get; set; }
    public string? ImageUrl { get; set; } // For push
    public string? ActionUrl { get; set; } // For push/in-app
    public bool IsActive { get; set; }
}
```

---

## API Contracts

### Reminder Management
```
GET /api/v1/reminders
Response:
{
  "data": [
    {
      "id": "guid",
      "appointmentId": "guid",
      "scheduledTime": "2026-08-15T10:00:00Z",
      "channel": "Email",
      "status": "Scheduled|Sent|Failed|Cancelled",
      "sentAt": "2026-08-15T10:05:00Z",
      "failureReason": null
    }
  ],
  "totalCount": 25,
  "pageSize": 10,
  "pageNumber": 1
}
```

### User Preferences
```
PUT /api/v1/preferences
Request:
{
  "emailRemindersEnabled": true,
  "smsRemindersEnabled": true,
  "pushRemindersEnabled": false,
  "inAppRemindersEnabled": true,
  "quietHourStart": "21:00",
  "quietHourEnd": "08:00",
  "maxRemindersPerDay": 5,
  "timeZoneId": "Asia/Kolkata"
}
```

---

## Testing Strategy

### Unit Tests
- Reminder rule evaluation (matching conditions)
- Timezone conversion
- Template variable substitution
- Retry backoff calculation

### Integration Tests
- Event consumption from Kafka
- Preference fetching + caching
- Notification persistence
- Provider API calls (mocked)

### E2E Tests
- Full flow: Appointment → Event → Reminder scheduled → Sent
- Multi-channel fallback (primary fails, secondary succeeds)
- Retry mechanism (failure → retry → success)
- User preferences honored (quiet hours respected)

### Load Tests
- 10K concurrent reminder sends
- Provider API throttling (SES rate limits)
- Database query performance (preference lookups)

---

## Success Criteria

1. **Reliability**: 99.5% of scheduled reminders delivered within 5 minutes
2. **Coverage**: All 4 channels (Email, SMS, Push, InApp) operational
3. **User Control**: Preferences respected (quiet hours, opt-out working)
4. **Timezone**: Correct time shown in all user-facing displays
5. **Monitoring**: Clear visibility into reminder delivery metrics
6. **HIPAA**: No sensitive data in logs/metrics

---

## Risks & Mitigation

| Risk | Impact | Mitigation |
|------|--------|-----------|
| Provider outages (SES, Twilio) | Reminders delayed/failed | Circuit breaker + fallback channels |
| Database query storms (preferences) | Performance degradation | Redis caching + distributed locking |
| Duplicate reminders sent | User confusion | Idempotency key per reminder |
| Timezone data stale | Wrong reminder times | Cache invalidation on user profile update |
| GDPR/Privacy violations | Legal risk | Data encryption, audit logging, retention policy |

---

## Rollout Plan

### Staging (24 hours)
- Deploy to staging environment
- Run smoke tests (all channels, retry logic)
- Monitor: Reminder latency, delivery rates, error rates
- Manual QA: Verify user preferences honored

### Production (Wave 1: 25% users)
- Enable for small cohort (QA team + staff)
- Monitor error rates, delivery metrics
- Collect user feedback

### Production (Wave 2: 75% users)
- Expand to broader user base
- Monitor performance + error rates

### Production (Wave 3: 100% users)
- Full rollout
- Continue monitoring metrics

---

## Next Steps (Week 1)
1. Design Phase 2 task breakdown (sprints 1-4)
2. Create database migration scripts
3. Implement AppointmentScheduledEventConsumer
4. Build ReminderRule + UserPreference services
5. Add integration tests for reminder scheduling
