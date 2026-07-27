# MySQL Migrations - EHR Platform

## Overview

Complete MySQL migration system with **full ACID compliance, InnoDB transactional safety, and HIPAA-grade audit trails**.

**Location:** `backend/db/migrations/mysql/`  
**Executor:** `MySqlMigrationExecutor.cs` in EHRPlatform.Common  
**Tracking:** `__MigrationHistory` table  
**Engine:** InnoDB (transactional, crash-safe, foreign key support)  
**Charset:** utf8mb4 (Unicode support for international HIPAA compliance)  

---

## Why MySQL for EHR?

| Feature | MySQL | PostgreSQL | Choice |
|---|---|---|---|
| ACID Compliance | ✅ InnoDB | ✅ Native | **MySQL** (proven, healthcare-standard) |
| Transactional Safety | ✅ Full | ✅ Full | **Tie** |
| Foreign Keys | ✅ InnoDB | ✅ Native | **Tie** |
| JSON Support | ✅ Native | ✅ Native | **Tie** |
| Replication | ✅ Native | ✅ Streaming | **MySQL** (simpler ops) |
| Compliance Auditing | ✅ Excellent | ✅ Good | **MySQL** (healthcare standard) |
| Full-text Search | ✅ Built-in | ✅ Built-in | **PostgreSQL** (use Elasticsearch anyway) |
| Availability | ✅ Simple | ✅ Complex | **MySQL** (easier HA setup) |

**Decision:** MySQL for relational data, PostgreSQL for analytics, MongoDB for documents, Elasticsearch for search.

---

## Version Format

```
YYYYMMDD_NNN_description.sql

YYYYMMDD  = Date (e.g., 20250115 = January 15, 2025)
NNN       = Sequential number (001, 002, 003 per day)
description = Kebab-case description
```

**Example:** `20250115_001_add-patient-indexes.sql`

---

## Running MySQL Migrations

### From .NET Code (Recommended)

```csharp
// In Program.cs
var connectionString = builder.Configuration.GetConnectionString("MySQL");

builder.Services.AddMySqlMigrations(connectionString);

// After building app:
await app.Services.RunMySqlMigrationAsync(
    "20250115_001_add-patient-indexes",
    async (connection, transaction) =>
    {
        var command = new MySqlCommand(
            @"ALTER TABLE `Patients` 
              ADD INDEX IF NOT EXISTS `idx_status` (`Status`)",
            connection, transaction);
        
        return await command.ExecuteNonQueryAsync();
    }
);
```

### From MySQL CLI

```bash
# Connect to MySQL
mysql -h localhost -u root -p ehr_platform

# Execute migration file
SOURCE backend/db/migrations/mysql/20250115_001_add-patient-indexes.sql;

# Verify
SELECT * FROM `__MigrationHistory` ORDER BY `AppliedAt` DESC;
```

### From Docker

```bash
docker exec -i ehr-mysql mysql -u root -p$MYSQL_ROOT_PASSWORD ehr_platform < \
    backend/db/migrations/mysql/20250115_001_add-patient-indexes.sql
```

### In docker-compose

```yaml
services:
  mysql:
    image: mysql:8.0
    volumes:
      - ./backend/db/migrations/mysql/20250101_001_baseline.sql:/docker-entrypoint-initdb.d/001_baseline.sql
    environment:
      MYSQL_ROOT_PASSWORD: password
      MYSQL_DATABASE: ehr_platform
```

---

## Migration Examples

### 1. Create Table with Constraints

```sql
CREATE TABLE IF NOT EXISTS `Patients` (
    `Id` CHAR(36) NOT NULL PRIMARY KEY COMMENT 'UUID',
    `FirstName` VARCHAR(100) NOT NULL,
    `LastName` VARCHAR(100) NOT NULL,
    `Email` VARCHAR(255),
    `MedicalRecordNumber` VARCHAR(50) UNIQUE NOT NULL,
    `Status` INT NOT NULL DEFAULT 0,
    `CreatedAt` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    `IsDeleted` BOOLEAN DEFAULT FALSE,
    
    KEY `idx_email` (`Email`),
    KEY `idx_status` (`Status`),
    
    ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
) COMMENT='Patient master data';
```

### 2. Add Column Safely

```sql
-- Idempotent: safe to run twice
ALTER TABLE `Patients`
ADD COLUMN IF NOT EXISTS `MiddleName` VARCHAR(100),
ADD COLUMN IF NOT EXISTS `PreferredLanguage` VARCHAR(10) DEFAULT 'en-US';
```

### 3. Create Indexes

```sql
-- Single-column index
ALTER TABLE `Patients` ADD INDEX IF NOT EXISTS `idx_mrn` (`MedicalRecordNumber`);

-- Composite index (query optimization)
ALTER TABLE `Patients` 
ADD INDEX IF NOT EXISTS `idx_search` (`LastName`, `FirstName`, `DateOfBirth`);

-- FULLTEXT index (text search)
ALTER TABLE `ClinicalNotes`
ADD FULLTEXT INDEX IF NOT EXISTS `ft_notes` (`NoteContent`);

-- Unique index (constraint)
ALTER TABLE `Users`
ADD UNIQUE INDEX IF NOT EXISTS `ux_email` (`Email`);

-- Prefix index (for long columns)
ALTER TABLE `Patients`
ADD INDEX IF NOT EXISTS `idx_notes_prefix` (`Notes`(100));
```

### 4. Add Foreign Keys

```sql
-- Simple foreign key
ALTER TABLE `Appointments`
ADD CONSTRAINT IF NOT EXISTS `fk_appointments_patients`
FOREIGN KEY (`PatientId`) REFERENCES `Patients`(`Id`)
ON DELETE RESTRICT ON UPDATE CASCADE;

-- Composite foreign key
ALTER TABLE `AppointmentNotes`
ADD CONSTRAINT IF NOT EXISTS `fk_notes_appointments`
FOREIGN KEY (`AppointmentId`, `PatientId`) 
REFERENCES `Appointments`(`Id`, `PatientId`)
ON DELETE CASCADE ON UPDATE CASCADE;
```

### 5. Add Check Constraints (MySQL 8.0.16+)

```sql
ALTER TABLE `Invoices`
ADD CONSTRAINT `chk_amount_positive` CHECK (`Amount` > 0),
ADD CONSTRAINT `chk_due_after_created` CHECK (`DueDate` >= DATE(`CreatedAt`));
```

### 6. Data Migration

```sql
-- Update with condition (idempotent)
UPDATE `Patients`
SET `PreferredLanguage` = 'en-US'
WHERE `PreferredLanguage` IS NULL
  AND `CreatedAt` > DATE_SUB(NOW(), INTERVAL 1 MONTH);

-- Populate computed values
UPDATE `Users`
SET `FullName` = CONCAT(`FirstName`, ' ', `LastName`)
WHERE `FullName` IS NULL;
```

### 7. Rename Column

```sql
ALTER TABLE `Patients`
CHANGE COLUMN `MRN` `MedicalRecordNumber` VARCHAR(50);
```

### 8. Modify Column Type

```sql
-- Expand column size (safe)
ALTER TABLE `AuditEntries` 
MODIFY COLUMN `Description` TEXT;

-- Change data type (risky - validate first)
ALTER TABLE `Invoices`
MODIFY COLUMN `Amount` DECIMAL(19,4);
```

### 9. Create View

```sql
-- View for frequently used queries
CREATE OR REPLACE VIEW `v_active_patients_with_appointments` AS
SELECT 
    p.`Id`,
    p.`FirstName`,
    p.`LastName`,
    p.`Email`,
    COUNT(a.`Id`) AS `AppointmentCount`,
    MAX(a.`ScheduledStart`) AS `LatestAppointment`
FROM `Patients` p
LEFT JOIN `Appointments` a ON p.`Id` = a.`PatientId`
WHERE p.`Status` = 0 AND p.`IsDeleted` = FALSE
GROUP BY p.`Id`;
```

### 10. Add JSON Column

```sql
ALTER TABLE `Patients`
ADD COLUMN `Metadata` JSON COMMENT 'Flexible schema for additional patient data';

-- Update with JSON data
UPDATE `Patients`
SET `Metadata` = JSON_OBJECT(
    'insurance_provider', 'BlueCross',
    'emergency_contact', 'John Doe',
    'allergies', JSON_ARRAY('Penicillin', 'Latex')
)
WHERE `Id` = 'patient-uuid';

-- Query JSON
SELECT * FROM `Patients`
WHERE JSON_EXTRACT(`Metadata`, '$.insurance_provider') = 'BlueCross';
```

---

## Creating a New Migration

### Step 1: Copy Template

```bash
cp backend/db/migrations/mysql/00_MIGRATION_TEMPLATE.sql \
   backend/db/migrations/mysql/20250120_001_add-patient-allergies.sql
```

### Step 2: Edit Migration

```sql
-- backend/db/migrations/mysql/20250120_001_add-patient-allergies.sql

CREATE TABLE IF NOT EXISTS `PatientAllergies` (
    `Id` CHAR(36) NOT NULL PRIMARY KEY,
    `PatientId` CHAR(36) NOT NULL,
    `AllergyName` VARCHAR(100) NOT NULL,
    `Severity` ENUM('Mild', 'Moderate', 'Severe') DEFAULT 'Moderate',
    `CreatedAt` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    KEY `idx_patient` (`PatientId`),
    
    CONSTRAINT `fk_allergies_patients` FOREIGN KEY (`PatientId`) 
        REFERENCES `Patients`(`Id`) ON DELETE CASCADE,
    
    ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
) COMMENT='Patient allergy tracking';

INSERT INTO `__MigrationHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20250120_001_add-patient-allergies', '1.0.0')
ON DUPLICATE KEY UPDATE `AppliedAt` = CURRENT_TIMESTAMP;
```

### Step 3: Test Locally

```bash
# Start local MySQL
docker-compose up -d mysql

# Apply migration
mysql -h localhost -u root -ppassword ehr_platform < \
    backend/db/migrations/mysql/20250120_001_add-patient-allergies.sql

# Verify
mysql -h localhost -u root -ppassword ehr_platform -e \
    "SELECT * FROM \`__MigrationHistory\` ORDER BY \`AppliedAt\` DESC LIMIT 1;"

mysql -h localhost -u root -ppassword ehr_platform -e \
    "DESCRIBE \`PatientAllergies\`;"
```

### Step 4: Create Rollback Script

```sql
-- backend/db/migrations/mysql/20250120_001_add-patient-allergies_rollback.sql

DROP TABLE IF EXISTS `PatientAllergies`;

DELETE FROM `__MigrationHistory` 
WHERE `MigrationId` = '20250120_001_add-patient-allergies';
```

### Step 5: Commit

```bash
git add backend/db/migrations/mysql/20250120_001_add-patient-allergies.sql
git add backend/db/migrations/mysql/20250120_001_add-patient-allergies_rollback.sql
git commit -m "db(mysql): Add patient allergies tracking (20250120_001)"
```

---

## Best Practices

### ✅ DO

- [ ] Use InnoDB engine for all tables (transactional safety)
- [ ] Use utf8mb4 charset (Unicode support)
- [ ] Test idempotency: run migrations twice
- [ ] Create indexes immediately after tables
- [ ] Use IF NOT EXISTS / IF EXISTS clauses
- [ ] Add meaningful comments to tables/columns
- [ ] Use TIMESTAMP for audit columns
- [ ] Use ENUM for fixed set values
- [ ] Use generated columns for computed fields (MySQL 8.0+)
- [ ] Create rollback scripts before deployment

### ❌ DON'T

- [ ] Use MyISAM engine (no transactions)
- [ ] Use latin1 charset (HIPAA compliance risk)
- [ ] Forget IF EXISTS clauses (breaks idempotency)
- [ ] Create unnecessary indexes (maintenance overhead)
- [ ] Use BLOB for text (use LONGTEXT instead)
- [ ] Skip foreign keys (data integrity)
- [ ] Use reserved keywords unescaped (always use backticks)
- [ ] Perform DDL operations inside transactions (not supported)

---

## MySQL 8.0+ Features

### Generated Columns

```sql
-- Computed column automatically maintained
ALTER TABLE `Patients` ADD COLUMN `FullName` VARCHAR(255)
GENERATED ALWAYS AS (CONCAT_WS(' ', `FirstName`, `LastName`)) STORED;

-- Or virtual (computed on read)
ADD COLUMN `DisplayName` VARCHAR(255)
GENERATED ALWAYS AS (COALESCE(`PreferredName`, CONCAT(`FirstName`, ' ', `LastName`))) VIRTUAL;
```

### Window Functions

```sql
-- Ranking for duplicate detection
SELECT `Email`, ROW_NUMBER() OVER (PARTITION BY `Email` ORDER BY `CreatedAt`) AS `DuplicateRank`
FROM `Patients`
WHERE ROW_NUMBER() OVER (PARTITION BY `Email` ORDER BY `CreatedAt`) > 1;
```

### JSON Functions

```sql
-- Enhanced JSON support
SELECT `Id`, JSON_PRETTY(`Metadata`) FROM `Patients`;

-- JSON validation
ALTER TABLE `Patients` ADD CHECK (JSON_VALID(`Metadata`));
```

---

## Performance Optimization

### Index Strategy

```sql
-- Query planner uses indexes most efficiently
EXPLAIN SELECT * FROM `Patients` WHERE `Email` = 'test@test.com' AND `Status` = 0;

-- Add composite index matching WHERE clause order
ALTER TABLE `Patients` ADD INDEX `idx_email_status` (`Email`, `Status`);
```

### Monitor Query Performance

```sql
-- Show slow queries (MySQL 5.7+)
SET GLOBAL slow_query_log = 'ON';
SET GLOBAL long_query_time = 2;

-- Analyze index usage
SELECT * FROM sys.statements_with_full_table_scans;
```

### Optimize Tables

```sql
-- Reclaim space and optimize index structure
OPTIMIZE TABLE `Patients`;

-- View table statistics
SELECT table_name, table_rows, avg_row_length
FROM information_schema.TABLES
WHERE table_schema = DATABASE();
```

---

## Troubleshooting

### Migration Already Applied

```sql
-- Check migration history
SELECT * FROM `__MigrationHistory` WHERE `MigrationId` = '20250120_001';

-- If corrupted, delete and re-run
DELETE FROM `__MigrationHistory` WHERE `MigrationId` = '20250120_001';
```

### Foreign Key Constraint Errors

```sql
-- Check constraints
SELECT * FROM information_schema.KEY_COLUMN_USAGE
WHERE TABLE_NAME = 'Appointments' AND CONSTRAINT_NAME LIKE 'fk_%';

-- Disable temporarily (dangerous!)
SET FOREIGN_KEY_CHECKS = 0;
-- ... fix data ...
SET FOREIGN_KEY_CHECKS = 1;
```

### Deadlock Issues

```sql
-- View current locks
SHOW PROCESSLIST;

-- Kill long-running query
KILL QUERY process_id;

-- Monitor innodb locks
SELECT * FROM INFORMATION_SCHEMA.INNODB_LOCKS;
```

### Table Not Found

```sql
-- List all tables
SHOW TABLES;

-- Check if table name is case-sensitive
SET GLOBAL lower_case_table_names = 0;
```

---

## Migration Checklist

### Pre-Deployment

- [ ] Migration file created with correct naming
- [ ] Tested on MySQL 5.7 AND 8.0 compatibility
- [ ] Rollback script created and tested
- [ ] Idempotency verified (run twice safely)
- [ ] Performance impact assessed
- [ ] Backup created
- [ ] Team notified

### Post-Deployment

- [ ] Migration applied successfully
- [ ] Data integrity verified
- [ ] Performance metrics normal
- [ ] No deadlocks or lock timeouts
- [ ] Replication lag acceptable (if using)
- [ ] Monitoring shows green

---

## CI/CD Integration

### GitHub Actions

```yaml
- name: Validate MySQL migrations
  run: |
    for file in backend/db/migrations/mysql/*.sql; do
      mysql-shell --file "$file" --validate
    done

- name: Apply to test database
  run: |
    mysql -h test-mysql -u root -ppassword ehr_platform < \
      backend/db/migrations/mysql/20250120_001_add-patient-allergies.sql
```

---

## Contact & Support

- **MySQL Issues:** #database-mysql Slack
- **Performance Issues:** #database-performance Slack
- **Emergency Issues:** Page DBA on-call

---

**Last Updated:** 2025-01-15  
**Version:** 1.0.0  
**Compatible:** MySQL 5.7, MySQL 8.0, MariaDB 10.5+
