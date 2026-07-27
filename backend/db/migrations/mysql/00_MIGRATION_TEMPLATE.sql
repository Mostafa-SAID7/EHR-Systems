-- ═══════════════════════════════════════════════════════════════════════════════
-- EHR Platform - MySQL Migration Template
-- Version: YYYYMMDD_NNN
-- Created: YYYY-MM-DD
-- Description: [Brief description of changes]
-- Author: [Your Name]
-- Database: MySQL 5.7+ / MySQL 8.0+
-- ═══════════════════════════════════════════════════════════════════════════════
-- 
-- MIGRATION GUIDELINES:
-- 1. Version Format: YYYYMMDD_NNN (e.g., 20250115_001)
-- 2. NNN: Sequential number (001, 002, 003, etc. per day)
-- 3. Use IF NOT EXISTS / IF EXISTS clauses for idempotency
-- 4. Always specify ENGINE=InnoDB for transactional safety
-- 5. Use utf8mb4 for Unicode support (HIPAA compliance)
-- 6. Create indexes immediately after table creation
-- 7. Use ALTER TABLE for non-destructive changes
-- 8. Include rollback information at the end
-- 9. Test on MySQL 5.7 and 8.0 compatibility
-- ═══════════════════════════════════════════════════════════════════════════════

-- ─────────────────────────────────────────────────────────────────────────────
-- TABLE CREATION EXAMPLE
-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS `Patients` (
    `Id` CHAR(36) NOT NULL PRIMARY KEY COMMENT 'UUID',
    `FirstName` VARCHAR(100) NOT NULL,
    `LastName` VARCHAR(100) NOT NULL,
    `DateOfBirth` DATE NOT NULL,
    `Email` VARCHAR(255),
    `PhoneNumber` VARCHAR(20),
    `MedicalRecordNumber` VARCHAR(50) UNIQUE COMMENT 'MRN - Unique patient identifier',
    `Status` INT NOT NULL DEFAULT 0 COMMENT '0=Active, 1=Inactive, 2=Archived',
    `CreatedAt` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    `IsDeleted` BOOLEAN DEFAULT FALSE,
    
    -- Indexes for common queries
    KEY `idx_mrn` (`MedicalRecordNumber`),
    KEY `idx_email` (`Email`),
    KEY `idx_status_created` (`Status`, `CreatedAt`),
    KEY `idx_last_name_first_name` (`LastName`, `FirstName`),
    
    -- Constraints
    CONSTRAINT `chk_email_format` CHECK (
        `Email` IS NULL OR `Email` LIKE '%@%'
    ),
    
    ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
) COMMENT='Patient master data - core entity for EHR';

-- ─────────────────────────────────────────────────────────────────────────────
-- COLUMN ADDITION EXAMPLE
-- ─────────────────────────────────────────────────────────────────────────────

-- Add new column (safe operation)
ALTER TABLE `Patients`
ADD COLUMN IF NOT EXISTS `MiddleName` VARCHAR(100) COMMENT 'Optional middle name',
ADD COLUMN IF NOT EXISTS `PreferredName` VARCHAR(100) COMMENT 'Name patient prefers to be called';

-- Add column with default value
ALTER TABLE `Patients`
ADD COLUMN IF NOT EXISTS `MaritalStatus` VARCHAR(20) DEFAULT 'Unknown';

-- ─────────────────────────────────────────────────────────────────────────────
-- INDEX MANAGEMENT EXAMPLE
-- ─────────────────────────────────────────────────────────────────────────────

-- Create single-column index
ALTER TABLE `Patients` ADD INDEX IF NOT EXISTS `idx_status` (`Status`);

-- Create composite index (for WHERE clause with multiple columns)
ALTER TABLE `Patients` 
ADD INDEX IF NOT EXISTS `idx_search` (`LastName`, `FirstName`, `DateOfBirth`);

-- Create FULLTEXT index (for search)
ALTER TABLE `Patients`
ADD FULLTEXT INDEX IF NOT EXISTS `ft_names` (`FirstName`, `LastName`);

-- Drop index (careful operation)
-- ALTER TABLE `Patients` DROP INDEX `idx_old_index`;

-- ─────────────────────────────────────────────────────────────────────────────
-- DATA TRANSFORMATION EXAMPLE
-- ─────────────────────────────────────────────────────────────────────────────

-- Update records (idempotent with WHERE clause)
UPDATE `Patients`
SET `Status` = 2
WHERE `Status` = 0 AND `UpdatedAt` < DATE_SUB(NOW(), INTERVAL 2 YEAR)
  AND NOT EXISTS (
      SELECT 1 FROM `Appointments`
      WHERE `PatientId` = `Patients`.`Id`
        AND `ScheduledStart` > DATE_SUB(NOW(), INTERVAL 1 YEAR)
  );

-- Populate new column from existing data
UPDATE `Patients`
SET `PreferredName` = COALESCE(`MiddleName`, `FirstName`)
WHERE `PreferredName` IS NULL;

-- ─────────────────────────────────────────────────────────────────────────────
-- FOREIGN KEY MANAGEMENT EXAMPLE
-- ─────────────────────────────────────────────────────────────────────────────

-- Add foreign key
ALTER TABLE `Appointments`
ADD CONSTRAINT IF NOT EXISTS `fk_appointments_patients`
FOREIGN KEY (`PatientId`) REFERENCES `Patients`(`Id`)
ON DELETE RESTRICT
ON UPDATE CASCADE;

-- Add composite foreign key
ALTER TABLE `AppointmentNotes`
ADD CONSTRAINT IF NOT EXISTS `fk_appointment_notes_composite`
FOREIGN KEY (`AppointmentId`, `PatientId`) REFERENCES `Appointments`(`Id`, `PatientId`)
ON DELETE CASCADE
ON UPDATE CASCADE;

-- ─────────────────────────────────────────────────────────────────────────────
-- VIEW CREATION EXAMPLE
-- ─────────────────────────────────────────────────────────────────────────────

-- Create view for commonly used queries
CREATE OR REPLACE VIEW `v_active_patients` AS
SELECT 
    `Id`,
    `FirstName`,
    `LastName`,
    `Email`,
    `MedicalRecordNumber`,
    `CreatedAt`,
    COUNT(DISTINCT `Appointments`.`Id`) AS `AppointmentCount`
FROM `Patients`
LEFT JOIN `Appointments` ON `Patients`.`Id` = `Appointments`.`PatientId`
WHERE `Patients`.`Status` = 0
GROUP BY `Patients`.`Id`;

-- ─────────────────────────────────────────────────────────────────────────────
-- MIGRATION TRACKING
-- ─────────────────────────────────────────────────────────────────────────────

-- Record this migration
INSERT INTO `__MigrationHistory` (`MigrationId`, `ProductVersion`)
VALUES ('YYYYMMDD_NNN_description', '1.0.0')
ON DUPLICATE KEY UPDATE `AppliedAt` = CURRENT_TIMESTAMP;

-- ═════════════════════════════════════════════════════════════════════════════
-- ROLLBACK PROCEDURE
-- ═════════════════════════════════════════════════════════════════════════════
-- In case of emergency, manually execute the reverse operations:
--
-- DROP TABLE IF EXISTS `NewTable`;
-- ALTER TABLE `ExistingTable` DROP COLUMN IF EXISTS `NewColumn`;
-- ALTER TABLE `ExistingTable` DROP INDEX IF EXISTS `idx_new_index`;
-- DROP VIEW IF EXISTS `v_new_view`;
-- DELETE FROM `__MigrationHistory` WHERE `MigrationId` = 'YYYYMMDD_NNN_description';
--
-- Then delete the migration file from db/migrations/mysql/
-- ═════════════════════════════════════════════════════════════════════════════

-- Set session variable to track execution (for debugging)
SET @migration_executed = TRUE;
SELECT 'Migration completed' AS Status;
