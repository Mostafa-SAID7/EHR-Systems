# Modern EHR Platform

Enterprise-grade Electronic Health Records system built on Angular 18 + ASP.NET Core 8 microservices.

## Architecture

```
frontend/      Angular 18 SPA (standalone components, Signals, Tailwind CSS)
backend/       ASP.NET Core 8 microservices (Clean Architecture)
devops/        Docker, Kubernetes, Terraform, GitHub Actions configs
docs/          Project documentation
```

### Microservices

| Service | Port | Responsibility |
|---|---|---|
| Identity | 5001 | Auth, JWT, RBAC |
| API Gateway | 5000 | YARP reverse proxy, routing |
| Patient | 5002 | Patient records, demographics |
| Clinical | 5003 | Clinical notes, diagnoses |
| Appointment | 5004 | Scheduling engine |
| Audit | 5006 | Audit logging |
| Billing | 5007 | Claims & payments |
| Prescription | 5008 | Medications & eRx |
| Analytics | 5009 | Population health, KPIs |

Frontend dev server runs on **port 4200**.

## How to Run

All workflows are pre-configured. Click the **Run** button to start everything in parallel.

- **Frontend** — Angular dev server: `cd frontend && ng serve --host 0.0.0.0 --port 4200`
- **Each backend service** — `cd backend && dotnet run --project src/EHRPlatform.Services.<Name>`

## Database

Replit's built-in PostgreSQL is used automatically via `PGHOST`/`PGPORT`/`PGUSER`/`PGPASSWORD`/`PGDATABASE` environment variables. The `BuildPostgresConnectionString()` extension in `EHRPlatform.Common` reads these at startup.

EF Core migrations are set to **Disabled** by default — the services verify connectivity but do not auto-migrate. To apply migrations, run `dotnet ef database update` per service.

## Optional Services (not running)

- **Redis** — caching (services degrade gracefully without it)
- **MongoDB** — document store for Patient/Clinical notes (optional; services log a warning and continue)
- **RabbitMQ / Kafka** — messaging (falls back to in-memory bus automatically)
- **Elasticsearch** — search indexing (optional)
- **Notification Service** (port 5005) — not configured as a workflow yet

## Stack

- **Frontend**: Angular 18, Tailwind CSS, TypeScript
- **Backend**: ASP.NET Core 8, Entity Framework Core, MediatR (CQRS), MassTransit, YARP, SignalR
- **Database**: PostgreSQL (Replit managed)
- **Auth**: JWT Bearer tokens
- **Language**: .NET 8.0 SDK (module: `dotnet-8.0`)

## User Preferences

_None recorded yet._
