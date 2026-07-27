# EHR Platform - Migration Versioning & Tracking

## Version Format

```
YYYYMMDD_NNN_description

YYYYMMDD  = Date (e.g., 20250115 = January 15, 2025)
NNN       = Sequential (001, 002, 003 per day)
description = Kebab-case description (e.g., add-patient-indexes)
```

**Example:** `20250115_001_add-patient-indexes.sql`

---

## Migration History

### Phase 1: Baseline (v1.0.0)

| Migration | Date | Description | Status |
|-----------|------|-------------|--------|
| `20250101_001_baseline.sql` | 2025-01-01 | Initial schema for all services | ✅ Applied |

**Services Included:**
- Patients table + indexes
- Appointments table + indexes
- Invoices (Billing) table + indexes
- AuditEntries table + indexes
- Users (Identity) table + indexes
- Reports (Analytics) table + indexes
- OutboxEvents (common infrastructure)

---

## Environment-Specific Versions

### Development (`develop` branch)
- **Auto-migrate:** All pending migrations applied on startup
- **Frequency:** Rapid (multiple migrations per sprint)
- **Rollback:** Simple (delete migration file, reset DB)

### Staging (`release/*` branches)
- **Manual apply:** Via `db-migrate.sh` before deployment
- **Frequency:** Weekly (end of sprint)
- **Rollback:** Via SQL rollback scripts

### Production (`main` branch)
- **Manual apply:** Via DBA-approved script + monitoring
- **Frequency:** Monthly (controlled release window)
- **Rollback:** Via pre-tested rollback scripts
- **Review:** All migrations reviewed before production

---

## Running Migrations

### Automatic (Development)

Services automatically migrate on startup:

```bash
docker-compose up -d
# Migrations applied automatically
```

### Manual (Staging/Production)

#### Option 1: Using Script

```bash
# Apply all pending migrations
./scripts/db-migrate.sh up

# Rollback to previous version
./scripts/db-migrate.sh down

# Show current version
./scripts/db-migrate.sh version

# Show pending migrations
./scripts/db-migrate.sh pending
```

#### Option 2: Using EF Core CLI

```bash
# Get pending migrations
dotnet ef migrations list --startup-project src/EHRPlatform.Services.Patient

# Apply specific migration
dotnet ef database update 20250115_001 --startup-project src/EHRPlatform.Services.Patient

# Rollback to previous migration
dotnet ef database update 20250101_001 --startup-project src/EHRPlatform.Services.Patient
```

#### Option 3: Direct SQL

```bash
# Connect to database
psql -h localhost -U ehr_user -d ehr_platform

# Execute migration script
\i /path/to/migrations/20250115_001_description.sql

# Verify
SELECT * FROM "__MigrationHistory" ORDER BY "AppliedAt" DESC;
```

---

## Migration Naming Conventions

### ✅ GOOD

```
20250115_001_add-patient-indexes.sql
20250115_002_create-audit-triggers.sql
20250120_001_add-billing-foreign-keys.sql
```

### ❌ BAD

```
migration.sql                    # No date, no sequence
add_indexes.sql                  # No date, unclear
20250115_AddPatientIndexes.sql   # PascalCase (use kebab-case)
20250115_001.sql                 # Missing description
```

---

## Creating a New Migration

### Step 1: Copy Template

```bash
cp db/migrations/00_MIGRATION_TEMPLATE.sql db/migrations/YYYYMMDD_NNN_description.sql
```

### Step 2: Edit Template

Replace placeholders:
- `YYYYMMDD` = Today's date
- `NNN` = Next sequence number
- `description` = Brief description in kebab-case

### Step 3: Write SQL

Edit the migration file with your changes:

```sql
-- ═══════════════════════════════════════════════════════════════════════════════
-- EHR Platform Database - [Your Description]
-- Version: 20250115_001
-- Created: 2025-01-15
-- Description: Add comprehensive patient search indexes for performance
-- ═══════════════════════════════════════════════════════════════════════════════

-- Your SQL changes here
CREATE INDEX IF NOT EXISTS "IX_Patients_LastName_FirstName" 
    ON "Patients" ("LastName", "FirstName");

-- Track migration
INSERT INTO "__MigrationHistory" ("MigrationId", "ProductVersion")
VALUES ('20250115_001_add-patient-search-indexes', '8.0.0')
ON CONFLICT DO NOTHING;

COMMIT;
```

### Step 4: Test

```bash
# Test on local development database first
psql -h localhost -U ehr_user -d ehr_platform_dev < db/migrations/20250115_001_description.sql

# Verify success
SELECT * FROM "__MigrationHistory" WHERE "MigrationId" LIKE '20250115%';
```

### Step 5: Commit

```bash
git add db/migrations/20250115_001_description.sql
git commit -m "db(migration): Add patient search indexes (20250115_001)"
```

---

## Rollback Procedures

### Development

1. Delete the migration file
2. Run `docker-compose down && docker-compose up -d`
3. Database resets automatically

### Staging

1. Execute the rollback SQL in the migration file comments
2. Update `__MigrationHistory` table

```sql
-- Reverse the changes
DROP INDEX IF EXISTS "IX_NewIndex";

-- Remove from tracking
DELETE FROM "__MigrationHistory" 
WHERE "MigrationId" = '20250115_001_description';
```

### Production

1. **NEVER** auto-rollback—contact DBA
2. DBA executes pre-tested rollback script
3. Monitor application behavior post-rollback
4. Post-mortem review

---

## Migration Best Practices

### ✅ DO

- [ ] Use `IF NOT EXISTS` / `IF EXISTS` clauses
- [ ] Create indexes immediately after table creation
- [ ] Test on development first
- [ ] Write clear, descriptive comments
- [ ] Include rollback procedures in comments
- [ ] Follow naming conventions strictly
- [ ] Review migrations before committing
- [ ] Keep migrations atomic (one logical change per file)

### ❌ DON'T

- [ ] Use `DROP TABLE` without CASCADE (risky!)
- [ ] Skip testing on development
- [ ] Use cryptic descriptions
- [ ] Create multiple unrelated changes in one migration
- [ ] Run migrations without backups
- [ ] Manually edit `__MigrationHistory` (use SQL instead)
- [ ] Apply migrations without version control

---

## Monitoring & Validation

### Check Current Version

```bash
psql -h localhost -U ehr_user -d ehr_platform -c \
  "SELECT * FROM \"__MigrationHistory\" ORDER BY \"AppliedAt\" DESC LIMIT 10;"
```

### Verify Schema

```bash
# List all tables
\dt

# List all indexes
\di

# Check table structure
\d "Patients"
```

### Performance Check

```sql
-- Verify indexes are being used
EXPLAIN ANALYZE SELECT * FROM "Patients" WHERE "Status" = 1;

-- Check index size
SELECT 
    schemaname, 
    tablename, 
    indexname, 
    pg_size_pretty(pg_relation_size(indexrelid)) AS index_size
FROM pg_indexes
WHERE schemaname = 'public'
ORDER BY pg_relation_size(indexrelid) DESC;
```

---

## CI/CD Integration

### GitHub Actions

Migrations run in this order:

1. **CI Build** (`ci-build.yml`): Lint migrations
2. **Dev Deploy**: Auto-run migrations
3. **Staging Deploy**: Manual approval, then run migrations
4. **Production Deploy**: DBA manual execution

See `.github/workflows/db-migrate.yml` for details.

---

## Troubleshooting

### Migration Failed

```bash
# Check logs
docker logs ehr-postgres  # or your container name

# Get migration status
SELECT * FROM "__MigrationHistory" ORDER BY "AppliedAt" DESC;

# Test migration manually
psql -h localhost -U ehr_user -d ehr_platform_test < db/migrations/YYYYMMDD_NNN.sql
```

### Lock on Table

```sql
-- List active locks
SELECT * FROM pg_stat_activity WHERE state = 'active';

-- Kill blocking session
SELECT pg_terminate_backend(pid) FROM pg_stat_activity 
WHERE application_name = 'psql' AND pid != pg_backend_pid();
```

### Rollback Issues

1. Check `__MigrationHistory` for applied migrations
2. Review migration file for rollback instructions
3. Execute rollback SQL manually if needed
4. Contact DBA for production issues

---

## Contact & Escalation

- **Development Issues:** Post in #database-dev Slack
- **Staging Issues:** Contact QA lead
- **Production Issues:** Page DBA on-call (urgent)

---

**Last Updated:** 2025-01-15
**Maintained By:** Database Team
