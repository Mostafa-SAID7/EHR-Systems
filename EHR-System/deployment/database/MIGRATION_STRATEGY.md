# Database Migration Strategy

## Overview

EHR Platform uses Entity Framework Core for database migrations with a centralized approach for managing schema changes across all 7 database services.

## Migration Architecture

### Services with Databases

1. **Identity Service**: User accounts, roles, permissions
2. **Patient Service**: Patient demographics, medical history
3. **Appointment Service**: Appointment schedules, availability
4. **Integration Service**: External API mappings, message logs
5. **Terminology Service**: Medical code mappings (ICD, SNOMED)
6. **FileStorage Service**: Document metadata, versioning
7. **Clinical Service**: Clinical records, lab results

### Shared Database vs. Service Databases

- **Shared**: PostgreSQL cluster (RDS) with multiple databases
- **Per-Service Schema**: Each service owns its schema (schema-per-service pattern)
- **No Cross-Schema Foreign Keys**: Maintains loose coupling

## Migration Workflow

### Local Development

```bash
# Generate migration (from service directory)
cd EHR-System/services/Patient/src/Patient.Persistence
dotnet ef migrations add AddPatientTable

# Apply locally
dotnet ef database update

# Review generated SQL
dotnet ef migrations script
```

### CI/CD Pipeline

**Step 1: Build & Test**
```yaml
- name: Build
  run: dotnet build -c Release

- name: Test Migrations
  run: |
    for service in Identity Patient Appointment Integration Terminology FileStorage Clinical; do
      cd services/$service/src/$service.Persistence
      dotnet ef migrations list
      dotnet ef database update --environment Development
      cd ../../../../
    done
```

**Step 2: Generate Migration Scripts**
```bash
# Per environment
dotnet ef migrations script -o migrations-dev.sql -i --idempotent

# For diff-based migration
dotnet ef migrations script -f "LastKnownGoodMigration" -o migrations-upgrade.sql --idempotent
```

**Step 3: Validate & Merge**
- Peer review migration SQL
- Check for breaking changes
- Validate against production schema

**Step 4: Deploy**
```yaml
- name: Migrate Database
  env:
    ConnectionString: ${{ secrets.DB_CONNECTION_STRING }}
  run: |
    for service in Identity Patient Appointment Integration Terminology FileStorage Clinical; do
      dotnet ef database update --project services/$service/src/$service.Persistence
    done
```

## Migration File Structure

```
services/[Service]/src/[Service].Persistence/Migrations/
├── 20240101_001_InitialSchema.cs
├── 20240102_002_AddPatientDemographics.cs
├── 20240103_003_CreateAppointmentTables.cs
└── 20240104_004_AddIndexes.cs
```

## Naming Convention

**Format**: `YYYYMMDD_[Sequence]_[DescriptiveName].cs`

**Examples**:
- `20240801_001_InitialSchema.cs`
- `20240802_002_AddPatientTable.cs`
- `20240803_003_AddAuditColumns.cs`
- `20240804_004_CreateIndexes.cs`

## Idempotent Migrations

All migrations must be idempotent (safe to run multiple times):

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // Safe: Check before creating
    migrationBuilder.Sql(
        @"DO $$ BEGIN
            IF NOT EXISTS(SELECT 1 FROM information_schema.tables 
                          WHERE table_name = 'Patient') THEN
                CREATE TABLE Patient (...);
            END IF;
        END $$;"
    );
}
```

## Seed Data

### Development Seed

```csharp
// services/[Service]/src/[Service].Persistence/Data/SeedData.cs
public static class SeedData
{
    public static void SeedDatabase(ModelBuilder builder)
    {
        builder.Entity<Patient>().HasData(
            new Patient { Id = Guid.NewGuid(), Name = "John Doe", ... }
        );
    }
}

// In DbContext.OnModelCreating
protected override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);
    
    if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
    {
        SeedData.SeedDatabase(builder);
    }
}
```

### Production Seed

Use separate seed jobs (no embedded seeds in migrations):

```bash
# Kubernetes Job
kubectl apply -f deployment/k8s/seed-data-job.yaml
```

## Rollback Strategy

### Rollback a Migration

```bash
# Remove last migration
dotnet ef migrations remove

# Or revert to specific migration
dotnet ef database update "20240802_001_PreviousMigration"
```

### Emergency Rollback

```sql
-- Manual rollback script
BEGIN TRANSACTION;
  -- Reverse schema changes
  DROP TABLE IF EXISTS NewTable;
  ALTER TABLE OldTable ADD COLUMN RestoredColumn TYPE;
COMMIT;
```

## Validation & Testing

### Pre-Migration Checks

1. **Syntax Validation**: All migrations compile
2. **Idempotency**: Run each migration twice, expect same result
3. **Data Integrity**: Foreign key constraints valid
4. **Performance**: Test migration on production-like dataset

### Test Environment Flow

```
1. Create test database from production backup
2. Apply pending migrations
3. Run integrity checks
4. Measure migration time
5. Validate application compatibility
6. Approve for production
```

## Performance Considerations

### Large Table Changes

For tables with millions of rows:

```csharp
// Use WITH (ONLINE=ON) for SQL Server
// Use CONCURRENTLY for PostgreSQL
migrationBuilder.CreateIndex(
    name: "IX_Patient_MRN",
    table: "Patient",
    column: "MRN",
    unique: true
);

// For large tables in prod, run separately:
// CREATE INDEX CONCURRENTLY IX_Patient_MRN ON Patient(MRN);
```

## Monitoring Migrations

### Pre-Migration

```sql
-- Check current schema version
SELECT * FROM __EFMigrationsHistory 
ORDER BY MigrationId DESC 
LIMIT 10;

-- Estimate migration time on similar data size
EXPLAIN ANALYZE [MIGRATION_SCRIPT];
```

### During Migration

```sql
-- Monitor long-running queries
SELECT pid, usename, state, query, query_start 
FROM pg_stat_activity 
WHERE state != 'idle';
```

### Post-Migration

```sql
-- Verify schema
\dt+  -- List tables with sizes
\di+  -- List indexes with sizes

-- Check for orphaned objects
SELECT * FROM information_schema.tables 
WHERE table_schema = 'public';
```

## Multi-Service Coordination

### Dependency Order

When services have dependencies, migrate in order:

1. **Identity** (auth, base entities)
2. **Patient** (core data)
3. **Appointment** (depends on Patient)
4. **Clinical** (depends on Patient)
5. **Billing** (depends on Patient, Appointment)
6. **Integration** (depends on others)
7. **FileStorage** (independent)

### Cross-Service References

Avoid foreign keys across services. Use eventual consistency:

```csharp
// Patient Service
public class Patient
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    // NO: public Guid IdentityUserId { get; set; }
}

// Instead, use IDs and lazy load via API
public async Task<PatientWithUser> GetPatientWithUser(Guid patientId)
{
    var patient = await _patientRepo.GetAsync(patientId);
    var user = await _identityService.GetUserAsync(patient.IdentityUserId);
    return new PatientWithUser { Patient = patient, User = user };
}
```

## Troubleshooting

### Migration Conflicts

```bash
# List pending migrations
dotnet ef migrations list --verbose

# Remove conflicting migration
dotnet ef migrations remove

# Re-create with new name
dotnet ef migrations add ResolvedConflictName
```

### Failed Migration

```bash
# Check migration history
SELECT * FROM __EFMigrationsHistory WHERE MigrationId = 'xxxxx';

# Mark as failed (requires manual intervention)
# Contact database admin for manual rollback

# Re-apply after fix
dotnet ef database update
```

## Environment-Specific Configurations

### Development
- Seed data included
- Fast migrations (can break)
- No backup required

### Staging
- Production backup
- Full validation
- Test migration window

### Production
- Zero-downtime migrations only
- Manual approval required
- Complete rollback plan

## References

- [EF Core Migrations Documentation](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [PostgreSQL Migration Best Practices](https://wiki.postgresql.org/wiki/Performance_Optimization)
- [Zero-Downtime Migrations](https://www.citusdata.com/blog/2018/07/18/zero-downtime-postgres-schema-migrations-without-exclusive-locks/)
