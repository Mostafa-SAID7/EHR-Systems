# EHR Platform - Database Rollback Strategies

## Overview

Enterprise-grade rollback procedures for each environment with safety checks and monitoring.

---

## Environment-Specific Strategies

### Development: Quick Rollback

**Goal:** Rapid iteration with minimal data protection

```bash
# Option 1: Docker reset (fastest)
docker-compose down -v
docker-compose up -d
# Database recreates with baseline schema

# Option 2: Manual rollback via SQL
psql -h localhost -U ehr_user -d ehr_platform < db/rollback/20250115_001_rollback.sql

# Option 3: Delete file and restart
rm db/migrations/20250115_001_description.sql
docker-compose restart
```

**Time:** 1-2 minutes  
**Risk:** Low (dev data)  
**Data Loss:** Complete (if using -v flag)

---

### Staging: Safe Rollback

**Goal:** Data preservation with tested rollback scripts

```bash
# Step 1: Create backup (pre-rollback)
pg_dump -h staging-db.local -U ehr_user ehr_platform > backups/pre_20250115_rollback.sql

# Step 2: Execute rollback
psql -h staging-db.local -U ehr_user -d ehr_platform < db/rollback/20250115_001_rollback.sql

# Step 3: Verify rollback
SELECT * FROM "__MigrationHistory" ORDER BY "AppliedAt" DESC LIMIT 5;
SELECT COUNT(*) FROM "Patients"; -- Check data integrity

# Step 4: Validate application
curl https://api-staging.ehr-platform.local/health
```

**Time:** 5-10 minutes  
**Risk:** Medium (staging data recoverable from backup)  
**Data Loss:** None (if rollback script is correct)

---

### Production: Controlled Rollback

**Goal:** Minimal disruption with DBA oversight

```bash
# Step 0: Pre-rollback checklist
# ✅ DBA approval obtained
# ✅ Full backup created
# ✅ Rollback script reviewed
# ✅ Maintenance window scheduled
# ✅ On-call team notified

# Step 1: Enter maintenance mode
curl -X POST https://api.ehr-platform.com/admin/maintenance \
  -H "Authorization: Bearer $ADMIN_TOKEN"
# Response: All traffic redirected to maintenance page

# Step 2: Stop all services gracefully
kubectl scale deployment api-gateway -n ehr-platform --replicas=0
kubectl scale statefulset postgresql -n ehr-platform --replicas=0 --grace-period=60

# Step 3: Full database backup
pg_dump -h prod-db.local -U ehr_admin ehr_platform \
  --format=custom --verbose \
  --file=backups/pre_production_rollback_$(date +%Y%m%d_%H%M%S).dump

# Step 4: Disable replication (if applicable)
# Contact DBA to pause streaming replication

# Step 5: Execute rollback
psql -h prod-db.local -U ehr_admin -d ehr_platform < db/rollback/20250115_001_rollback.sql

# Step 6: Validation checks
psql -h prod-db.local -U ehr_admin -d ehr_platform << EOF
-- Verify migration was removed
SELECT COUNT(*) FROM "__MigrationHistory" WHERE "MigrationId" = '20250115_001_description';
-- Should return: 0

-- Check data consistency
SELECT COUNT(*) FROM "Patients" WHERE "CreatedAt" > NOW() - INTERVAL '1 hour';
-- Should return: approximately same as before migration

-- Verify no orphaned records
SELECT COUNT(*) FROM "OutboxEvents" WHERE "AggregateId" NOT IN (SELECT "Id" FROM "Patients");
-- Should return: 0 (no orphaned events)
EOF

# Step 7: Enable replication
# Contact DBA to resume streaming replication

# Step 8: Restart services
kubectl scale deployment api-gateway -n ehr-platform --replicas=3
kubectl scale statefulset postgresql -n ehr-platform --replicas=1

# Step 9: Health check
kubectl get pods -n ehr-platform
curl https://api.ehr-platform.com/health

# Step 10: Exit maintenance mode
curl -X DELETE https://api.ehr-platform.com/admin/maintenance \
  -H "Authorization: Bearer $ADMIN_TOKEN"

# Step 11: Post-mortem
# Analyze root cause
# Document what happened
# Update runbooks if needed
```

**Time:** 30-60 minutes  
**Risk:** Low (DBA-controlled with backups)  
**Data Loss:** None (full backup before rollback)  
**Downtime:** 10-15 minutes (during maintenance window)

---

## Rollback Script Templates

### Development Rollback

```sql
-- ═══════════════════════════════════════════════════════════════════════════════
-- ROLLBACK: 20250115_001 - Add Patient Search Indexes
-- ═══════════════════════════════════════════════════════════════════════════════

-- Drop indexes
DROP INDEX IF EXISTS "IX_Patients_LastName_FirstName" CASCADE;
DROP INDEX IF EXISTS "IX_Patients_Status" CASCADE;

-- Remove from tracking
DELETE FROM "__MigrationHistory" 
WHERE "MigrationId" = '20250115_001_add-patient-search-indexes';

COMMIT;
```

### Staging/Production Rollback

```sql
-- ═══════════════════════════════════════════════════════════════════════════════
-- ROLLBACK: 20250115_001 - Add Patient Search Indexes
-- Version: 1.0.0
-- Requires: PostgreSQL 14+, pg_trgm extension
-- ═══════════════════════════════════════════════════════════════════════════════

-- Step 1: Verify no active transactions using new indexes
DO $$
DECLARE
    active_count INTEGER;
BEGIN
    SELECT COUNT(*)
    INTO active_count
    FROM pg_stat_activity
    WHERE query ILIKE '%Patients%' AND state = 'active';
    
    IF active_count > 0 THEN
        RAISE EXCEPTION 'Cannot rollback: % active queries on Patients table', active_count;
    END IF;
END $$;

-- Step 2: Drop indexes in reverse order of creation
DROP INDEX CONCURRENTLY IF EXISTS "IX_Patients_LastName_FirstName";
DROP INDEX CONCURRENTLY IF EXISTS "IX_Patients_Status";

-- Step 3: Verify indexes are dropped
DO $$
DECLARE
    index_count INTEGER;
BEGIN
    SELECT COUNT(*)
    INTO index_count
    FROM pg_indexes
    WHERE tablename = 'Patients' 
    AND indexname IN ('IX_Patients_LastName_FirstName', 'IX_Patients_Status');
    
    IF index_count > 0 THEN
        RAISE EXCEPTION 'Rollback failed: % indexes still exist', index_count;
    END IF;
    
    RAISE NOTICE 'Rollback successful: All indexes dropped';
END $$;

-- Step 4: Remove from migration history
DELETE FROM "__MigrationHistory" 
WHERE "MigrationId" = '20250115_001_add-patient-search-indexes';

-- Step 5: Log rollback event
INSERT INTO "AuditEntries" ("Id", "UserId", "Action", "EntityType", "EntityId", "Timestamp")
VALUES (
    uuid_generate_v4(),
    NULL,
    'MIGRATION_ROLLBACK',
    'Database',
    NULL,
    CURRENT_TIMESTAMP
);

COMMIT;
```

---

## Automated Rollback Triggers

### Automatic Rollback Conditions

```yaml
# .kiro/rollback-triggers.yml
triggers:
  # Health check failures
  - name: health_check_failure
    condition: "GET /health returns != 200"
    grace_period: 30s
    action: rollback_immediate
    
  # Error rate spike
  - name: error_rate_spike
    condition: "error_rate > 5% for 2 minutes"
    grace_period: 120s
    action: rollback_immediate
    
  # Database connectivity loss
  - name: db_connection_loss
    condition: "database ping fails"
    grace_period: 10s
    action: rollback_immediate
    
  # Slow queries detected
  - name: slow_queries
    condition: "P95 latency > 5s for 5 minutes"
    grace_period: 300s
    action: manual_review_first
```

---

## Data Integrity Validation

### Pre-Rollback Checks

```sql
-- 1. Backup verification
SELECT COUNT(*) as backup_tables FROM information_schema.tables 
WHERE table_schema = 'backup' 
AND table_name LIKE 'backup_%';

-- 2. Data consistency
SELECT 
    (SELECT COUNT(*) FROM "Patients") as patient_count,
    (SELECT COUNT(*) FROM "Appointments") as appointment_count,
    (SELECT COUNT(*) FROM "OutboxEvents" WHERE "IsPublished" = false) as unpublished_events,
    (SELECT COUNT(*) FROM "Invoices") as invoice_count;

-- 3. Orphaned records
SELECT COUNT(*) as orphaned_appointments 
FROM "Appointments" 
WHERE "PatientId" NOT IN (SELECT "Id" FROM "Patients");

-- 4. Transaction status
SELECT COUNT(*) as active_transactions FROM pg_stat_activity WHERE state = 'active';
```

### Post-Rollback Validation

```sql
-- Verify migration was removed
SELECT COUNT(*) as migration_count 
FROM "__MigrationHistory" 
WHERE "MigrationId" LIKE '20250115%';

-- Verify data wasn't lost
SELECT 
    'Patients' as table_name, COUNT(*) as count FROM "Patients"
    UNION ALL
    SELECT 'Appointments', COUNT(*) FROM "Appointments"
    UNION ALL
    SELECT 'Invoices', COUNT(*) FROM "Invoices";

-- Check for any locks
SELECT * FROM pg_stat_activity WHERE state != 'idle';
```

---

## Rollback Monitoring

### During Rollback

```bash
# Monitor database
watch -n 1 'psql -U ehr_admin -d ehr_platform -c "SELECT * FROM pg_stat_activity WHERE state = '\''active'\''"'

# Monitor disk I/O
iostat -x 1

# Monitor CPU
top

# Watch application logs
kubectl logs -f deployment/api-gateway -n ehr-platform
```

### Post-Rollback Verification

```bash
# Health check
curl -v https://api.ehr-platform.com/health

# Database connectivity
psql -h prod-db.local -U ehr_admin -d ehr_platform -c "SELECT 1"

# Application restart
kubectl rollout status deployment/api-gateway -n ehr-platform --timeout=5m

# User-facing endpoints
curl https://api.ehr-platform.com/api/patients/me
curl https://api.ehr-platform.com/api/appointments?patientId=...
```

---

## Emergency Procedures

### Complete Database Recovery

```bash
# Step 1: Restore from full backup
pg_restore -h prod-db.local -U ehr_admin -d ehr_platform --verbose \
  backups/pre_production_rollback_20250115_143022.dump

# Step 2: Check restored state
psql -h prod-db.local -U ehr_admin -d ehr_platform -c \
  "SELECT COUNT(*) FROM \"__MigrationHistory\""

# Step 3: Rebuild indexes (if needed)
REINDEX DATABASE ehr_platform;

# Step 4: Analyze tables (for query optimization)
ANALYZE;

# Step 5: Restart services
kubectl restart deployment/api-gateway -n ehr-platform
```

### Partial Data Recovery

```sql
-- Recover specific patient data from point-in-time
SELECT * FROM pg_class WHERE relname = 'Patients';

-- Use MVCC (Multi-Version Concurrency Control) if transaction log available
-- Contact DBA for point-in-time recovery (PITR)
```

---

## Communication Template

### Staging Rollback Notification

```
🚨 STAGING DATABASE ROLLBACK IN PROGRESS

Migration: 20250115_001 - Add Patient Search Indexes
Status: ROLLING BACK (Duration: ~5 min)
Impact: Staging API temporarily unavailable
Expected Recovery: 2:15 PM EST

Reason: Performance regression detected in patient search
Alternative: Use advanced filters instead of full-text search

Updates will be posted in #database-alerts
```

### Production Rollback Notification

```
🚨 PRODUCTION DATABASE ROLLBACK IN PROGRESS

Migration: 20250115_001 - Add Patient Search Indexes
Status: ROLLING BACK (Duration: ~15 min)
Impact: API in MAINTENANCE MODE - Users redirected
Expected Recovery: 3:45 PM EST

Root Cause: Performance impact on critical query path
Timeline:
  3:30 PM - Maintenance mode enabled
  3:35 PM - Rollback started
  3:45 PM - Rollback complete
  3:50 PM - Full validation
  4:00 PM - Services online

Workaround: Manual patient lookup available in admin console
Postmortem: Friday 2 PM

DBA: Alice Smith | On-call: Bob Jones
```

---

**Last Updated:** 2025-01-15  
**Version:** 1.0.0
