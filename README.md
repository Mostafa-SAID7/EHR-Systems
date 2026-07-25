<div align="center">

# 🏥 Modern EHR Platform

**Enterprise-grade Electronic Health Records system built for the modern healthcare era.**

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](./LICENSE)
[![Angular](https://img.shields.io/badge/Angular-18-red.svg)](https://angular.io)
[![.NET](https://img.shields.io/badge/.NET-8-purple.svg)](https://dotnet.microsoft.com)
[![HIPAA](https://img.shields.io/badge/HIPAA-Compliant-green.svg)](./docs/SECURITY.md)
[![Docker](https://img.shields.io/badge/Docker-Ready-blue.svg)](./devops/)

[📖 Docs](#-documentation) · [🚀 Quick Start](#-quick-start) · [🏗 Architecture](#-architecture) · [🔐 Security](#-security--compliance) · [🤝 Contributing](./docs/CONTRIBUTING.md)

</div>

---

## ✨ Overview

Modern EHR Platform is a **production-ready, HIPAA-compliant** Electronic Health Records system featuring a clean microservices backend and a responsive Angular frontend. It covers the full clinical workflow — from patient registration and scheduling through medical coding, billing, and analytics.

**Key capabilities:**

| Area | Details |
|:--- |:--- |
| 🧑‍⚕️ Patient Management | Demographics, medical history, timelines |
| 📅 Appointments | Scheduling, reminders, calendar integration |
| 📋 Medical Records | SOAP notes, vitals, ICD-10 diagnoses, CPT procedures |
| 💊 Prescriptions | eRx, medication history, interaction checking |
| 🧪 Labs & Imaging | Results, trends, PDF viewer |
| 💰 Billing & Insurance | Claims submission, payment tracking, insurance verification |
| 📊 Analytics | Population health, compliance KPIs, dashboards |
| 🌍 i18n & RTL | English + Arabic, full RTL layout support |

---

## 🏗 Architecture

```
modern-ehr-platform/
├── frontend/          # Angular 18 SPA (standalone components, Signals)
├── backend/           # ASP.NET Core 8 Microservices (Clean Architecture)
├── devops/            # Docker, Kubernetes, Terraform, GitHub Actions
└── docs/              # All project documentation (see below)
```

**Full architecture detail** → [`docs/Backend/Architectures/`](./docs/Backend/Architectures/)

### Microservices

| Service | Responsibility |
|:--- |:--- |
| `PatientService` | Patient records, demographics |
| `AppointmentService` | Scheduling engine |
| `MedicalRecordService` | Clinical notes, diagnoses |
| `PrescriptionService` | Medication & eRx |
| `BillingService` | Claims & payments |
| `AuthService` | Identity, JWT, RBAC |

---

## 🚀 Quick Start

### Prerequisites

- **Node.js** 18+ & npm 9+
- **Docker** & Docker Compose
- **.NET 8 SDK**

### Run Locally

```bash
# 1. Clone
git clone https://github.com/Mostafa-SAID7/EHR-Systems.git
cd EHR-Systems

# 2. Start everything (recommended)
docker-compose up -d
# Frontend → http://localhost:4200
# API      → http://localhost:5000
# Swagger  → http://localhost:5000/swagger

# --- OR run individually ---

# Frontend only
cd frontend && npm install && npm start

# Backend only
cd backend && dotnet restore && dotnet run --project src/EHRPlatform.Api
```

### Run Tests

```bash
# Angular unit tests
cd frontend && npm test

# Angular E2E tests
cd frontend && npm run e2e

# .NET backend tests
cd backend && dotnet test
```

---

## 📦 Tech Stack

### Frontend
| | |
|:--- |:--- |
| Framework | Angular 18 (standalone components, Signals) |
| State | NgRx Signals |
| Styling | Tailwind CSS |
| Testing | Jasmine · Karma · Cypress |
| Build | Vite |

### Backend
| | |
|:--- |:--- |
| Framework | ASP.NET Core 8 |
| Architecture | Clean Architecture · CQRS (MediatR) · Event-Driven |
| Database | SQL Server / PostgreSQL · EF Core · Dapper |
| Messaging | Apache Kafka |
| Auth | JWT + OAuth2 (OpenID Connect) |
| Realtime | SignalR |
| Logging | Serilog + OpenTelemetry |
| Testing | xUnit · Moq · Testcontainers |

### DevOps
| | |
|:--- |:--- |
| Containers | Docker · Kubernetes |
| IaC | Terraform (Azure / AWS) |
| CI/CD | GitHub Actions |
| Monitoring | Prometheus · Grafana · Jaeger |
| Secrets | Azure Key Vault / AWS Secrets Manager |

---

## 🔐 Security & Compliance

| Requirement | Status |
|:--- |:--- |
| HIPAA Compliance | ✅ |
| HITECH Act | ✅ |
| JWT Authentication | ✅ |
| Role-Based Access Control | ✅ |
| PHI Encryption (AES-256 at-rest, TLS in-transit) | ✅ |
| Immutable Audit Logging | ✅ |
| OWASP Top 10 Mitigations | ✅ |
| Dependency Vulnerability Scanning | ✅ |
| WCAG AA Accessibility | ✅ |

→ Details in [`docs/SECURITY.md`](./docs/SECURITY.md) and [`docs/Security/`](./docs/Security/)

---

## 📖 Documentation

### Core Docs (root `docs/`)

| Document | Purpose |
|:--- |:--- |
| [`docs/SECURITY.md`](./docs/SECURITY.md) | HIPAA compliance checklist & encryption standards |
| [`docs/CONTRIBUTING.md`](./docs/CONTRIBUTING.md) | Git workflow, coding standards, PR process |

### Backend Docs

| Folder | Covers |
|:--- |:--- |
| [`docs/Backend/Architectures/`](./docs/Backend/Architectures/) | Clean Architecture, Microservices patterns |
| [`docs/Backend/API-Design/`](./docs/Backend/API-Design/) | REST conventions, versioning, error standards |
| [`docs/Backend/ASP.NET-Core/`](./docs/Backend/ASP.NET-Core/) | DI, middleware, configuration |
| [`docs/Backend/C#/`](./docs/Backend/C/) | Async patterns, records, generics |
| [`docs/Backend/Caching/`](./docs/Backend/Caching/) | Redis & distributed caching |
| [`docs/Backend/Monitoring/`](./docs/Backend/Monitoring/) | OpenTelemetry, Serilog, health checks |
| [`docs/Backend/Performance/`](./docs/Backend/Performance/) | DB optimization, async/memory tuning |

### Frontend Docs

| Folder | Covers |
|:--- |:--- |
| [`docs/Angular/`](./docs/Angular/) | Components, DI, routing, HTTP Client, security |
| [`docs/Angular/RxJS/`](./docs/Angular/RxJS/) | Observables, operators (switchMap, mergeMap…) |

### Architecture & Patterns

| Folder | Covers |
|:--- |:--- |
| [`docs/SystemDesign/`](./docs/SystemDesign/) | CAP theorem, Event Sourcing, Saga, scalability, Kubernetes |
| [`docs/DesignPatterns/`](./docs/DesignPatterns/) | Creational, Structural, Behavioral, CQRS, Specification |

### Cross-Cutting

| Folder | Covers |
|:--- |:--- |
| [`docs/Database/`](./docs/Database/) | EF Core, Dapper, query optimization, migrations |
| [`docs/DevOps/`](./docs/DevOps/) | Docker, Kubernetes, CI/CD, Terraform |
| [`docs/Testing/`](./docs/Testing/) | Unit, integration, E2E testing patterns |
| [`docs/Security/`](./docs/Security/) | AuthN/AuthZ, OWASP, HIPAA, rate limiting |

---

## 🔄 CI/CD Pipeline

```
Push / PR
  ├─→ Lint & Type Check (Angular)
  ├─→ Unit Tests (frontend + backend)
  ├─→ Integration Tests
  ├─→ Security Scan (SAST + dependency audit)
  ├─→ Docker Build & Push
  └─→ Deploy to Staging  ──(manual approval)──▶  Deploy to Production
```

Workflow files → [`.github/workflows/`](./.github/workflows/)

---

## 🗺 Roadmap

| Phase | Status | Highlights |
|:--- |:--- |:--- |
| Phase 1 | ✅ Complete | Patient management, appointments, medical records, prescriptions, basic billing |
| Phase 2 (Q4 2026) | 🔄 Planned | HL7/FHIR interoperability, advanced analytics, telemedicine, mobile app |
| Phase 3 (2027) | 🔮 Future | AI-assisted diagnostics, predictive analytics, integration marketplace |

---

## 🤝 Contributing

Contributions are welcome! Please read [`docs/CONTRIBUTING.md`](./docs/CONTRIBUTING.md) before opening a PR.

---

## 📄 License

MIT — see [`LICENSE`](./LICENSE) for details.

---

<div align="center">
Built with ❤️ for healthcare &nbsp;·&nbsp; Last Updated: July 2026
</div>
