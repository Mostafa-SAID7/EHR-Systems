# EHR Platform - Microservices Backend

**Production-grade, service-isolated microservices architecture for healthcare**

True microservices with independent databases, decoupled domain models, and event-driven communication via Kafka.

---

## 🏗️ Architecture Overview

**10 Independent Microservices** with true service isolation:

```
┌─────────────────────────────────────────────────────────────────┐
│                       API Gateway (Port 5000)                    │
│              (Routing, Rate Limiting, Authentication)             │
└──────────────────────────────────────────────────────────────────┘
                              │
    ┌─────────────────────────┼──────────────────────────┐
    │                         │                          │
    ▼                         ▼                          ▼
┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐
│ Identity Service │  │ Patient Service  │  │ Clinical Service │
│ (Port 5001)      │  │ (Port 5002)      │  │ (Port 5003)      │
│ ehr_identity_db  │  │ ehr_patient_db   │  │ ehr_clinical_db  │
└──────────────────┘  └──────────────────┘  └──────────────────┘
        │                     │                      │
        └─────────────────────┼──────────────────────┘
                              │
                    ┌─────────▼──────────┐
                    │  Kafka Event Bus   │
                    │ (9+ Topics)        │
                    └────────────────────┘
                              │
    ┌─────────────────────────┼──────────────────────────┐
    │                         │                          │
    ▼                         ▼                          ▼
┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐
│ Appointment Svc  │  │ Notification Svc │  │ Audit Service    │
│ (Port 5004)      │  │ (Port 5005)      │  │ (Port 5006)      │
│ ehr_appointment_ │  │ ehr_notification │  │ ehr_audit_db     │
│ db (PG + MySQL)  │  │ _db (all 3)      │  │ (PostgreSQL)     │
└──────────────────┘  └──────────────────┘  └──────────────────┘
```

**Key Architecture Features:**
- ✅ **Database-per-service:** Each service owns its database (no sharing)
- ✅ **Decoupled domain models:** Independent entities, no shared DTOs (only events)
- ✅ **Event-driven:** Kafka + MassTransit for asynchronous communication
- ✅ **Polyglot persistence:** PostgreSQL (primary), MySQL, MongoDB (specialized)
- ✅ **HIPAA compliant:** Comprehensive audit logging via event stream
- ✅ **Resilient:** Retry policies, circuit breakers, dead letter queues
- ✅ **Observable:** Prometheus metrics, Grafana dashboards, centralized logging

---


```
backend/
├── src/
│   ├── EHRPlatform.ApiGateway/              # API Gateway (Port 5000)
│   │   ├── Controllers/
│   │   ├── Middleware/
│   │   └── Program.cs
│   │
│   ├── EHRPlatform.Common/                  # Shared infrastructure only
│   │   ├── Infrastructure/
│   │   │   ├── Caching/
│   │   │   ├── Resilience/
│   │   │   ├── Logging/
│   │   │   └── Messaging/
│   │   └── Shared/DTOs/                     # Inter-service DTOs + Events
│   │       ├── UserDto.cs
│   │       ├── PatientDto.cs
│   │       └── ...+ 30+ Event DTOs
│   │
│   ├── EHRPlatform.Services.Identity/       # Identity Service (Port 5001)
│   │   ├── Domain/Entities/
│   │   │   ├── User.cs
│   │   │   ├── Role.cs
│   │   │   ├── Permission.cs
│   │   │   └── RefreshToken.cs
│   │   ├── Data/IdentityContext.cs
│   │   ├── Application/Commands/
│   │   ├── Controllers/
│   │   └── Program.cs
│   │
│   ├── EHRPlatform.Services.Patient/        # Patient Service (Port 5002)
│   │   ├── Domain/Entities/
│   │   │   ├── Patient.cs
│   │   │   ├── PatientContact.cs
│   │   │   ├── PatientAllergy.cs
│   │   │   └── ...
│   │   ├── Data/PatientContext.cs
│   │   ├── Consumers/                       # Event consumers
│   │   ├── Application/Commands/
│   │   ├── Controllers/
│   │   └── Program.cs
│   │
│   ├── EHRPlatform.Services.Clinical/       # Clinical Service (Port 5003)
│   │   ├── Domain/Entities/
│   │   ├── Data/ClinicalContext.cs
│   │   ├── Consumers/
│   │   └── ...
│   │
│   ├── EHRPlatform.Services.Appointment/    # Appointment Service (Port 5004)
│   ├── EHRPlatform.Services.Notification/   # Notification Service (Port 5005)
│   ├── EHRPlatform.Services.Audit/          # Audit Service (Port 5006)
│   ├── EHRPlatform.Services.Billing/        # Billing Service (Port 5007)
│   ├── EHRPlatform.Services.Prescription/   # Prescription Service (Port 5008)
│   ├── EHRPlatform.Services.OutboxProcessor/# Outbox Processor (Port 5009)
│   ├── EHRPlatform.Services.Analytics/      # Analytics Service (Port 5010)
│   │
│   └── EHRPlatform.sln                      # Single solution, 10 services
│
├── db/
│   ├── MICROSERVICES_ISOLATION_PHASE*.md    # Refactoring documentation
│   ├── migrations/
│   │   ├── identity/postgres/
│   │   ├── patient/postgres/
│   │   ├── clinical/postgres/
│   │   └── ...
│   └── POLYGLOT_DATABASE_MIGRATION_GUIDE.md
│
├── init-scripts/
│   ├── postgres-init.sql                    # Create 10 service DBs
│   ├── mysql-init.sql                       # Create 5 service DBs
│   └── mongo-init-services.js               # Create 6 MongoDB databases
│
├── k8s/
│   └── ehr-platform/                        # Kubernetes Helm chart
│       ├── Chart.yaml
│       ├── values.yaml
│       ├── values-prod.yaml
│       └── templates/
│
├── monitoring/
│   ├── prometheus.yml                       # Metrics collection
│   ├── prometheus-rules/alerts.yml          # Alert rules
│   └── grafana-provisioning/                # Dashboards + datasources
│
├── ARCHITECTURE.md                          # Architecture overview
├── DEPLOYMENT.md                            # Deployment guide
├── docker-compose.yml                       # All 10 services + infrastructure
├── Dockerfile                               # Multi-stage build
├── .env                                     # Configuration
└── package.json                             # Build scripts
```

---

## 📊 Database-Per-Service Pattern

Each service owns its database (no shared tables):

| Service | PostgreSQL | MySQL | MongoDB |
|---------|------------|-------|---------|
| Identity | ✅ ehr_identity_db | - | - |
| Patient | ✅ ehr_patient_db | - | ✅ ehr_patient_documents |
| Clinical | ✅ ehr_clinical_db | - | ✅ ehr_clinical_documents |
| Appointment | ✅ ehr_appointment_db | ✅ ehr_appointment_db | - |
| Notification | ✅ ehr_notification_db | ✅ ehr_notification_db | ✅ ehr_notification_docs |
| Audit | ✅ ehr_audit_db | - | ✅ ehr_audit_documents |
| Billing | ✅ ehr_billing_db | ✅ ehr_billing_db | - |
| Prescription | ✅ ehr_prescription_db | - | ✅ ehr_prescription_docs |
| Analytics | ✅ ehr_analytics_db | ✅ ehr_analytics_db | - |
| Outbox Processor | ✅ ehr_outbox_db | ✅ ehr_outbox_db | ✅ ehr_outbox_db |

**Benefits:**
- Each service scales independently
- Breaking schema changes don't affect other services
- Technology choice per service (PostgreSQL, MySQL, MongoDB)
- Clear ownership and accountability

---

## 🔄 Event-Driven Communication

Services communicate asynchronously via Kafka:

```yaml
Topics:
  - patient-events       # PatientCreated, PatientUpdated, etc.
  - user-events          # UserCreated, UserUpdated, etc.
  - appointment-events   # AppointmentScheduled, etc.
  - clinical-events      # ClinicalNoteCreated, etc.
  - billing-events       # InvoiceGenerated, PaymentReceived, etc.
  - prescription-events  # PrescriptionCreated, etc.
  - notification-events  # EmailSent, SmsSent, etc.
  - audit-events         # All events logged for HIPAA
  - dlq-events           # Dead letter queue for failures
```

**Pattern:**
1. Service publishes event to Kafka (local transaction)
2. Consumers subscribe to events
3. No direct service-to-service calls
4. Automatic retry + circuit breaker
5. Complete audit trail for compliance

---

## 🚀 Quick Start (Development)

### Prerequisites
- Docker Desktop (or Docker + Docker Compose)
- .NET 8 SDK (for local development)
- Git

### One-Command Deployment

```bash
cd backend
docker-compose up -d
```

Wait 30-60 seconds for database initialization.

### Verify Services Running

```bash
docker-compose ps

# Expected output:
# ehr-postgres (healthy)
# ehr-mysql (healthy)
# ehr-mongodb (healthy)
# ehr-kafka (healthy)
# ehr-identity-service (running)
# ehr-patient-service (running)
# ... (all 10 services)
```

### Access Services

| Service | URL | Purpose |
|---------|-----|---------|
| API Gateway | http://localhost:5000/swagger | Main entry point |
| Identity Service | http://localhost:5001/swagger | Authentication |
| Patient Service | http://localhost:5002/swagger | Patient management |
| Grafana | http://localhost:3000 | Dashboards (admin/admin) |
| Prometheus | http://localhost:9090 | Metrics |

### Test API

```bash
# Create a patient
curl -X POST http://localhost:5002/api/patients \
  -H "Content-Type: application/json" \
  -d '{
    "mrn": "MRN001",
    "firstName": "John",
    "lastName": "Doe",
    "dateOfBirth": "1990-01-15"
  }'

# Expected: 201 Created with patient ID
```

---

## 🏢 10 Microservices

### 1. Identity Service (Port 5001)
**Responsibility:** User authentication, authorization, JWT tokens

**Database:** PostgreSQL (ehr_identity_db)

**Entities:** User, Role, Permission, RefreshToken

**Events Published:** UserCreatedEvent, UserUpdatedEvent, UserRoleAssignedEvent, UserDeactivatedEvent

---

### 2. Patient Service (Port 5002)
**Responsibility:** Patient master data, demographics, medical history

**Databases:** PostgreSQL (ehr_patient_db) + MongoDB (ehr_patient_documents)

**Entities:** Patient, PatientContact, PatientAllergy, PatientCondition, PatientInsurance

**Events Published:** PatientCreatedEvent, PatientUpdatedEvent, PatientArchivedEvent

---

### 3. Clinical Service (Port 5003)
**Responsibility:** Clinical notes, vital signs, diagnoses

**Databases:** PostgreSQL (ehr_clinical_db) + MongoDB (ehr_clinical_documents)

**Entities:** ClinicalNote, VitalSigns, Diagnosis, LabResult

**Events Published:** ClinicalNoteCreatedEvent, VitalSignsRecordedEvent, DiagnosisCreatedEvent

---

### 4. Appointment Service (Port 5004)
**Responsibility:** Appointment scheduling, confirmations, cancellations

**Databases:** PostgreSQL + MySQL (ehr_appointment_db)

**Entities:** Appointment, AppointmentSlot, AppointmentHistory

**Events Published:** AppointmentScheduledEvent, AppointmentConfirmedEvent, AppointmentCancelledEvent

---

### 5. Notification Service (Port 5005)
**Responsibility:** Email, SMS, push notifications

**Databases:** PostgreSQL + MySQL + MongoDB (ehr_notification_*)

**Consumes:** UserCreatedEvent, PatientCreatedEvent, AppointmentScheduledEvent, etc.

**Events Published:** EmailNotificationSentEvent, SmsNotificationSentEvent, NotificationFailedEvent

---

### 6. Audit Service (Port 5006)
**Responsibility:** Compliance logging, data access tracking (HIPAA)

**Databases:** PostgreSQL (ehr_audit_db) + MongoDB (ehr_audit_documents)

**Consumes:** **ALL EVENTS** (complete audit trail)

**Events Published:** DataAccessLoggedEvent, DataModificationLoggedEvent, SecurityIncidentLoggedEvent

---

### 7. Billing Service (Port 5007)
**Responsibility:** Invoicing, payments, claims processing

**Databases:** PostgreSQL + MySQL (ehr_billing_db)

**Consumes:** AppointmentCompletedEvent, PatientCreatedEvent

**Events Published:** InvoiceGeneratedEvent, PaymentReceivedEvent, InvoiceOverdueEvent

---

### 8. Prescription Service (Port 5008)
**Responsibility:** Medication management, refill requests, pharmacy coordination

**Databases:** PostgreSQL (ehr_prescription_db) + MongoDB (ehr_prescription_documents)

**Consumes:** ClinicalNoteCreatedEvent, PatientCreatedEvent

**Events Published:** PrescriptionCreatedEvent, PrescriptionFilledEvent, PrescriptionRefillRequestedEvent

---

### 9. Outbox Processor (Port 5009)
**Responsibility:** Transactional outbox pattern (ensures reliable event publishing)

**Databases:** All 3 databases (ehr_outbox_db)

**Pattern:** Guarantees no lost events between service commit and Kafka publish

---

### 10. Analytics Service (Port 5010)
**Responsibility:** Reporting, metrics, dashboards

**Databases:** PostgreSQL + MySQL (ehr_analytics_db)

**Consumes:** PatientCreatedEvent, AppointmentCompletedEvent, InvoiceGeneratedEvent

**Events Published:** ReportGeneratedEvent, MetricsAggregatedEvent, AnalyticsAlertGeneratedEvent

---

## 📊 Database Schemas

### Migrations

Each service owns its database migrations:

```bash
# Create migration for Patient Service
dotnet ef migrations add AddPatientContacts \
  --project src/EHRPlatform.Services.Patient \
  --context PatientContext

# Apply migrations
dotnet ef database update \
  --project src/EHRPlatform.Services.Patient \
  --context PatientContext
```

See [db/](./db/) directory for all service migrations.

---

## 🔐 Security & Compliance

### HIPAA Audit Trail
- **All events logged** to ehr_audit_db via Audit Service
- **Data access logged** for every patient record retrieval
- **Immutable audit logs** prevent tampering
- **7-year retention** for compliance

### Authentication & Authorization
- JWT tokens issued by Identity Service
- Role-Based Access Control (RBAC)
- Roles: Admin, Doctor, Nurse, Patient
- Fine-grained permissions enforcement

### Data Protection
- Encryption at rest: All database fields
- Encryption in transit: TLS 1.2+
- Password hashing: bcrypt
- PII masking in logs

---

## 📡 Event-Driven Architecture (Kafka/MassTransit)

### Publisher Pattern

Services publish events to Kafka when actions occur:

```csharp
// Patient Service publishes when patient created
public class CreatePatientHandler : IRequestHandler<CreatePatientCommand>
{
    private readonly IPublishEndpoint _publishEndpoint;
    
    public async Task Handle(CreatePatientCommand request, CancellationToken cancellationToken)
    {
        var patient = new Patient { ... };
        await _context.SaveChangesAsync();
        
        // Publish event
        await _publishEndpoint.Publish(new PatientCreatedEvent 
        { 
            PatientId = patient.Id, 
            MRN = patient.MRN,
            // ...
        });
    }
}
```

### Consumer Pattern

Services subscribe to events from Kafka:

```csharp
// Notification Service consumes UserCreatedEvent
public class UserCreatedConsumer : IConsumer<UserCreatedEvent>
{
    public async Task Consume(ConsumeContext<UserCreatedEvent> context)
    {
        var @event = context.Message;
        
        // Send welcome email
        await _emailService.SendWelcomeEmailAsync(@event.Email);
        
        // Publish notification sent event
        await context.Publish(new EmailNotificationSentEvent { ... });
    }
}
```

### Resilience Features
- ✅ **Retry Policy:** Exponential backoff (1s, 2s, 4s)
- ✅ **Circuit Breaker:** Prevent cascading failures
- ✅ **Dead Letter Queue:** Capture unprocessable messages
- ✅ **Correlation IDs:** Trace events across services

---

## 📈 Monitoring & Observability

### Prometheus Metrics
- Request latency (95th percentile)
- Error rates (5xx, timeouts, circuit breaker trips)
- Kafka consumer lag
- Database connection pool usage

### Grafana Dashboards
Pre-built dashboards available in `monitoring/grafana-provisioning/dashboards/`:
- System overview
- Per-service metrics
- Infrastructure health (Kafka, databases)

### Centralized Logging
- Structured JSON logging
- Correlation IDs for tracing
- Log aggregation via ELK Stack

---

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run specific service tests
dotnet test src/EHRPlatform.Services.Patient.Tests

# With code coverage
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover
```

---

## 📚 Documentation

| Document | Purpose |
|----------|---------|
| [ARCHITECTURE.md](./ARCHITECTURE.md) | Service architecture & communication patterns |
| [DEPLOYMENT.md](./DEPLOYMENT.md) | Step-by-step deployment guide (dev → production) |
| [db/MICROSERVICES_ISOLATION_PHASE*.md](./db/) | Refactoring documentation (6 phases) |
| [Swagger/OpenAPI](http://localhost:5000/swagger) | Live API documentation |

---

## 🚀 Deployment

### Development (Docker Compose)

```bash
docker-compose up -d
# All services running in 60 seconds
```

### Production (Kubernetes)

```bash
helm install ehr-platform ./k8s/ehr-platform \
  --namespace ehr \
  --values k8s/ehr-platform/values-prod.yaml
```

See [DEPLOYMENT.md](./DEPLOYMENT.md) for detailed instructions.

---

## 📊 Key Metrics

| Metric | Target |
|--------|--------|
| API Latency (p95) | < 200ms |
| Error Rate | < 0.1% |
| Kafka Consumer Lag | < 1000 messages |
| Database Connection Pool | 80% utilization |
| Service Uptime | > 99.9% |

---

## 🔍 Troubleshooting

### Services won't start
```bash
# Check logs
docker-compose logs identity-service

# Restart all
docker-compose restart
```

### Database connection errors
```bash
# Verify databases are created
docker exec ehr-postgres psql -U ehr_user -c "\l" | grep ehr_

# Wait for initialization (60 seconds)
```

### Kafka topics not created
```bash
# Manually create topics
docker exec ehr-kafka kafka-topics --create \
  --bootstrap-server localhost:9092 \
  --topic patient-events --partitions 3 --replication-factor 1
```

See [DEPLOYMENT.md](./DEPLOYMENT.md#troubleshooting) for more issues.

---

## 📞 References

- **Architecture Decision Record (ADR):** See Phase 8 documentation
- **Database Migrations:** `db/migrations/` (per-service)
- **Event Schemas:** `src/EHRPlatform.Common/Shared/DTOs/` (34+ events)
- **Consumer Examples:** `src/EHRPlatform.Services.*/Consumers/`
- **Configuration:** `k8s/ehr-platform/values*.yaml`

---

## ✅ Verification Checklist

- [ ] All 10 services running (`docker-compose ps`)
- [ ] All databases initialized
- [ ] Kafka topics created
- [ ] Health checks passing
- [ ] API endpoints responding
- [ ] Events flowing through Kafka
- [ ] Audit logs being written
- [ ] Metrics visible in Prometheus
- [ ] Dashboards displaying in Grafana
- [ ] Backups configured
