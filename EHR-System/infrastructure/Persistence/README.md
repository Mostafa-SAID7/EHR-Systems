# EHR Platform - Persistence & Database Configuration

Complete database initialization and configuration for PostgreSQL, MongoDB, and MySQL.

## Database Architecture Overview

EHR Platform uses a **polyglot persistence** approach with three database technologies for optimal service isolation and performance:

### Database Distribution by Service

| Service | Primary DB | Secondary DB | Purpose |
|---------|-----------|--------------|---------|
| **Identity** | PostgreSQL | - | Authentication, user accounts |
| **Patient** | PostgreSQL | MongoDB | Patient master data + flexible preferences |
| **Appointment** | PostgreSQL | MySQL | Relational scheduling + caching |
| **Clinical** | PostgreSQL | MongoDB | Clinical records + unstructured notes |
| **Integration** | PostgreSQL | MongoDB | External API logs + sync state |
| **Terminology** | PostgreSQL | MongoDB | Medical code mappings + hierarchy |
| **FileStorage** | PostgreSQL | MongoDB | Document metadata + versioning |
| **AI** | PostgreSQL | MongoDB | Predictions + training data cache |
| **Notification** | PostgreSQL | MySQL + MongoDB | Queue + templates + history |
| **Billing** | PostgreSQL | MySQL | Invoicing + payment reporting |
| **Audit** | PostgreSQL | MongoDB | Immutable audit logs (7-year retention) |

## PostgreSQL Configuration

### Setup

```bash
# Initialize PostgreSQL
docker-compose up -d postgres

# Create databases
docker-compose exec postgres psql -U ehruser -f /init-scripts/postgres-init.sql

# Verify
docker-compose exec postgres psql -U ehruser -l
```

### Database Schema

Each service gets its own PostgreSQL database with schema isolation:

```sql
-- Per-service databases
ehr_identity          -- Identity Service
ehr_patient          -- Patient Service
ehr_appointment      -- Appointment Service
ehr_integration      -- Integration Service
ehr_terminology      -- Terminology Service
ehr_filestorage      -- FileStorage Service
ehr_ai               -- AI Service

-- Per-service schemas within each database
identity.*           -- Identity tables
patient.*            -- Patient tables
appointment.*        -- Appointment tables
integration.*        -- Integration tables
terminology.*        -- Terminology tables
filestorage.*        -- FileStorage tables
ai.*                 -- AI tables
```

### Key Features

✅ **Schema-per-service isolation** - No cross-service foreign keys  
✅ **Automatic failover** - Master-replica replication  
✅ **Point-in-time recovery** - Continuous backups  
✅ **Connection pooling** - Via PgBouncer (max 100 connections)  
✅ **Query optimization** - Indexes on foreign keys and search fields  

### Configuration Examples

```yaml
# Connection String
postgresql://ehruser:password@postgres:5432/ehr_patient

# EF Core (C# in microservices)
services.AddDbContext<PatientDbContext>(options =>
  options.UseNpgsql(connectionString,
    x => x.MigrationsHistoryTable("_EFMigrationsHistory", "patient"))
);

# Connection Pool
services.AddNpgsqlDataSource(connectionString,
  builder => builder.MaxPoolSize(20).MinPoolSize(5)
);
```

### Maintenance Commands

```bash
# Backup specific database
pg_dump -U ehruser -d ehr_patient > patient_backup.sql

# Restore database
psql -U ehruser -d ehr_patient < patient_backup.sql

# View active connections
SELECT datname, count(*) FROM pg_stat_activity GROUP BY datname;

# Kill long-running queries
SELECT pg_terminate_backend(pid) FROM pg_stat_activity 
WHERE duration > interval '5 minutes';

# Analyze query performance
EXPLAIN ANALYZE SELECT * FROM patient.patients WHERE id = '123';

# Check table size
SELECT tablename, pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename))
FROM pg_tables WHERE schemaname = 'patient' ORDER BY pg_total_relation_size DESC;
```

## MongoDB Configuration

### Setup

```bash
# Start MongoDB
docker-compose up -d mongo

# Initialize databases (runs mongo-init-services.js)
docker-compose exec mongo mongosh < /init-scripts/mongo-init-services.js

# Verify
docker-compose exec mongo mongosh --eval "db.adminCommand('listDatabases')"
```

### Database Mapping

Each service gets dedicated MongoDB databases for document storage:

```javascript
ehr_patient_documents        // Patient preferences, flexible data
ehr_clinical_documents       // Clinical notes, progress notes
ehr_appointment_documents    // Appointment history, visit summaries
ehr_integration_documents    // HL7/FHIR logs, sync state
ehr_terminology_documents    // Medical codes, mappings, hierarchy
ehr_filestorage_documents    // Document metadata, versions
ehr_ai_documents             // Predictions, training data cache
ehr_notification_documents   // Notification queue, templates
ehr_audit_documents          // Audit logs (7-year retention via TTL)
ehr_prescription_documents   // Medication history
ehr_outbox_documents         // Transactional outbox events
```

### Collections & Indexes

**Patient Service Example:**
```javascript
db.PatientPreferences
  ├─ Index: { patientId: 1 } [unique]
  ├─ Index: { createdAt: -1 }
  └─ Validator: JSON Schema for PII protection

db.ClinicalDocuments
  ├─ Index: { patientId: 1 }
  ├─ Index: { patientId: 1, documentType: 1, createdAt: -1 }
  ├─ Index: { content: "text" } [full-text search]
  ├─ Index: { providerId: 1, createdAt: -1 }
  └─ Validator: JSON Schema for compliance

db.AuditLogs
  ├─ Index: { timestamp: 1 } [TTL: 7 years]
  ├─ Index: { userId: 1, timestamp: -1 }
  ├─ Index: { resourceType: 1, resourceId: 1 }
  ├─ Index: { action: 1, timestamp: -1 }
  └─ Immutable storage for HIPAA compliance
```

### Key Features

✅ **Flexible schema** - No migrations needed for document changes  
✅ **Full-text search** - Indexed text fields for clinical content  
✅ **TTL Indexes** - Auto-delete notifications (90 days) and audit logs (7 years)  
✅ **Schema validation** - JSON Schema enforcement for critical collections  
✅ **Horizontal scaling** - Sharding support for large collections  

### Configuration Examples

```csharp
// C# MongoDB Driver (Microservices)
var client = new MongoClient("mongodb://localhost:27017");
var database = client.GetDatabase("ehr_patient_documents");
var collection = database.GetCollection<PatientPreference>("PatientPreferences");

// Connection string
mongodb://mongo:27017/?retryWrites=true&w=majority
```

### Maintenance Commands

```bash
# Connect to MongoDB
mongosh --host mongo --port 27017

# List all databases
db.adminCommand('listDatabases')

# View collections in a database
use ehr_patient_documents
show collections

# Check index details
db.PatientPreferences.getIndexes()

# Reindex collection
db.ClinicalDocuments.reIndex()

# Check disk usage
db.stats()

# View TTL indexes
db.ClinicalDocuments.getIndexes() // Check expireAfterSeconds

# Manually trigger TTL deletion
db.NotificationQueue.deleteMany({ 
  createdAt: { $lt: new Date(Date.now() - 7776000000) } 
})

# Validate collection
db.ClinicalDocuments.validate()

# Backup database
mongodump --uri "mongodb://mongo:27017" --db ehr_patient_documents --out backup/

# Restore database
mongorestore --uri "mongodb://mongo:27017" --nsInclude "ehr_patient_documents.*" backup/
```

## MySQL Configuration

### Setup

```bash
# Start MySQL
docker-compose up -d mysql

# Initialize databases
docker-compose exec mysql mysql -u root -p$MYSQL_ROOT_PASSWORD < /init-scripts/mysql-init.sql

# Verify
docker-compose exec mysql mysql -u ehruser -e "SHOW DATABASES LIKE 'ehr_%';"
```

### Database Mapping

MySQL hosts high-performance, relational workloads:

```sql
ehr_appointment_mysql      // Appointment slots, notifications, metrics
ehr_billing_mysql          // Invoices, payments, billing reports
ehr_notification_mysql     // Notification preferences, delivery logs
ehr_analytics_mysql        // Visit analytics, clinical metrics, system performance
```

### Tables & Indexes

**Appointment Database:**
```sql
appointment_slots
  ├─ Primary Key: id
  ├─ Unique: { provider_id, appointment_date, start_time }
  ├─ Index: { provider_id, appointment_date }
  ├─ Index: { status, appointment_date }
  └─ Columns: provider_id, date, time, status, type, created_at

appointment_notifications
  ├─ Primary Key: id
  ├─ Index: { appointment_id }
  ├─ Index: { patient_id }
  ├─ Index: { status }
  └─ Columns: appointment_id, patient_id, type, status, retry_count

appointment_metrics
  ├─ Primary Key: id
  ├─ Unique: { provider_id, appointment_date }
  └─ Columns: total, completed, cancelled, no_show, avg_duration
```

### Key Features

✅ **Denormalized schema** - Pre-aggregated analytics tables  
✅ **Optimized indexes** - For high-throughput reads  
✅ **Views** - Common analytical queries pre-built  
✅ **Partitioning** - Date-based partitioning for time-series data  
✅ **Slow query log** - Built-in performance monitoring  

### Configuration Examples

```csharp
// C# EF Core with MySQL
services.AddDbContext<BillingDbContext>(options =>
  options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

// Connection String
Server=mysql;Port=3306;Database=ehr_billing_mysql;Uid=ehruser;Pwd=password;
```

### Maintenance Commands

```bash
# Connect to MySQL
mysql -h mysql -u ehruser -p

# Show databases
SHOW DATABASES LIKE 'ehr_%';

# Show tables
USE ehr_billing_mysql;
SHOW TABLES;

# Check table sizes
SELECT 
  TABLE_NAME,
  ROUND(((data_length + index_length) / 1024 / 1024), 2) as size_mb
FROM information_schema.TABLES
WHERE TABLE_SCHEMA = 'ehr_billing_mysql'
ORDER BY data_length DESC;

# Optimize table
OPTIMIZE TABLE invoice_items;

# Check index usage
SELECT * FROM sys.schema_unused_indexes;

# Enable slow query log
SET GLOBAL slow_query_log = 'ON';
SET GLOBAL long_query_time = 1;

# View slow queries
SELECT * FROM mysql.slow_log;

# Backup database
mysqldump -u ehruser -p ehr_billing_mysql > billing_backup.sql

# Restore database
mysql -u ehruser -p ehr_billing_mysql < billing_backup.sql
```

## Database Selection Guidelines

**Use PostgreSQL when:**
- Relational data with foreign keys
- ACID transactions required
- Schema is well-defined
- Strong consistency needed

**Use MongoDB when:**
- Flexible/semi-structured data
- Document-oriented storage
- Nested objects with varying structures
- High write throughput
- TTL auto-expiration needed

**Use MySQL when:**
- High-speed analytical queries
- Denormalized reporting tables
- Time-series data with partitioning
- Web-scale read-heavy operations

## Backup & Recovery Strategy

### Automated Backups

```bash
# PostgreSQL: Daily full backup
0 2 * * * pg_dump -U ehruser -Fc ehr_patient > /backups/ehr_patient_$(date +\%Y\%m\%d).dump

# MongoDB: Daily snapshot
0 2 * * * mongodump --uri "mongodb://localhost:27017" --out /backups/mongo_$(date +\%Y\%m\%d)

# MySQL: Daily backup
0 2 * * * mysqldump -u ehruser -p'$PASS' --all-databases > /backups/mysql_$(date +\%Y\%m\%d).sql
```

### Disaster Recovery

```bash
# Restore PostgreSQL
pg_restore -U ehruser -d ehr_patient /backups/ehr_patient_20250101.dump

# Restore MongoDB
mongorestore --uri "mongodb://localhost:27017" /backups/mongo_20250101/

# Restore MySQL
mysql -u ehruser -p < /backups/mysql_20250101.sql
```

## Monitoring & Alerts

### PostgreSQL Metrics
- Connection pool utilization
- Query execution time (P95, P99)
- Replication lag
- Disk usage

### MongoDB Metrics
- Operation latency
- Document count per collection
- Index usage
- TTL expiration success rate

### MySQL Metrics
- Query throughput (QPS)
- Slow query count
- Table lock waits
- Disk I/O

## Security Best Practices

1. **Never commit passwords** - Use environment variables
2. **Schema isolation** - Services cannot access other schemas
3. **User permissions** - Least privilege per service
4. **Encryption at rest** - Enable in production
5. **Encryption in transit** - SSL for all connections
6. **Audit logging** - Track all data modifications
7. **PII protection** - Mask sensitive data in logs

## Troubleshooting

### PostgreSQL Connection Issues
```bash
# Test connection
psql -h postgres -U ehruser -d ehr_patient -c "SELECT 1;"

# Check connection pool
SELECT datname, count(*) FROM pg_stat_activity GROUP BY datname;
```

### MongoDB Connection Issues
```bash
# Test connection
mongosh --eval "db.adminCommand('ping')"

# Check replication status
db.isMaster()
```

### MySQL Connection Issues
```bash
# Test connection
mysql -h mysql -u ehruser -e "SELECT 1;"

# Check connections
SHOW PROCESSLIST;
```

## References

- PostgreSQL Docs: https://www.postgresql.org/docs/
- MongoDB Docs: https://docs.mongodb.com/
- MySQL Docs: https://dev.mysql.com/doc/
- Database Per Service Pattern: https://microservices.io/patterns/data/database-per-service.html
