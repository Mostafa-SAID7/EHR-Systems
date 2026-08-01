# EHR Platform - Microservices Architecture

## Overview

EHR Platform is a true microservices architecture with service-isolated design:
- **Database-per-service:** Each service owns its database (no shared tables)
- **Decoupled domain models:** Services have independent entities
- **Event-driven communication:** Kafka/MassTransit for inter-service messaging
- **HIPAA compliant:** Comprehensive audit logging via event stream
- **Polyglot persistence:** PostgreSQL (primary), MySQL, MongoDB (specialized use cases)

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                              API Gateway (Port 5000)                             │
│                    (Routing, Rate Limiting, Authentication)                      │
└─────────────────────────────┬───────────────────────────────────────────────────┘
                              │
        ┌─────────────────────┼─────────────────────┐
        │                     │                     │
        ▼                     ▼                     ▼
┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐
│ Identity Service │  │ Patient Service  │  │ Clinical Service │
│ (Port 5001)      │  │ (Port 5002)      │  │ (Port 5003)      │
│                  │  │                  │  │                  │
│ DbContext:       │  │ DbContext:       │  │ DbContext:       │
│ IdentityContext  │  │ PatientContext   │  │ ClinicalContext  │
│                  │  │                  │  │                  │
│ Database:        │  │ Database:        │  │ Database:        │
│ ehr_identity_db  │  │ ehr_patient_db   │  │ ehr_clinical_db  │
│ (PostgreSQL)     │  │ (PostgreSQL)     │  │ (PostgreSQL)     │
│                  │  │ ehr_patient_docs │  │ ehr_clinical_docs│
│ Entities:        │  │ (MongoDB)        │  │ (MongoDB)        │
│ • User           │  │                  │  │                  │
│ • Role           │  │ Entities:        │  │ Entities:        │
│ • Permission     │  │ • Patient        │  │ • ClinicalNote   │
│ • RefreshToken   │  │ • Contact        │  │ • VitalSigns     │
│                  │  │ • Allergy        │  │ • Diagnosis      │
│ Events:          │  │ • Condition      │  │                  │
│ • UserCreated    │  │ • Insurance      │  │ Events:          │
│ • UserUpdated    │  │ • EmergencyCtc   │  │ • NoteCreated    │
│ • RoleAssigned   │  │ • MedicalHist    │  │ • VitalsRecorded │
│ • Deactivated    │  │                  │  │ • DiagnosisAdded │
│                  │  │ Events:          │  │                  │
│                  │  │ • Created        │  │                  │
│                  │  │ • Updated        │  │                  │
│                  │  │ • Archived       │  │                  │
│                  │  │ • AllergyAdded   │  │                  │
│                  │  │ • ConditionAdded │  │                  │
│                  │  │ • StatusChanged  │  │                  │
└──────────────────┘  └──────────────────┘  └──────────────────┘
        │                     │                     │
        └─────────────────────┼─────────────────────┘
                              │
                    ┌─────────▼──────────┐
                    │  Kafka Event Bus   │
                    │  (Zookeeper)       │
                    │                    │
                    │ Topics:            │
                    │ • user-events      │
                    │ • patient-events   │
                    │ • clinical-events  │
                    │ • appointment-...  │
                    │ • billing-events   │
                    │ • prescription-... │
                    │ • notification-... │
                    │ • audit-events     │
                    │ • dlq-events       │
                    └─────────┬──────────┘
                              │
        ┌─────────────────────┼─────────────────────┬──────────────────┐
        │                     │                     │                  │
        ▼                     ▼                     ▼                  ▼
┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐  ┌─────────────┐
│ Appointment Svc  │  │ Notification Svc │  │ Audit Service    │  │ Analytics   │
│ (Port 5004)      │  │ (Port 5005)      │  │ (Port 5006)      │  │ (Port 5010) │
│                  │  │                  │  │                  │  │             │
│ Database:        │  │ Database:        │  │ Database:        │  │ Database:   │
│ ehr_appointment_ │  │ ehr_notification │  │ ehr_audit_db     │  │ ehr_analyti │
│ db (PG + MySQL)  │  │ _db (all 3)      │  │ (PostgreSQL)     │  │ _db (all 3) │
│                  │  │                  │  │ ehr_audit_docs   │  │             │
│ Consumers:       │  │ Consumers:       │  │ (MongoDB)        │  │ Consumers:  │
│ • Appointment... │  │ • UserCreated    │  │                  │  │ • All       │
│ • Appointment... │  │ • PatientCreated │  │ Consumers:       │  │   events    │
│ • Clinical...    │  │ • Appointment... │  │ • UserCreated    │  │             │
│ • Invoice...     │  │ • Billing...     │  │ • PatientCreated │  │ Produces:   │
│ • Prescription...│  │ • Prescription...│  │ • Appointment... │  │ • Reports   │
│                  │  │ • Audit...       │  │ • Invoice...     │  │ • Metrics   │
│ Produces:        │  │                  │  │ • Notification...│  │ • Alerts    │
│ • Scheduled      │  │ Produces:        │  │                  │  │             │
│ • Confirmed      │  │ • EmailSent      │  │ Produces:        │  │             │
│ • Cancelled      │  │ • SmsSent        │  │ • DataAccessed   │  │             │
│ • Completed      │  │ • Failed         │  │ • DataModified   │  │             │
│ • Rescheduled    │  │                  │  │ • SecurityEvent  │  │             │
└──────────────────┘  └──────────────────┘  └──────────────────┘  └─────────────┘
        │                     │                     │                  │
        └─────────────────────┼─────────────────────┴──────────────────┘
                              │
                    ┌─────────▼──────────┐
                    │ Outbox Processor   │
                    │ (Port 5009)        │
                    │                    │
                    │ Database:          │
                    │ ehr_outbox_db      │
                    │ (all 3 databases)  │
                    │                    │
                    │ Role:              │
                    │ Processes outbox   │
                    │ Ensures reliability│
                    └────────────────────┘
```

## Service Layer Details

### Identity Service (Port 5001)
**Responsibility:** User authentication, authorization, role management

**Database:** PostgreSQL (ehr_identity_db)

**Entities:**
- User: Core user account data (email, password hash, status)
- Role: Permission groupings (Admin, Doctor, Nurse, Patient)
- Permission: Fine-grained access control (read_patient, create_appointment, etc.)
- UserRole: Many-to-many relationship
- RolePermission: Many-to-many relationship
- RefreshToken: JWT token refresh management

**Events Published:**
- UserCreatedEvent
- UserUpdatedEvent
- UserRoleAssignedEvent
- UserDeactivatedEvent

**Events Consumed:**
- None (produces only)

---

### Patient Service (Port 5002)
**Responsibility:** Patient master data, demographics, contacts, medical history

**Database:**
- PostgreSQL (ehr_patient_db) - Relational data
- MongoDB (ehr_patient_documents) - Document storage (scans, images, PDFs)

**Entities:**
- Patient: Master patient record (MRN, demographics, status)
- PatientContact: Primary/secondary/emergency contacts
- PatientAllergy: Allergies with severity and reactions
- PatientCondition: Active/resolved conditions (ICD-10 codes)
- PatientInsurance: Insurance policy information
- PatientEmergencyContact: Emergency contact details
- PatientMedicalHistory: Historical medical summary

**Events Published:**
- PatientCreatedEvent
- PatientUpdatedEvent
- PatientArchivedEvent
- PatientAllergyAddedEvent
- PatientConditionAddedEvent
- PatientStatusChangedEvent

**Events Consumed:**
- UserCreatedEvent (verify user exists)

---

### Clinical Service (Port 5003)
**Responsibility:** Clinical notes, vital signs, diagnoses, treatment plans

**Database:**
- PostgreSQL (ehr_clinical_db) - Structured clinical data
- MongoDB (ehr_clinical_documents) - Unstructured notes, attachments

**Entities:**
- ClinicalNote: Provider-authored clinical documentation
- VitalSigns: Heart rate, blood pressure, temperature, SpO2, etc.
- Diagnosis: ICD-10 diagnoses with status (active, resolved)
- TreatmentPlan: Care recommendations
- LabResult: Lab findings with normal/abnormal indicators

**Events Published:**
- ClinicalNoteCreatedEvent
- VitalSignsRecordedEvent
- DiagnosisCreatedEvent

**Events Consumed:**
- PatientCreatedEvent (initialize clinical profile)
- AppointmentCompletedEvent (trigger note entry)

---

### Appointment Service (Port 5004)
**Responsibility:** Appointment scheduling, confirmations, cancellations

**Database:**
- PostgreSQL (ehr_appointment_db)
- MySQL (ehr_appointment_db) - Redundancy for high availability

**Entities:**
- Appointment: Scheduled appointments with provider, time, type
- AppointmentSlot: Available time slots
- AppointmentHistory: Audit trail of changes

**Events Published:**
- AppointmentScheduledEvent
- AppointmentConfirmedEvent
- AppointmentCancelledEvent
- AppointmentCompletedEvent
- AppointmentRescheduledEvent

**Events Consumed:**
- UserCreatedEvent (validate provider)
- PatientCreatedEvent (validate patient)
- NotificationFailedEvent (retry notification)

---

### Billing Service (Port 5007)
**Responsibility:** Invoicing, payments, claims processing

**Database:**
- PostgreSQL (ehr_billing_db)
- MySQL (ehr_billing_db) - Financial data redundancy

**Entities:**
- Invoice: Bill generated for services
- Payment: Payment records (cash, credit card, insurance)
- Claim: Insurance claim submission
- AdjustmentReason: Discounts, write-offs, adjustments

**Events Published:**
- InvoiceGeneratedEvent
- PaymentReceivedEvent
- InvoiceOverdueEvent
- PaymentFailedEvent

**Events Consumed:**
- AppointmentCompletedEvent (trigger invoicing)
- PatientCreatedEvent (setup billing profile)

---

### Prescription Service (Port 5008)
**Responsibility:** Medication management, refill requests, pharmacy coordination

**Database:**
- PostgreSQL (ehr_prescription_db)
- MongoDB (ehr_prescription_documents) - Pharmacy communications

**Entities:**
- Prescription: Medication order with dosage, frequency, refills
- PrescriptionItem: Individual medication items
- PharmacyFulfillment: Pharmacy fill status

**Events Published:**
- PrescriptionCreatedEvent
- PrescriptionFilledEvent
- PrescriptionRefillRequestedEvent
- PrescriptionRefillApprovedEvent
- PrescriptionCancelledEvent

**Events Consumed:**
- ClinicalNoteCreatedEvent (extract prescriptions)
- PatientCreatedEvent (verify patient)

---

### Notification Service (Port 5005)
**Responsibility:** Email, SMS, push notifications, alerting

**Database:**
- PostgreSQL (ehr_notification_db)
- MySQL (ehr_notification_db)
- MongoDB (ehr_notification_documents) - Notification templates, history

**Entities:**
- Notification: Email, SMS, push notification records
- NotificationTemplate: Email/SMS templates
- NotificationLog: Delivery history

**Events Consumed:**
- UserCreatedEvent → Send welcome email
- PatientCreatedEvent → Send patient registration confirmation
- AppointmentScheduledEvent → Send appointment reminder
- InvoiceGeneratedEvent → Send invoice via email
- PrescriptionFilledEvent → Notify patient pickup ready
- AllEvents → Send alerts for critical failures

**Events Published:**
- EmailNotificationSentEvent
- SmsNotificationSentEvent
- NotificationFailedEvent

---

### Audit Service (Port 5006)
**Responsibility:** Compliance logging, data access tracking, security incidents

**Database:**
- PostgreSQL (ehr_audit_db) - Audit logs
- MongoDB (ehr_audit_documents) - Document storage for large payloads

**Entities:**
- AuditLog: Comprehensive log of all changes (who, what, when, where, why)
- SecurityIncident: Policy violations, failed auth attempts
- DataAccessLog: Data retrieval events (PHI access for compliance)

**Events Consumed:**
- **ALL EVENTS** - Complete audit trail for HIPAA compliance
  - UserCreatedEvent → Log user creation
  - PatientCreatedEvent → Log patient registration
  - AppointmentScheduledEvent → Log appointment
  - InvoiceGeneratedEvent → Log financial transaction
  - PrescriptionCreatedEvent → Log medication order
  - EmailNotificationSentEvent → Log communication
  - NotificationFailedEvent → Log failures

**Events Published:**
- DataAccessLoggedEvent
- DataModificationLoggedEvent
- SecurityIncidentLoggedEvent

---

### Analytics Service (Port 5010)
**Responsibility:** Reporting, metrics, dashboards, trend analysis

**Database:**
- PostgreSQL (ehr_analytics_db)
- MySQL (ehr_analytics_db) - Time-series data
- MongoDB (ehr_analytics_documents) - Aggregated reports

**Entities:**
- AnalyticsReport: Generated reports
- MetricData: Daily/weekly/monthly aggregates
- Dashboard: User-defined dashboard configurations

**Events Consumed:**
- PatientCreatedEvent → Update patient demographics report
- AppointmentCompletedEvent → Update appointment metrics
- InvoiceGeneratedEvent → Update revenue metrics
- PrescriptionCreatedEvent → Update prescription trends

**Events Published:**
- ReportGeneratedEvent
- MetricsAggregatedEvent
- AnalyticsAlertGeneratedEvent

---

### Outbox Processor (Port 5009)
**Responsibility:** Ensures reliable event publishing (transactional outbox pattern)

**Database:** ehr_outbox_db (PostgreSQL, MySQL, MongoDB)

**Pattern:** Outbox Pattern Implementation
1. Service writes domain event to local outbox table (same transaction)
2. Service commits transaction (guarantees event is in DB)
3. Outbox Processor polls outbox table
4. Processor publishes event to Kafka
5. Processor marks event as published
6. Event guaranteed to be published (no loss)

**Benefits:**
- No distributed transactions needed
- Guaranteed event publishing
- No orphaned events
- Automatic retry on failure

---

## Communication Flow

### Example: Patient Registration Workflow

```
1. API Gateway receives POST /api/patients
   └─> Request routed to Patient Service

2. Patient Service
   ├─> Create patient record in ehr_patient_db
   ├─> Write PatientCreatedEvent to local outbox
   ├─> Commit transaction
   └─> Return 201 Created

3. Outbox Processor
   ├─> Poll ehr_outbox_db for unpublished events
   ├─> Find PatientCreatedEvent
   ├─> Publish to patient-events topic in Kafka
   └─> Mark as published in outbox

4. Kafka Distribution
   ├─> patient-events topic routes to subscribers
   └─> Multiple services consume based on subscriptions

5. Notification Service consumes PatientCreatedEvent
   ├─> Extract patient email
   ├─> Send welcome email via SMTP
   ├─> Publish EmailNotificationSentEvent
   └─> Write to notification log

6. Audit Service consumes PatientCreatedEvent
   ├─> Extract event details
   ├─> Create AuditLog entry
   ├─> Store in ehr_audit_db + MongoDB
   └─> Publish DataModificationLoggedEvent

7. Analytics Service consumes PatientCreatedEvent
   ├─> Update patient count metric
   ├─> Update demographics report
   ├─> Publish MetricsAggregatedEvent
   └─> Update dashboard

8. Audit Service consumes EmailNotificationSentEvent
   ├─> Create audit entry: "Email sent"
   ├─> Store in audit database
   └─> Publish DataAccessLoggedEvent (HIPAA compliance)
```

### Synchronous Communication (Rare)

When services need immediate responses (booking appointment with availability check):

```
Appointment Service (Request/Reply via MassTransit)
    │
    ├─> Send GetAvailabilityRequest to Appointment Service
    │   (via correlation ID)
    │
    └─> Wait for GetAvailabilityResponse
        ├─> If available: Create appointment
        └─> If not: Return 409 Conflict
```

**Best Practice:** Minimize synchronous calls. Prefer eventual consistency via events.

---

## Resilience & Fault Tolerance

### Retry Policy (Exponential Backoff)
```
Attempt 1: Immediate
Attempt 2: Wait 1 second → Retry
Attempt 3: Wait 2 seconds → Retry
Attempt 4: Wait 4 seconds → Retry

After 3 failures → Dead Letter Queue
```

### Circuit Breaker
```
State: CLOSED (Normal)
    └─> If 5 consecutive failures → Open

State: OPEN (Circuit broken)
    └─> Block all requests for 30 seconds
    └─> Fail fast with circuit breaker exception

State: HALF_OPEN (After 30s)
    └─> Allow 1 test request
    └─> If succeeds → CLOSED
    └─> If fails → OPEN again
```

### Dead Letter Queue
```
Messages that fail after 3 retries
    └─> Moved to dlq-events topic
    └─> Ops team reviews logs
    └─> Manual intervention or fix → Republish
```

---

## Data Consistency

### Eventual Consistency Model
- **No 2-phase commits** across services
- **Each service commits locally** to its database
- **Events guarantee eventual consistency** across all services
- **Compensation transactions** if needed (saga pattern)

### Example: Appointment Booking
1. Appointment Service: Reserves slot (immediate)
2. Billing Service: Creates invoice (eventual, within seconds)
3. Notification Service: Sends confirmation (eventual, within minutes)
4. Audit Service: Logs transaction (eventual, within minutes)

**Trade-off:** Slightly delayed (seconds to minutes) vs. guaranteed no service dependencies

---

## Deployment Architecture

### Single Host Deployment (Development)
```
docker-compose up -d

All services + databases on localhost:
- API Gateway: localhost:5000
- Identity: localhost:5001
- Patient: localhost:5002
- Clinical: localhost:5003
- Appointment: localhost:5004
- Notification: localhost:5005
- Audit: localhost:5006
- Billing: localhost:5007
- Prescription: localhost:5008
- Outbox Processor: localhost:5009
- Analytics: localhost:5010
```

### Kubernetes Deployment (Production)
```
See: k8s/ehr-platform/

Services deployed as:
- Deployments (stateless services)
- StatefulSets (databases, Kafka)
- Services (internal DNS)
- Ingress (external routing)
- ConfigMaps (settings)
- Secrets (credentials)
```

---

## Security

### Authentication
- JWT tokens issued by Identity Service
- Token expiry: 15 minutes (access token)
- Refresh token: 30 days
- Signed with RS256 asymmetric key

### Authorization
- RBAC (Role-Based Access Control)
- Roles: Admin, Doctor, Nurse, Patient
- Permissions: read_patient, create_appointment, etc.
- Enforced at API Gateway + service level

### Data Protection
- Encryption at rest: All databases encrypted
- Encryption in transit: TLS 1.2+ for all connections
- Sensitive fields: Password hash (bcrypt), tokens (JWT)
- PII: Masked in logs, audit trail only

### HIPAA Compliance
- Audit logging: All events captured for 7 years
- Data access logging: Every patient record access logged
- Immutable logs: Audit logs cannot be modified
- De-identification: Patient data removable on request

---

## Monitoring & Observability

### Metrics (Prometheus)
- Request latency: 95th percentile response times
- Throughput: Requests per second per service
- Error rate: 5xx responses, timeouts, circuit breaker trips
- Business metrics: Patient registrations, appointments, revenue

### Logging (ELK Stack)
- Centralized logs from all services
- Structured logging (JSON format)
- Log levels: ERROR, WARN, INFO, DEBUG
- Correlation IDs trace requests across services

### Tracing (Jaeger)
- Distributed tracing from API Gateway → databases
- Trace events across all services
- Identify bottlenecks and latency issues

### Dashboards (Grafana)
- Real-time service health
- Business KPIs
- Alert on anomalies
- Historical trend analysis

---

## Scaling Strategy

### Horizontal Scaling (Add more instances)
```
Patient Service
├─> Instance 1 (Pod 1)
├─> Instance 2 (Pod 2)
└─> Instance 3 (Pod 3)

Load Balancer routes to all 3 instances
Each reads from ehr_patient_db (shared)
```

### Vertical Scaling (Increase resources per instance)
```
Notification Service needs 4GB memory
    └─> Increase container memory limit
    └─> Restart service
    └─> No code changes
```

### Database Scaling

**Read Replicas:**
- ehr_patient_db (Primary) → ehr_patient_db_read_replica
- Read-heavy queries go to replica
- Writes always go to primary

**Partitioning:**
- Patient data by region or date range
- Appointment data by clinic location
- Billing data by payment method

---

## Migration Path (Monolith → Microservices)

### What Changed
✅ **Before (Monolith):**
- Single EHRPlatform.sln solution
- Shared EHRPlatform.Common with all domain models
- Single database (ehr_main)
- Direct service calls (method invocation)

✅ **After (Microservices):**
- Single solution, 10 independent services
- Each service owns domain models + database
- Event-driven communication via Kafka
- No direct service-to-service calls (async only)

### What Stayed the Same
- Single solution structure (EHRPlatform.sln) for easier deployment
- Shared infrastructure in EHRPlatform.Common (Caching, Resilience, Logging)
- Shared DTOs in EHRPlatform.Common/Shared/DTOs (inter-service contracts)

### Benefits Achieved
✅ Independent scalability (Patient service scales separately from Appointment)
✅ Technology flexibility (Appointment in MySQL, Patient in PostgreSQL)
✅ Failure isolation (Notification service down ≠ Patient service affected)
✅ Team autonomy (Patient team owns Patient service, database, deployment)
✅ Development speed (Small teams, independent delivery)

---

## Summary

The EHR Platform microservices architecture provides:

| Aspect | Benefit |
|--------|---------|
| **Isolation** | Service failures don't cascade |
| **Scalability** | Scale each service independently |
| **Flexibility** | Different tech stacks per service |
| **Resilience** | Retry + circuit breaker patterns |
| **Compliance** | Complete audit trail for HIPAA |
| **Performance** | Asynchronous event processing |
| **Maintainability** | Clear service boundaries |
| **Testing** | Test each service in isolation |

**Next:** See [DEPLOYMENT.md](./DEPLOYMENT.md) for step-by-step deployment instructions.

