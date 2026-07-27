# Quick Start: Running EHR Platform with Migrations

## Prerequisites

- Docker & Docker Compose
- .NET 8 SDK (for local development without Docker)
- PostgreSQL CLI (optional, for manual verification)

---

## Option 1: Run with Docker Compose (Recommended for Development)

### Start All Services

```bash
cd backend
docker-compose up -d
```

This will:
1. ✅ Start PostgreSQL, Redis, Elasticsearch, Kafka, Prometheus, Grafana
2. ✅ Build and start all 11 microservices
3. ✅ **Automatically run migrations** (Development environment)
4. ✅ Initialize database schema

### Monitor Migrations

Check logs for a specific service:

```bash
docker-compose logs -f patient-service
```

Look for output like:
```
patient-service | [12:34:56 INF] Applying migration: 20250101_001_baseline
patient-service | [12:34:58 INF] Migration applied: 20250101_001_baseline
patient-service | [12:34:58 INF] Patient database schema verified/created
```

### Verify Migrations Ran

```bash
# Connect to PostgreSQL
docker exec -it ehr-postgres psql -U ehr_user -d ehr_platform -c \
  "SELECT * FROM \"__MigrationHistory\" ORDER BY \"AppliedAt\" DESC;"
```

Expected output:
```
 MigrationId          | ProductVersion | AppliedAt
----------------------+----------------+---------------------------
 20250101_001_baseline | 8.0.0          | 2025-01-15 12:34:58.123
```

### Stop All Services

```bash
docker-compose down
```

---

## Option 2: Run Locally Without Docker

### Prerequisites

- PostgreSQL running locally (default: localhost:5432)
- Redis running locally (optional, default: localhost:6379)
- Elasticsearch running locally (optional, default: localhost:9200)

### Start Patient Service Locally

```bash
cd backend/src/EHRPlatform.Services.Patient
dotnet run --configuration Development
```

Expected output:
```
[12:34:56 INF] Applying migration: 20250101_001_baseline
[12:34:58 INF] Migration applied: 20250101_001_baseline
[12:34:58 INF] Patient database schema verified/created
[12:34:59 INF] Swagger UI: http://localhost:5002/swagger
```

### Start Other Services

```bash
# In separate terminals:
cd backend/src/EHRPlatform.Services.Billing && dotnet run
cd backend/src/EHRPlatform.Services.Identity && dotnet run
cd backend/src/EHRPlatform.Services.Clinical && dotnet run
# ... etc
```

---

## Service Endpoints

| Service | Port | Health | Swagger |
|---------|------|--------|---------|
| API Gateway | 5000 | http://localhost:5000/health | http://localhost:5000/swagger |
| Identity | 5001 | http://localhost:5001/health | http://localhost:5001/swagger |
| Patient | 5002 | http://localhost:5002/health | http://localhost:5002/swagger |
| Clinical | 5003 | http://localhost:5003/health | http://localhost:5003/swagger |
| Appointment | 5004 | http://localhost:5004/health | http://localhost:5004/swagger |
| Notification | 5005 | http://localhost:5005/health | http://localhost:5005/swagger |
| Audit | 5006 | http://localhost:5006/health | http://localhost:5006/health |
| Billing | 5007 | http://localhost:5007/health | http://localhost:5007/swagger |
| Prescription | 5008 | http://localhost:5008/health | http://localhost:5008/swagger |
| Analytics | 5009 | http://localhost:5009/health | http://localhost:5009/swagger |

---

## Migration Details

### Development Environment (Default)

```
Policy: AutomaticOnStartup
Behavior: All pending migrations run automatically on app startup
Backup: None
Location: ASPNETCORE_ENVIRONMENT=Development (docker-compose.yml)
```

### Staging Environment

```
Policy: ManualOnly
Behavior: Migrations must be applied manually via script
Backup: Full database backup before applying
Command: ./backend/scripts/db-migrate.sh up --env=staging
Location: ASPNETCORE_ENVIRONMENT=Staging
```

### Production Environment

```
Policy: Disabled
Behavior: No automatic migrations (DBA-controlled)
Backup: Full database backup + archiving
Command: ./backend/scripts/db-migrate.sh up --env=production (DBA only)
Location: ASPNETCORE_ENVIRONMENT=Production
```

---

## Troubleshooting

### Migrations Failed to Run

**Error:** `Migration failed for PatientService`

**Solution:**
1. Check database connection: `docker logs ehr-postgres`
2. Verify `__MigrationHistory` table exists: 
   ```bash
   docker exec -it ehr-postgres psql -U ehr_user -d ehr_platform -c "\dt"
   ```
3. Check service logs: `docker-compose logs patient-service`

### Port Already in Use

**Error:** `bind: address already in use`

**Solution:**
```bash
# Stop conflicting service
docker-compose down

# Or use custom ports
docker-compose -f docker-compose.override.yml up -d
```

### Database Connection Timeout

**Error:** `Failed to connect to database`

**Solution:**
1. Ensure PostgreSQL container is running: `docker ps | grep postgres`
2. Check PostgreSQL is healthy: `docker-compose logs postgres`
3. Verify connection string in environment

---

## Creating New Migrations

### 1. Create Migration File

```bash
cp backend/db/migrations/00_MIGRATION_TEMPLATE.sql \
   backend/db/migrations/20250120_001_add-patient-tags.sql
```

### 2. Edit Migration

Add your SQL changes to the file:

```sql
-- ═══════════════════════════════════════════════════════════════════════════════
-- EHR Platform Database - Add Patient Tags
-- Version: 20250120_001
-- Created: 2025-01-20
-- Description: Add support for patient tagging system
-- ═══════════════════════════════════════════════════════════════════════════════

CREATE TABLE IF NOT EXISTS "PatientTags" (
    "Id" uuid NOT NULL PRIMARY KEY,
    "PatientId" uuid NOT NULL,
    "TagName" character varying(100) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "FK_PatientTags_Patients" FOREIGN KEY ("PatientId")
        REFERENCES "Patients" ("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_PatientTags_PatientId" ON "PatientTags" ("PatientId");

INSERT INTO "__MigrationHistory" ("MigrationId", "ProductVersion")
VALUES ('20250120_001_add-patient-tags', '8.0.0')
ON CONFLICT DO NOTHING;

COMMIT;
```

### 3. Test Locally

```bash
docker exec -it ehr-postgres psql -U ehr_user -d ehr_platform < \
  backend/db/migrations/20250120_001_add-patient-tags.sql
```

### 4. Commit

```bash
git add backend/db/migrations/20250120_001_add-patient-tags.sql
git commit -m "db(migration): Add patient tags support (20250120_001)"
git push origin main
```

---

## Next Steps

1. **Pull latest changes:** `git pull origin main`
2. **Run services:** `docker-compose up -d` (or `dotnet run` locally)
3. **Verify migrations:** Check logs or query `__MigrationHistory` table
4. **Test endpoints:** Visit http://localhost:5000/health
5. **Deploy to staging:** CI/CD pipeline triggers automatically on merge
6. **Deploy to production:** Manual approval required

---

## Support

- **Migration Issues:** See `backend/db/migrations/ROLLBACK_STRATEGIES.md`
- **Integration Guide:** See `backend/db/INTEGRATION_GUIDE.md`
- **Version Tracking:** See `backend/db/migrations/MIGRATION_VERSION.md`

---

**Last Updated:** 2025-01-15  
**Status:** Production-Ready
