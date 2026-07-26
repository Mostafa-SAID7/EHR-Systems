# Modern EHR Platform - Backend Services

Production-grade ASP.NET Core microservices architecture for healthcare data management.

---

## 🏗️ Architecture

```
API Gateway (Kong/Nginx)
    │
    ├─→ Auth Service        (Identity, JWT, RBAC)
    ├─→ Patient Service     (Demographics, medical history)
    ├─→ Appointment Service (Scheduling, calendar)
    ├─→ Medical Record Service (SOAP notes, vitals, diagnostics)
    ├─→ Prescription Service (eRx, medication interactions)
    ├─→ Billing Service     (Claims, payments, insurance)
    └─→ Notification Service (Email, SMS, push notifications)

Shared Resources:
├─→ SQL Server Database (EHR Platform DB)
├─→ Redis Cache (Sessions, permissions, search indexes)
├─→ Message Queue (RabbitMQ/Azure Service Bus)
├─→ File Storage (Azure Blob Storage)
└─→ Search Engine (Elasticsearch for patient search)
```

---


```
backend/
├── src/
│   ├── EHRPlatform.ApiGateway/              # API Gateway
│   │   ├── Controllers/
│   │   ├── Middleware/
│   │   ├── Program.cs
│   │   └── appsettings.json
│   │
│   ├── EHRPlatform.Domain/                  # Shared domain models
│   │   ├── Entities/
│   │   │   ├── Patient.cs
│   │   │   ├── Appointment.cs
│   │   │   └── ...
│   │   ├── Interfaces/
│   │   ├── ValueObjects/
│   │   └── EHRPlatform.Domain.csproj
│   │
│   ├── EHRPlatform.Infrastructure/          # Data access & external services
│   │   ├── Data/
│   │   │   ├── Context/
│   │   │   ├── Migrations/
│   │   │   └── Repositories/
│   │   ├── Services/
│   │   ├── Cache/
│   │   └── EHRPlatform.Infrastructure.csproj
│   │
│   ├── EHRPlatform.Services/                # Business logic microservices
│   │   ├── AuthService/
│   │   │   ├── Controllers/
│   │   │   ├── Services/
│   │   │   ├── Models/
│   │   │   └── Program.cs
│   │   │
│   │   ├── PatientService/
│   │   │   ├── Controllers/
│   │   │   ├── Services/
│   │   │   ├── Models/
│   │   │   └── Program.cs
│   │   │
│   │   ├── AppointmentService/
│   │   ├── MedicalRecordService/
│   │   ├── PrescriptionService/
│   │   ├── BillingService/
│   │   └── NotificationService/
│   │
│   ├── EHRPlatform.Tests/                   # Unit & integration tests
│   │   ├── AuthServiceTests/
│   │   ├── PatientServiceTests/
│   │   ├── Common/
│   │   └── Integration/
│   │
│   └── EHRPlatform.sln
│
├── docs/
│   ├── MICROSERVICES.md                     # Service overview
│   ├── DATABASE.md                          # EF Core migrations
│   └── API_DOCS.md                          # OpenAPI/Swagger
│
├── .github/
│   └── workflows/                           # CI/CD (see root .github/)
│
├── Dockerfile                               # Multi-stage Docker build
├── docker-compose.override.yml              # Development environment
└── package.json                             # NPM scripts for backend tasks
```

---

## 🚀 Quick Start

### Prerequisites

- **.NET 8 SDK** installed
- **SQL Server** (local or Docker)
- **Redis** (optional, for caching)
- **Visual Studio 2022** or VS Code

### Setup

```bash
# 1. Navigate to backend
cd backend

# 2. Install dependencies
dotnet restore

# 3. Update database
dotnet ef database update --project src/EHRPlatform.Infrastructure

# 4. Run migrations
dotnet ef migrations add InitialCreate
dotnet ef database update

# 5. Start services
dotnet run --project src/EHRPlatform.ApiGateway

# API Gateway runs on: http://localhost:5000
# Swagger UI: http://localhost:5000/swagger
```

---

## 🏢 Microservices Overview

### 1. Auth Service

**Responsibility**: User authentication, authorization, RBAC

**Endpoints**:
```
POST   /auth/login
POST   /auth/refresh
POST   /auth/logout
GET    /auth/profile
POST   /auth/change-password
```

**Key Features**:
- JWT token generation & validation
- Refresh token management
- Role-based access control (RBAC)
- Permission caching
- Multi-factor authentication (MFA)

---

### 2. Patient Service

**Responsibility**: Patient demographics, medical history, search

**Endpoints**:
```
GET    /patients
GET    /patients/{id}
POST   /patients
PUT    /patients/{id}
DELETE /patients/{id}
GET    /patients/{id}/history
POST   /patients/{id}/allergies
POST   /patients/{id}/conditions
```

**Key Features**:
- Patient search (name, MRN, DOB)
- Medical history timeline
- Allergy & condition management
- CSV export
- Audit logging

---


**Responsibility**: Claims, payments, insurance verification

**Endpoints**:
```
GET    /claims
POST   /claims
GET    /claims/{id}
GET    /invoices
POST   /invoices/{id}/payment
```

**Key Features**:
- Claim generation & submission
- Payment processing
- Insurance verification
- Financial reporting
- Revenue cycle management

---

## 📊 Database

### Entity Framework Core

**Setup**:
```bash
# Create migration
dotnet ef migrations add MigrationName --project src/EHRPlatform.Infrastructure

# Apply migration
dotnet ef database update --project src/EHRPlatform.Infrastructure

# Revert migration
dotnet ef database update PreviousMigration --project src/EHRPlatform.Infrastructure
```

### Connection String

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ehr_platform;User Id=sa;Password=YourPassword;TrustServerCertificate=true;"
  }
}
```

---

## 🔐 Security

### JWT Authentication

All endpoints require Bearer token:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

### RBAC Guards

```csharp
[Authorize(Roles = "Doctor,Admin")]
[HttpPost("patients")]
public async Task<IActionResult> CreatePatient(CreatePatientRequest request)
{
    // Only Doctors and Admins can create patients
}
```

### Data Encryption

- PII encrypted at rest (SSN, DOB, MRN)
- HTTPS/TLS 1.3 for transit
- Secure key management via Key Vault

---

## 📝 API Documentation

**Swagger UI**: http://localhost:5000/swagger

**OpenAPI Spec**: http://localhost:5000/swagger/v1/swagger.json

---

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test src/EHRPlatform.Tests/AuthServiceTests/AuthServiceTests.csproj

# With coverage
dotnet test /p:CollectCoverage=true
```

---

## 📈 Monitoring & Logging

### Serilog

Logs configured in `appsettings.json`:

```json
{
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      { "Name": "Console" },
      { "Name": "File", "Args": { "path": "logs/app-.txt" } }
    ]
  }
}
```

---

## 🚀 Deployment

### Docker

```bash
# Build image
docker build -f Dockerfile -t ehr-backend:latest .

# Run container
docker run -p 5000:80 \
  -e ConnectionStrings__DefaultConnection="Server=db;..." \
  ehr-backend:latest
```

### Kubernetes

See [devops/kubernetes/](../devops/kubernetes/) for K8s manifests.

---

## 📞 Support & Documentation

- **Detailed Service Docs**: [docs/MICROSERVICES.md](./docs/MICROSERVICES.md)
- **Database Schema**: [docs/DATABASE.md](./docs/DATABASE.md)
- **API Reference**: [docs/API_DOCS.md](./docs/API_DOCS.md)
- **Root Docs**: [../docs/](../docs/)

---

**Version**: 1.0.0 | Last Updated: July 2026
