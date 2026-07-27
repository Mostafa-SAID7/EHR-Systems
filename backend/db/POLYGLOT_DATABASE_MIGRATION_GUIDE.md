# Polyglot Database Migration Guide - EHR Platform

## Overview

Enterprise EHR platform uses **multiple database technologies** optimized for different use cases:

| Database | Purpose | Executor | Path |
|---|---|---|---|
| **PostgreSQL** | Relational data (structured) | MigrationExecutor | `db/migrations/*.sql` |
| **MongoDB** | Documents (unstructured) | MongoMigrationExecutor | `db/migrations/mongo/*.js` |
| **Elasticsearch** | Full-text search | Manual index templates | `db/migrations/elasticsearch/` |
| **Redis** | Cache layer | CLI commands | N/A (ephemeral) |

---

## Why Polyglot?

### Decision Matrix

```
┌─────────────────────┬────────────┬────────────┬────────────────────────┐
│ Data Pattern        │ PostgreSQL │ MongoDB    │ Reason                 │
├─────────────────────┼────────────┼────────────┼────────────────────────┤
│ Patients            │ ✅ Primary │ ✗          │ Relational, consistent │
│ Appointments        │ ✅ Primary │ ✗          │ ACID constraints       │
│ Invoices            │ ✅ Primary │ ✗          │ Financial, immutable   │
│ Clinical Notes      │ ✗          │ ✅ Primary │ Flexible content, blob │
│ Lab Results         │ ✅ Primary │ ✗          │ Structured data        │
│ Audit Logs          │ ✅ Primary │ ✗          │ Immutability required  │
│ User Preferences    │ ✗          │ ✅ Primary │ Dynamic schema         │
│ Tags/Metadata       │ ✅ Primary │ ✗          │ Consistency            │
│ Document Cache      │ ✗          │ ✅ Primary │ Denormalized search    │
│ Search Index        │ ✗          │ ✗          │ Elasticsearch         │
└─────────────────────┴────────────┴────────────┴────────────────────────┘
```

---

## Database-Specific Migration Guides

### 1. PostgreSQL Migrations

**Use for:** Relational, ACID-guaranteed data  
**Executor:** `MigrationExecutor` (.NET)  
**Format:** SQL scripts (`.sql`)  
**Location:** `backend/db/migrations/`

#### Quick Start

```bash
# Create migration
cp backend/db/migrations/00_MIGRATION_TEMPLATE.sql \
   backend/db/migrations/20250120_001_description.sql

# Edit and test
nano backend/db/migrations/20250120_001_description.sql

# Commit
git add backend/db/migrations/20250120_001_description.sql
git commit -m "db(sql): Description (20250120_001)"
```

#### Example: Add Patient Indexes

```sql
-- backend/db/migrations/20250120_001_add-patient-indexes.sql
CREATE INDEX IF NOT EXISTS "IX_Patients_Status" ON "Patients" ("Status");
CREATE INDEX IF NOT EXISTS "IX_Patients_CreatedAt" ON "Patients" ("CreatedAt");

INSERT INTO "__MigrationHistory" ("MigrationId", "ProductVersion")
VALUES ('20250120_001_add-patient-indexes', '8.0.0')
ON CONFLICT DO NOTHING;

COMMIT;
```

#### See Also

- `MIGRATION_VERSION.md` - SQL versioning & tracking
- `ROLLBACK_STRATEGIES.md` - Rollback procedures
- `INTEGRATION_GUIDE.md` - Service integration

---

### 2. MongoDB Migrations

**Use for:** Document-based, schema-flexible data  
**Executor:** `MongoMigrationExecutor` (.NET)  
**Format:** JavaScript (`.js`)  
**Location:** `backend/db/migrations/mongo/`

#### Quick Start

```bash
# Create migration
cp backend/db/migrations/mongo/00_MIGRATION_TEMPLATE.js \
   backend/db/migrations/mongo/20250120_001_description.js

# Edit and test
nano backend/db/migrations/mongo/20250120_001_description.js

# Commit
git add backend/db/migrations/mongo/20250120_001_description.js
git commit -m "db(mongo): Description (20250120_001)"
```

#### Example: Add Clinical Document Indexes

```javascript
// backend/db/migrations/mongo/20250120_001_add-clinical-indexes.js

// Create indexes for patient queries
db.ClinicalDocuments.createIndex({ patientId: 1 });
db.ClinicalDocuments.createIndex({ patientId: 1, documentType: 1, createdAt: -1 });

// Create text index for search
db.ClinicalDocuments.createIndex({ content: "text", "tags": "text" });

// Track migration
db.__MigrationHistory.insertOne({
    migrationId: "20250120_001_add-clinical-indexes",
    appliedAt: new Date(),
    productVersion: "1.0.0"
});

print("✅ Migration complete");
```

#### See Also

- `MONGODB_MIGRATIONS.md` - Full MongoDB guide
- Examples: Collection creation, bulk operations, transactions

---

### 3. Elasticsearch Migrations

**Use for:** Full-text search, analytics  
**Executor:** Manual (index templates)  
**Format:** JSON mappings  
**Location:** `backend/db/migrations/elasticsearch/`

#### Quick Start

```bash
# Create index template
cat > backend/db/migrations/elasticsearch/20250120_001_clinical-search-template.json << 'EOF'
{
  "index_patterns": ["clinical-*"],
  "template": {
    "settings": {
      "number_of_shards": 1,
      "number_of_replicas": 1,
      "analysis": {
        "analyzer": {
          "clinical_analyzer": {
            "type": "standard",
            "stopwords": "_english_"
          }
        }
      }
    },
    "mappings": {
      "properties": {
        "patientId": { "type": "keyword" },
        "documentType": { "type": "keyword" },
        "content": { 
          "type": "text",
          "analyzer": "clinical_analyzer"
        },
        "tags": { "type": "keyword" },
        "createdAt": { "type": "date" }
      }
    }
  }
}
EOF

# Apply template
curl -X PUT "localhost:9200/_index_template/clinical-template" \
  -H 'Content-Type: application/json' \
  -d @backend/db/migrations/elasticsearch/20250120_001_clinical-search-template.json
```

---

## Cross-Database Scenarios

### Scenario 1: New Patient Record (Multiple Databases)

**Flow:**
1. Insert into PostgreSQL `Patients` table (primary)
2. Create MongoDB document for preferences (document store)
3. Index in Elasticsearch (search layer)
4. Cache in Redis (performance)

**Migrations Needed:**
```
PostgreSQL: 
  - CREATE TABLE "Patients" (...)
  - CREATE INDEX "IX_Patients_Email" (...)

MongoDB:
  - db.createCollection("PatientPreferences", {...})
  - db.PatientPreferences.createIndex({ patientId: 1 }, { unique: true })

Elasticsearch:
  - PUT _index_template/patients-template {...}
```

### Scenario 2: Add Clinical Notes Field (Multiple Databases)

**Flow:**
1. Add `notes_content` to PostgreSQL `ClinicalNotes` table (new column)
2. Create MongoDB `ClinicalDocuments` collection (document storage)
3. Add Elasticsearch mapping for full-text search
4. Cache invalidation in Redis

**Migrations Needed:**
```
PostgreSQL:
  ALTER TABLE "ClinicalNotes" ADD COLUMN "notes_content" TEXT;

MongoDB:
  db.createCollection("ClinicalDocuments", {...})

Elasticsearch:
  PUT _index_template/clinical-template {...}
```

---

## Migration Ordering & Dependencies

### Critical Order

```
1. PostgreSQL: Create base tables/constraints
2. MongoDB: Create collections/validation
3. Elasticsearch: Create index templates
4. Redis: (auto-managed, no migrations)
5. Application: Deploy with code changes
```

### Example Timeline

```
T+0:00   PostgreSQL baseline created
T+0:05   PostgreSQL indexes added
T+0:10   MongoDB collections created
T+0:15   Elasticsearch templates applied
T+0:20   Redis cache warmed (auto)
T+0:25   All services migrated
T+0:30   Health checks pass
T+0:35   Deployment complete ✅
```

---

## Data Consistency Patterns

### Pattern 1: Write-Through (Transactional)

✅ **Best for:** Appointments, Invoices (critical data)

```csharp
// Write to PostgreSQL first (ACID guaranteed)
appointment = await db.Appointments.AddAsync(new Appointment { ... });
await db.SaveChangesAsync();

// Then write to search layer (non-critical)
await elasticsearchService.IndexAsync(appointment);

// Cache invalidation
await cacheService.RemoveAsync($"appointments:{patientId}");
```

### Pattern 2: Event-Driven (Eventual Consistency)

✅ **Best for:** Clinical documents, analytics

```csharp
// 1. Write to PostgreSQL
event = new ClinicalNoteCreatedEvent { ... };
clinicalNote = await db.ClinicalNotes.AddAsync(clinicalNote);
await db.SaveChangesAsync();

// 2. Outbox pattern: Write to Kafka topic
await outboxRepository.AddEventAsync(event);

// 3. Async processors handle the rest:
//    - MongoDB: Document storage
//    - Elasticsearch: Search indexing
//    - Cache: Invalidation
```

### Pattern 3: Read Replicas (Denormalization)

✅ **Best for:** Analytics, reporting

```csharp
// PostgreSQL: Normalized data (write source)
INSERT INTO "Patients" VALUES (...)

// MongoDB: Denormalized for fast reads
db.PatientDenormalized.insertOne({
    patientId: ...,
    name: ...,
    contactInfo: { ... }
})

// Elasticsearch: Pre-aggregated metrics
PUT analytics-index/_doc/patient-metrics {
    patientId: ...,
    totalVisits: 25,
    averageRating: 4.8
}
```

---

## Environment-Specific Strategies

### Development

```yaml
PostgreSQL:   AUTO    # Automatic on startup
MongoDB:      AUTO    # Automatic on startup
Elasticsearch: AUTO   # Auto-create indexes
Redis:        N/A     # Ephemeral
```

### Staging

```yaml
PostgreSQL:   MANUAL  # Script-based approval
MongoDB:      MANUAL  # Script-based approval
Elasticsearch: MANUAL # Template deployment
Redis:        N/A     # Auto-reset per deployment
```

### Production

```yaml
PostgreSQL:   DBA-APPROVED    # Manual + audit trail
MongoDB:      DBA-APPROVED    # Manual + audit trail
Elasticsearch: APPROVED        # Template deployment
Redis:        AUTO            # Ephemeral data
```

---

## CI/CD Pipeline

### GitHub Actions Flow

```
┌──────────────────────────────────────────────────────────┐
│ 1. Validate                                              │
│    - SQL syntax check (sqlfluff)                         │
│    - MongoDB JS syntax (JSHint)                          │
│    - Elasticsearch JSON validation                       │
└──────────────────────────────────────────────────────────┘
                          ↓
┌──────────────────────────────────────────────────────────┐
│ 2. Lint                                                  │
│    - SQL best practices                                  │
│    - MongoDB idempotency check                           │
│    - Elasticsearch mapping review                        │
└──────────────────────────────────────────────────────────┘
                          ↓
┌──────────────────────────────────────────────────────────┐
│ 3. Test (DEV)                                            │
│    - Run all migrations on test DB                       │
│    - Verify schema integrity                             │
│    - Check indexes created                               │
└──────────────────────────────────────────────────────────┘
                          ↓
┌──────────────────────────────────────────────────────────┐
│ 4. Approve (STAGING)                                     │
│    - Manual review required                              │
│    - DBA approves SQL changes                            │
│    - Data impact assessment                              │
└──────────────────────────────────────────────────────────┘
                          ↓
┌──────────────────────────────────────────────────────────┐
│ 5. Deploy (PROD)                                         │
│    - Execute approved migrations                         │
│    - Monitor application behavior                        │
│    - Immediate rollback capability                       │
└──────────────────────────────────────────────────────────┘
```

---

## Best Practices Summary

### ✅ DO

- [ ] Test all migrations on development first
- [ ] Use environment-specific strategies
- [ ] Create rollback scripts for each migration
- [ ] Document cross-database dependencies
- [ ] Version all migration files
- [ ] Track migration history in each database
- [ ] Use idempotent operations (safe to run twice)
- [ ] Commit migration files with application code

### ❌ DON'T

- [ ] Skip testing on development
- [ ] Run different migrations on staging vs production
- [ ] Delete migration files after applying
- [ ] Manually edit migration history records
- [ ] Create unidirectional data flows
- [ ] Mix multiple database changes in one file
- [ ] Deploy without rollback procedure

---

## Migration Checklists

### Pre-Deployment Checklist

- [ ] All migration files created and committed
- [ ] Tested on development environment
- [ ] Rollback procedures documented
- [ ] Data impact assessed
- [ ] Performance implications reviewed
- [ ] Backup created
- [ ] Monitoring enabled
- [ ] Team notified

### Post-Deployment Checklist

- [ ] Migrations applied successfully
- [ ] Application health checks pass
- [ ] Data integrity verified
- [ ] Performance metrics normal
- [ ] No error spikes in logs
- [ ] User acceptance testing complete
- [ ] Monitoring shows green
- [ ] Post-mortem (if issues occurred)

---

## Support & Escalation

### By Database

| Database | Contact | Priority |
|---|---|---|
| PostgreSQL | #database-sql | High (critical path) |
| MongoDB | #database-mongo | Medium (documents) |
| Elasticsearch | #search-team | Medium (analytics) |
| Redis | #devops-team | Medium (cache) |

### Escalation Path

1. **Development Issue** → Reach out in Slack channel
2. **Staging Issue** → QA Lead approval
3. **Production Issue** → Page DBA on-call (urgent)

---

## Reference Links

- [`MIGRATION_VERSION.md`](MIGRATION_VERSION.md) - SQL versioning
- [`ROLLBACK_STRATEGIES.md`](ROLLBACK_STRATEGIES.md) - SQL rollback
- [`MONGODB_MIGRATIONS.md`](MONGODB_MIGRATIONS.md) - MongoDB guide
- [`INTEGRATION_GUIDE.md`](INTEGRATION_GUIDE.md) - .NET integration

---

**Last Updated:** 2025-01-15  
**Version:** 1.0.0  
**Maintained By:** Database + DevOps Team
