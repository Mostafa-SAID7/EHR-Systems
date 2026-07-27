# EHR Platform - Migration Integration Guide

## Overview

This guide shows how to integrate database migrations into microservices using the migration utilities and strategies.

---

## Quick Start (5 minutes)

### 1. Update Program.cs in Your Service

Add migration registration to your service's `Program.cs`:

```csharp
using EHRPlatform.Common.Data.Migrations;

var builder = WebApplication.CreateBuilder(args);

// ... other configurations ...

// Add DbContext
builder.Services.AddDbContext<YourServiceContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register migration strategy based on environment
var environment = builder.Environment.EnvironmentName;
new MigrationConfiguration(builder.Services)
    .WithEnvironment(environment)
    .AddContext<YourServiceContext>()
    .Build();

var app = builder.Build();

// Apply migrations before running app
await app.Services.RunMigrationsAsync<YourServiceContext>("YourServiceName");

// ... rest of app setup ...

app.Run();
```

### 2. Verify in appsettings.json

Ensure connection string is configured:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=ehr_platform_dev;Username=ehr_user;Password=password"
  }
}
```

### 3. Run Service

```bash
dotnet run
```

Migrations apply automatically based on environment setting.

---

## Environment-Specific Configurations

### Development (Auto-Migrate)

**File:** `appsettings.Development.json`

```json
{
  "ASPNETCORE_ENVIRONMENT": "development",
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=ehr_platform_dev;Username=ehr_user;Password=password"
  }
}
```

**Behavior:**
- ✅ Auto-runs migrations on startup
- ✅ No backup required
- ✅ Rapid iteration
- ❌ No production data protection

### Staging (Manual)

**File:** `appsettings.Staging.json`

```json
{
  "ASPNETCORE_ENVIRONMENT": "staging",
  "ConnectionStrings": {
    "DefaultConnection": "Host=staging-db.local;Port=5432;Database=ehr_platform_staging;Username=ehr_user;Password=${DB_PASSWORD}"
  }
}
```

**Behavior:**
- ✅ Requires manual migration approval
- ✅ Full backup before applying
- ✅ Production-like environment
- ❌ Cannot auto-migrate

**Manual migration:**

```bash
./backend/scripts/db-migrate.sh up --env=staging
```

### Production (DBA-Controlled)

**File:** `appsettings.Production.json`

```json
{
  "ASPNETCORE_ENVIRONMENT": "production",
  "ConnectionStrings": {
    "DefaultConnection": "Host=prod-db.local;Port=5432;Database=ehr_platform;Username=ehr_admin;Password=${DB_PASSWORD}"
  }
}
```

**Behavior:**
- ✅ Highest safety
- ✅ DBA approval required
- ✅ No auto-migrations
- ✅ Full audit trail
- ❌ Manual process required

**Manual migration (DBA only):**

```bash
./backend/scripts/db-migrate.sh up --env=production
```

---

## Full Program.cs Example

### PatientService

```csharp
using EHRPlatform.Common.Data.Migrations;
using EHRPlatform.Services.Patient.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbContext
builder.Services.AddDbContext<PatientContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("EHRPlatform.Services.Patient")));

// Add logging
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
});

// Register migration strategy
var environment = builder.Environment.EnvironmentName;
new MigrationConfiguration(builder.Services)
    .WithEnvironment(environment)
    .AddContext<PatientContext>()
    .Build();

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// ✅ Apply migrations here
try
{
    await app.Services.RunMigrationsAsync<PatientContext>("PatientService");
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Failed to apply migrations");
    if (app.Environment.IsProduction())
        throw; // Fail-fast in production
}

app.Run();
```

### BillingService (Multiple DbContexts)

```csharp
using EHRPlatform.Common.Data.Migrations;
using EHRPlatform.Services.Billing.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();

// Add DbContexts
builder.Services.AddDbContext<BillingContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<AuditContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("AuditConnection")));

// Register migration strategies
var environment = builder.Environment.EnvironmentName;
new MigrationConfiguration(builder.Services)
    .WithEnvironment(environment)
    .AddContexts(typeof(BillingContext), typeof(AuditContext))
    .Build();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// ✅ Apply migrations for both contexts
try
{
    await app.Services.RunMigrationsAsync(
        (typeof(BillingContext), "BillingService"),
        (typeof(AuditContext), "AuditService")
    );
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Failed to apply migrations");
    if (app.Environment.IsProduction())
        throw;
}

app.Run();
```

---

## Advanced Configuration

### Custom Migration Policy

```csharp
// Override default policy for specific environment
builder.Services.AddScoped(sp =>
{
    var logger = sp.GetRequiredService<ILogger<MigrationExecutor>>();
    var policy = builder.Environment.EnvironmentName switch
    {
        "testing" => MigrationPolicy.AutomaticOnStartup,
        "custom" => MigrationPolicy.ManualOnly,
        _ => MigrationPolicy.Disabled
    };
    
    return new MigrationExecutor(logger, sp, policy);
});
```

### Conditional Migration Registration

```csharp
// Only migrate if database exists
if (builder.Configuration.GetValue<bool>("Features:RunMigrations"))
{
    new MigrationConfiguration(builder.Services)
        .WithEnvironment(environment)
        .AddContext<PatientContext>()
        .Build();
}
```

### Parallel Migration Execution

```csharp
// Run migrations for multiple services in parallel
await Task.WhenAll(
    app.Services.RunMigrationsAsync<PatientContext>("PatientService"),
    app.Services.RunMigrationsAsync<BillingContext>("BillingService"),
    app.Services.RunMigrationsAsync<AuditContext>("AuditService")
);
```

---

## Creating Migrations

### Step 1: Copy Template

```bash
cp backend/db/migrations/00_MIGRATION_TEMPLATE.sql \
   backend/db/migrations/YYYYMMDD_NNN_description.sql
```

### Step 2: Edit Migration

Replace placeholders with your changes:

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

### Step 3: Test

```bash
# Development
docker-compose up -d postgres
psql -h localhost -U ehr_user -d ehr_platform_dev -f backend/db/migrations/20250120_001_add-patient-tags.sql

# Verify
psql -h localhost -U ehr_user -d ehr_platform_dev -c "SELECT * FROM \"__MigrationHistory\" ORDER BY \"AppliedAt\" DESC LIMIT 1"
```

### Step 4: Create Rollback Script

```sql
-- backend/db/rollback/20250120_001_add-patient-tags_rollback.sql

-- Drop table
DROP TABLE IF EXISTS "PatientTags" CASCADE;

-- Remove from tracking
DELETE FROM "__MigrationHistory" WHERE "MigrationId" = '20250120_001_add-patient-tags';

COMMIT;
```

### Step 5: Commit

```bash
git add backend/db/migrations/20250120_001_add-patient-tags.sql
git add backend/db/rollback/20250120_001_add-patient-tags_rollback.sql
git commit -m "db(migration): Add patient tags support (20250120_001)"
```

---

## Troubleshooting

### Migration Won't Apply

**Error:** "Table already exists"

**Solution:** Check `__MigrationHistory` table:

```bash
psql -h localhost -U ehr_user -d ehr_platform_dev -c \
  "SELECT * FROM \"__MigrationHistory\" WHERE \"MigrationId\" LIKE '%tags%'"
```

If migration is already recorded, skip it in the migration file.

### Connection Timeout

**Error:** "Unable to connect to database"

**Solution:**

```bash
# Check connection string
grep DefaultConnection backend/appsettings.Development.json

# Test connection
psql -h localhost -U ehr_user -d ehr_platform_dev -c "SELECT 1"

# Check if database exists
psql -h localhost -U ehr_user -c "\l" | grep ehr_platform
```

### Schema Validation Fails

**Error:** "Expected table not found"

**Solution:**

```bash
# List all tables
psql -h localhost -U ehr_user -d ehr_platform_dev -c "\dt"

# Check migration history
psql -h localhost -U ehr_user -d ehr_platform_dev -c \
  "SELECT COUNT(*) FROM \"__MigrationHistory\""

# Re-apply baseline
psql -h localhost -U ehr_user -d ehr_platform_dev -f \
  backend/db/migrations/20250101_001_baseline.sql
```

### Lock Timeout

**Error:** "Timeout waiting for table lock"

**Solution:**

```bash
# Find blocking sessions
psql -h localhost -U ehr_user -d ehr_platform_dev -c \
  "SELECT * FROM pg_stat_activity WHERE state = 'active'"

# Kill blocking session (be careful!)
psql -h localhost -U ehr_user -d ehr_platform_dev -c \
  "SELECT pg_terminate_backend(pid) FROM pg_stat_activity 
   WHERE application_name = 'psql' AND pid != pg_backend_pid()"
```

---

## Best Practices

### ✅ DO

- [ ] Test migrations on development first
- [ ] Create rollback scripts before applying
- [ ] Use meaningful migration names
- [ ] Include comments explaining changes
- [ ] Track migrations in `__MigrationHistory`
- [ ] Create backups before production migrations
- [ ] Review migrations in pull requests

### ❌ DON'T

- [ ] Skip testing migrations
- [ ] Apply migrations directly to production
- [ ] Use cryptic migration names
- [ ] Manually edit `__MigrationHistory`
- [ ] Create overly large migrations (keep atomic)
- [ ] Forget to create rollback scripts
- [ ] Run multiple migrations in parallel on production

---

## CI/CD Integration

### GitHub Actions

Migrations run in CI/CD pipeline:

```yaml
# .github/workflows/deploy-k8s.yml

deploy-staging:
  steps:
    - name: Apply database migrations
      run: ./backend/scripts/db-migrate.sh up --env=staging
```

### Kubernetes

Define init container for migrations:

```yaml
# backend/k8s/ehr-platform/templates/deployment-microservice.yaml

initContainers:
- name: db-migration
  image: ehr-platform:latest
  command:
    - /bin/sh
    - -c
    - |
      dotnet EHRPlatform.Services.Patient.dll \
        --run-migrations \
        --connection-string="$CONNECTION_STRING"
  env:
  - name: CONNECTION_STRING
    valueFrom:
      secretKeyRef:
        name: db-credentials
        key: connection-string
```

---

## Monitoring & Alerting

### Track Migration Status

```bash
# Add to health check
curl https://api.ehr-platform.com/health/migrations
```

### Alert on Failed Migrations

```yaml
# backend/monitoring/prometheus-rules/alerts.yml

- alert: MigrationFailed
  expr: migration_status{status="failed"} == 1
  for: 5m
  annotations:
    summary: "Database migration failed"
    description: "Migration {{ $labels.migration }} failed on {{ $labels.instance }}"
```

---

## Support & Escalation

- **Development Issues:** Post in #database-dev Slack
- **Staging Issues:** Contact QA lead
- **Production Issues:** Page DBA on-call (urgent)

---

**Last Updated:** 2025-01-15  
**Maintained By:** Database Team
