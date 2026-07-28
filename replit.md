# EHR Platform — Replit Project

## Overview
Full-stack Electronic Health Records platform built with **Angular 18** (frontend) and **9 .NET 8 microservices** (backend). The API Gateway on port 5000 is the single entry point; the Angular dev server runs on port 4200.

## Architecture

### Services & Ports
| Workflow | Port | Description |
|---|---|---|
| API Gateway | 5000 | YARP reverse proxy — entry point for all API calls |
| Identity Service | 5001 | Auth, JWT, roles, permissions |
| Patient Service | 5002 | Patient demographics, search |
| Clinical Service | 5003 | Clinical notes, vitals, diagnoses |
| Appointment Service | 5004 | Scheduling |
| Audit Service | 5006 | HIPAA audit log |
| Billing Service | 5007 | Invoices, claims |
| Prescription Service | 5008 | Drug orders |
| Analytics Service | 5009 | Dashboards, reports |
| Frontend | 4200 | Angular 18 dev server |

### Database per Service
| Service | Primary DB | ORM/Access | Notes |
|---|---|---|---|
| Identity | PostgreSQL | EF Core | Users, roles, tokens |
| Patient | PostgreSQL | EF Core | Demographics |
| Clinical | PostgreSQL + MongoDB (optional) | EF Core + `IMongoRepository` | MongoDB for SOAP note documents |
| Appointment | PostgreSQL | EF Core | Relational scheduling |
| Billing | PostgreSQL | EF Core + Dapper | Dapper for invoice aging & revenue reports |
| Prescription | PostgreSQL | EF Core | Drug/dosage structured data |
| Audit | PostgreSQL | EF Core + Dapper | Dapper for bulk HIPAA compliance queries |
| Analytics | PostgreSQL | EF Core + Dapper | Dapper for metric aggregations |
| Notification | PostgreSQL | EF Core | Notification history |
| OutboxProcessor | PostgreSQL | EF Core | Transactional outbox pattern |

**MySQL** was removed entirely — it existed only as dead code with no service consuming it.

## Running the Project

### Prerequisites
All services require a PostgreSQL database. The connection is read from Replit-managed env vars (`PGHOST`, `PGPORT`, `PGDATABASE`, `PGUSER`, `PGPASSWORD`). Add a Replit PostgreSQL database to provision these automatically.

Required secrets:
- `JWT_SECRET` — minimum 32 characters; used by all services for token validation
- `ENCRYPTION_KEY` — used by Identity Service for sensitive field encryption

Optional:
- `REDIS_URL` — Redis caching (services degrade gracefully without it)
- `ELASTICSEARCH_URL` — full-text search (services degrade gracefully without it)
- `MONGODB_URI` — Clinical service document store (degrades gracefully without it)

### Running
Start all workflows from the Replit workflow panel. Each service auto-discovers `PGHOST` and skips migrations in Development when the strategy is set to Disabled (schema must be pre-migrated or created manually).

### EF Core Migrations
Migrations are pre-generated under `Data/Migrations/` in each service. To run a new migration:

```bash
export DOTNET_ROOT=/nix/store/1blv644vinali34masnw6g5fjjjaa4y6-dotnet-sdk-8.0.416/share/dotnet
export PATH="$PATH:$HOME/.dotnet/tools"
cd backend
dotnet-ef migrations add <MigrationName> \
  --project src/EHRPlatform.Services.<ServiceName> \
  --context <ContextName> \
  --output-dir Data/Migrations
```

The design-time factory in each service (`Data/Design/DesignTimeContextFactory.cs`) reads `DESIGN_TIME_CONNECTION_STRING` or falls back to a localhost stub.

## User Preferences
- Keep the polyglot storage strategy (PostgreSQL OLTP, optional MongoDB for documents, optional Redis cache)
- Services must start cleanly without Kafka, RabbitMQ, MongoDB, or Redis configured — treat those as optional
- Use Dapper for bulk read / reporting queries in Audit and Billing; keep EF Core for all writes
