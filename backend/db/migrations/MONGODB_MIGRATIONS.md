# MongoDB Migrations - EHR Platform

## Overview

MongoDB migration system for document-based data (clinical documents, preferences, audit logs).

**Location:** `backend/db/migrations/mongo/`  
**Executor:** `MongoMigrationExecutor.cs` in EHRPlatform.Common  
**Tracking:** `__MigrationHistory` collection

---

## Version Format

```
YYYYMMDD_NNN_description.js

YYYYMMDD  = Date (e.g., 20250115 = January 15, 2025)
NNN       = Sequential number (001, 002, 003 per day)
description = Kebab-case description
```

**Example:** `20250115_001_add-clinical-indexes.js`

---

## Running MongoDB Migrations

### From .NET Code (Recommended)

```csharp
// In Program.cs
var mongoDb = services.GetRequiredService<IMongoDatabase>();

// Single migration
await services.RunMongoMigrationAsync(
    "20250115_001_add-clinical-indexes",
    async (db) => 
    {
        await db.GetCollection<ClinicalDocument>("ClinicalDocuments")
            .Indexes.CreateOneAsync(
                new CreateIndexModel<ClinicalDocument>(
                    Builders<ClinicalDocument>.IndexKeys.Ascending(d => d.PatientId)
                )
            );
    }
);
```

### From MongoDB Shell

```javascript
// Connect to MongoDB
mongosh "mongodb://localhost:27017/ehr_patient"

// Run migration file
load("/path/to/backend/db/migrations/mongo/20250115_001_add-clinical-indexes.js")
```

### From Docker

```bash
docker exec -i ehr-mongodb mongosh /dev/stdin < \
    backend/db/migrations/mongo/20250115_001_add-clinical-indexes.js
```

---

## Migration Examples

### 1. Create Collection with Schema Validation

```javascript
db.createCollection("ClinicalDocuments", {
    validator: {
        $jsonSchema: {
            bsonType: "object",
            required: ["_id", "patientId", "documentType", "createdAt"],
            properties: {
                _id: { bsonType: "objectId" },
                patientId: { bsonType: "string" },
                documentType: { enum: ["NoteText", "LabResult", "Imaging"] },
                content: { bsonType: "string" },
                createdAt: { bsonType: "date" },
                isDeleted: { bsonType: "bool" }
            }
        }
    }
});

print("✅ Created collection with schema validation");
```

### 2. Create Indexes

```javascript
// Single field index
db.ClinicalDocuments.createIndex({ patientId: 1 });

// Compound index
db.ClinicalDocuments.createIndex({ patientId: 1, createdAt: -1 });

// Text index
db.ClinicalDocuments.createIndex({ content: "text", "tags": "text" });

// Unique index
db.PatientPreferences.createIndex({ patientId: 1 }, { unique: true });

// TTL index (auto-delete after 30 days)
db.AuditLog.createIndex({ createdAt: 1 }, { expireAfterSeconds: 2592000 });
```

### 3. Add Fields to All Documents

```javascript
// Idempotent field addition
db.ClinicalDocuments.updateMany(
    {},
    { $set: { version: 1, lastReviewedAt: null } },
    { upsert: false }
);

print(`Updated ${db.ClinicalDocuments.countDocuments({})} documents`);
```

### 4. Rename Fields

```javascript
// Rename field across all documents
db.ClinicalDocuments.updateMany(
    {},
    { $rename: { "oldFieldName": "newFieldName" } }
);
```

### 5. Convert Data Types

```javascript
// Convert string to ObjectId
db.ClinicalDocuments.updateMany(
    { departmentId: { $type: "string" } },
    [
        {
            $set: {
                departmentId: { $toObjectId: "$departmentId" }
            }
        }
    ]
);
```

### 6. Bulk Operations

```javascript
db.ClinicalDocuments.bulkWrite([
    {
        updateMany: {
            filter: { status: "draft" },
            update: { $set: { status: "inactive" } }
        }
    },
    {
        insertOne: {
            document: {
                migrationAudit: true,
                timestamp: new Date()
            }
        }
    }
]);
```

### 7. Transactions (Multiple Collections)

```javascript
var session = db.getMongo().startSession();

try {
    session.startTransaction();

    db.ClinicalDocuments.updateMany(
        { status: "pending" },
        { $set: { status: "processing" } }
    );

    db.AuditLog.insertOne({
        action: "migration",
        timestamp: new Date()
    });

    session.commitTransaction();
} catch (error) {
    session.abortTransaction();
    throw error;
} finally {
    session.endSession();
}
```

---

## Creating a New Migration

### Step 1: Copy Template

```bash
cp backend/db/migrations/mongo/00_MIGRATION_TEMPLATE.js \
   backend/db/migrations/mongo/YYYYMMDD_NNN_description.js
```

### Step 2: Edit with Your Changes

```javascript
/**
 * ═══════════════════════════════════════════════════════════════════════════
 * EHR Platform - MongoDB Migration
 * Version: 20250120_001
 * Description: Add clinical document search indexes
 * ═══════════════════════════════════════════════════════════════════════════
 */

// Your migration code here
db.ClinicalDocuments.createIndex({ patientId: 1, createdAt: -1 });

// Track migration
db.__MigrationHistory.insertOne({
    migrationId: "20250120_001_add-clinical-indexes",
    appliedAt: new Date(),
    productVersion: "1.0.0"
});

print("✅ Migration complete");
```

### Step 3: Test Locally

```bash
# Connect to local MongoDB
mongosh "mongodb://localhost:27017/ehr_patient_dev"

# Run migration
load("backend/db/migrations/mongo/20250120_001_add-clinical-indexes.js")

# Verify
db.__MigrationHistory.find({})
db.ClinicalDocuments.getIndexes()
```

### Step 4: Create Rollback Script

```javascript
// backend/db/migrations/mongo/20250120_001_add-clinical-indexes_rollback.js

// Drop indexes
db.ClinicalDocuments.dropIndex("patientId_1_createdAt_-1");

// Remove from history
db.__MigrationHistory.deleteOne({ migrationId: "20250120_001_add-clinical-indexes" });

print("✅ Rollback complete");
```

### Step 5: Commit

```bash
git add backend/db/migrations/mongo/20250120_001_add-clinical-indexes.js
git add backend/db/migrations/mongo/20250120_001_add-clinical-indexes_rollback.js
git commit -m "db(mongo): Add clinical document search indexes (20250120_001)"
```

---

## Polyglot Database Strategy

### SQL (PostgreSQL) vs MongoDB Decision

| Data Type | Database | Reason |
|---|---|---|
| **Relational Data** | PostgreSQL | Appointments, Invoices, Users (ACID, joins) |
| **Patient Vitals** | PostgreSQL | High frequency, time-series (optimized queries) |
| **Clinical Documents** | MongoDB | Flexible schema, text content, nested metadata |
| **Audit Logs** | PostgreSQL | Compliance, immutability requirements |
| **User Preferences** | MongoDB | Dynamic schema, quick reads |
| **Search Index** | Elasticsearch | Full-text search, analytics |
| **Cache Layer** | Redis | Session, frequently accessed data |

---

## Best Practices

### ✅ DO

- [ ] Use idempotent operations (safe to run twice)
- [ ] Create indexes before heavy queries
- [ ] Use bulk operations for large updates
- [ ] Include upsert: false for safety
- [ ] Test on development first
- [ ] Document rollback procedures
- [ ] Use transactions for multi-collection operations
- [ ] Clean up temporary collections

### ❌ DON'T

- [ ] Use forEach loops (use bulkWrite instead)
- [ ] Drop collections without backup
- [ ] Modify __MigrationHistory manually
- [ ] Run migrations without testing
- [ ] Use find().forEach() for large datasets
- [ ] Create excessive indexes (maintenance overhead)

---

## Migration Tracking

### View Applied Migrations

```javascript
db.__MigrationHistory.find({}).sort({ appliedAt: -1 })
```

### View Current Schema

```javascript
db.ClinicalDocuments.getIndexes()
db.ClinicalDocuments.validate()
```

### Get Collection Statistics

```javascript
db.ClinicalDocuments.stats()
```

---

## Environment-Specific Behavior

| Environment | Strategy | Behavior |
|---|---|---|
| **Development** | Auto-run | Migrations apply automatically in Program.cs |
| **Staging** | Manual | Run via migration script before deployment |
| **Production** | Manual approval | DBA reviews and runs after approval |

---

## Troubleshooting

### Migration Already Applied

```javascript
// Check if migration exists
db.__MigrationHistory.findOne({ migrationId: "20250120_001" })

// If exists but failed, delete and re-run
db.__MigrationHistory.deleteOne({ migrationId: "20250120_001" })
```

### Index Creation Failed

```javascript
// List all indexes
db.ClinicalDocuments.getIndexes()

// Drop problematic index
db.ClinicalDocuments.dropIndex("indexName")

// Recreate index
db.ClinicalDocuments.createIndex({ patientId: 1 })
```

### Collection Validation Error

```javascript
// Temporarily disable validation
db.runCommand({
    collMod: "ClinicalDocuments",
    validator: {},
    validationLevel: "off"
})

// Fix data
db.ClinicalDocuments.updateMany({}, { $set: { missingField: null } })

// Re-enable validation
db.runCommand({
    collMod: "ClinicalDocuments",
    validator: { /* schema here */ },
    validationLevel: "strict"
})
```

---

## Performance Considerations

### Index Strategy

```javascript
// Good: Compound index matching query patterns
db.ClinicalDocuments.createIndex({ patientId: 1, createdAt: -1 });

// Query: db.ClinicalDocuments.find({ patientId: "...", createdAt: { $gte: ... } })
```

### Bulk Operations

```javascript
// ❌ SLOW: 1000 separate updates
for (let i = 0; i < 1000; i++) {
    db.ClinicalDocuments.updateOne(...);
}

// ✅ FAST: Single bulkWrite
db.ClinicalDocuments.bulkWrite([...]);
```

---

## Contact & Support

- **Development Issues:** Post in #database-dev Slack
- **Staging Issues:** Contact QA lead
- **Production Issues:** Page DBA on-call

---

**Last Updated:** 2025-01-15  
**Version:** 1.0.0
