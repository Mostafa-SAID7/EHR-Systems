-- ═══════════════════════════════════════════════════════════════════════════════
-- EHR Platform - MySQL Baseline Migration
-- Version: 20250101_001
-- Created: 2025-01-01
-- Description: Create base tables and indexes for all microservices
-- Database: MySQL 5.7+ / MySQL 8.0+
-- Engine: InnoDB with full ACID compliance
-- ═══════════════════════════════════════════════════════════════════════════════

-- ─────────────────────────────────────────────────────────────────────────────
-- MIGRATION TRACKING TABLE
-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS `__MigrationHistory` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `MigrationId` VARCHAR(255) NOT NULL UNIQUE,
    `ProductVersion` VARCHAR(50) NOT NULL,
    `AppliedAt` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    INDEX `idx_migrationid` (`MigrationId`),
    INDEX `idx_appliedat` (`AppliedAt`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Migration history tracking for all database changes';

-- ─────────────────────────────────────────────────────────────────────────────
-- PATIENT SERVICE TABLES
-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS `Patients` (
    `Id` CHAR(36) NOT NULL PRIMARY KEY COMMENT 'UUID',
    `FirstName` VARCHAR(100) NOT NULL,
    `LastName` VARCHAR(100) NOT NULL,
    `DateOfBirth` DATE NOT NULL,
    `Email` VARCHAR(255),
    `PhoneNumber` VARCHAR(20),
    `MedicalRecordNumber` VARCHAR(50) UNIQUE NOT NULL,
    `Status` INT NOT NULL DEFAULT 0 COMMENT '0=Active, 1=Inactive, 2=Archived',
    `CreatedAt` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    `IsDeleted` BOOLEAN DEFAULT FALSE,
    
    KEY `idx_mrn` (`MedicalRecordNumber`),
    KEY `idx_email` (`Email`),
    KEY `idx_status` (`Status`),
    KEY `idx_lastname_firstname` (`LastName`, `FirstName`),
    
    ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
) COMMENT='Patient master data';

-- ─────────────────────────────────────────────────────────────────────────────
-- APPOINTMENT SERVICE TABLES
-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS `Appointments` (
    `Id` CHAR(36) NOT NULL PRIMARY KEY COMMENT 'UUID',
    `PatientId` CHAR(36) NOT NULL,
    `ProviderId` CHAR(36) NOT NULL,
    `ScheduledStart` DATETIME NOT NULL,
    `ScheduledEnd` DATETIME NOT NULL,
    `Status` INT NOT NULL DEFAULT 0 COMMENT '0=Scheduled, 1=InProgress, 2=Completed, 3=Cancelled',
    `ReasonForVisit` TEXT,
    `Notes` TEXT,
    `CreatedAt` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    `IsDeleted` BOOLEAN DEFAULT FALSE,
    
    KEY `idx_patientid` (`PatientId`),
    KEY `idx_providerid` (`ProviderId`),
    KEY `idx_scheduled_start` (`ScheduledStart`),
    KEY `idx_status` (`Status`),
    KEY `idx_patient_scheduled` (`PatientId`, `ScheduledStart`),
    
    CONSTRAINT `fk_appointments_patients` FOREIGN KEY (`PatientId`) 
        REFERENCES `Patients`(`Id`) ON DELETE RESTRICT ON UPDATE CASCADE,
    
    ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
) COMMENT='Patient appointments with provider scheduling';

-- ─────────────────────────────────────────────────────────────────────────────
-- BILLING SERVICE TABLES
-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS `Invoices` (
    `Id` CHAR(36) NOT NULL PRIMARY KEY COMMENT 'UUID',
    `PatientId` CHAR(36) NOT NULL,
    `InvoiceNumber` VARCHAR(50) NOT NULL UNIQUE,
    `Amount` DECIMAL(18,2) NOT NULL,
    `DueDate` DATE NOT NULL,
    `Status` INT NOT NULL DEFAULT 0 COMMENT '0=Draft, 1=Issued, 2=Paid, 3=Overdue, 4=Cancelled',
    `CreatedAt` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    KEY `idx_patientid` (`PatientId`),
    KEY `idx_status` (`Status`),
    KEY `idx_duedate` (`DueDate`),
    KEY `idx_invoice_number` (`InvoiceNumber`),
    
    CONSTRAINT `fk_invoices_patients` FOREIGN KEY (`PatientId`) 
        REFERENCES `Patients`(`Id`) ON DELETE RESTRICT ON UPDATE CASCADE,
    
    ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
) COMMENT='Patient billing invoices';

-- ─────────────────────────────────────────────────────────────────────────────
-- AUDIT SERVICE TABLES
-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS `AuditEntries` (
    `Id` CHAR(36) NOT NULL PRIMARY KEY COMMENT 'UUID',
    `UserId` CHAR(36),
    `Action` VARCHAR(100) NOT NULL COMMENT 'Create, Read, Update, Delete, Export, Login, etc.',
    `EntityType` VARCHAR(100) NOT NULL COMMENT 'Patient, Appointment, Invoice, etc.',
    `EntityId` CHAR(36),
    `OldValues` JSON,
    `NewValues` JSON,
    `Timestamp` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `IpAddress` VARCHAR(50),
    
    KEY `idx_userid` (`UserId`),
    KEY `idx_entitytype` (`EntityType`),
    KEY `idx_timestamp` (`Timestamp`),
    KEY `idx_action_entity` (`Action`, `EntityType`),
    
    ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
) COMMENT='Complete audit trail for compliance';

-- ─────────────────────────────────────────────────────────────────────────────
-- IDENTITY SERVICE TABLES
-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS `Users` (
    `Id` CHAR(36) NOT NULL PRIMARY KEY COMMENT 'UUID',
    `Username` VARCHAR(100) NOT NULL UNIQUE,
    `Email` VARCHAR(255) NOT NULL UNIQUE,
    `PasswordHash` VARCHAR(255) NOT NULL,
    `FirstName` VARCHAR(100),
    `LastName` VARCHAR(100),
    `IsActive` BOOLEAN DEFAULT TRUE,
    `LastLoginAt` TIMESTAMP NULL,
    `CreatedAt` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    KEY `idx_email` (`Email`),
    KEY `idx_username` (`Username`),
    KEY `idx_isactive` (`IsActive`),
    
    ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
) COMMENT='User authentication and identity';

-- ─────────────────────────────────────────────────────────────────────────────
-- ANALYTICS SERVICE TABLES
-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS `Reports` (
    `Id` CHAR(36) NOT NULL PRIMARY KEY COMMENT 'UUID',
    `Name` VARCHAR(255) NOT NULL,
    `Description` TEXT,
    `Type` INT NOT NULL COMMENT '0=Financial, 1=Clinical, 2=Operational, 3=Compliance',
    `Query` LONGTEXT,
    `Status` INT NOT NULL DEFAULT 0 COMMENT '0=Draft, 1=Published, 2=Archived',
    `CreatedAt` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    `CreatedBy` CHAR(36) NOT NULL,
    
    KEY `idx_type` (`Type`),
    KEY `idx_createdby` (`CreatedBy`),
    KEY `idx_status` (`Status`),
    
    ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
) COMMENT='Analytics reports and queries';

-- ─────────────────────────────────────────────────────────────────────────────
-- OUTBOX PATTERN TABLE (Atomic Event Publishing)
-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS `OutboxEvents` (
    `Id` CHAR(36) NOT NULL PRIMARY KEY COMMENT 'UUID',
    `EventType` VARCHAR(256) NOT NULL COMMENT 'Event class name',
    `EventData` JSON NOT NULL,
    `CreatedAt` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `IsPublished` BOOLEAN DEFAULT FALSE,
    `PublishedAt` TIMESTAMP NULL,
    `PublishAttempts` INT DEFAULT 0,
    `MaxPublishAttempts` INT DEFAULT 3,
    `ErrorMessage` TEXT,
    `AggregateId` CHAR(36),
    `Transport` VARCHAR(50) DEFAULT 'kafka' COMMENT 'kafka or rabbitmq',
    `RoutingKey` VARCHAR(255),
    
    KEY `idx_unpublished` (`IsPublished`, `PublishAttempts`, `CreatedAt`),
    KEY `idx_aggregateid` (`AggregateId`),
    KEY `idx_eventtype` (`EventType`),
    KEY `idx_createdat` (`CreatedAt`),
    
    ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
) COMMENT='Outbox for atomic event publishing to message brokers';

-- ─────────────────────────────────────────────────────────────────────────────
-- PERFORMANCE: MYSQL 8.0+ FEATURES
-- ─────────────────────────────────────────────────────────────────────────────

-- Generated columns for common queries (MySQL 8.0+)
-- ALTER TABLE `Patients` ADD COLUMN full_name VARCHAR(255) GENERATED ALWAYS AS 
-- (CONCAT_WS(' ', FirstName, LastName)) STORED;

-- Window functions for ranking (MySQL 8.0+)
-- Can be used in views for patient statistics

-- ─────────────────────────────────────────────────────────────────────────────
-- STATISTICS AND PERFORMANCE
-- ─────────────────────────────────────────────────────────────────────────────

-- Enable statistics for query optimizer (MySQL 8.0+)
SET GLOBAL innodb_stats_on_metadata = OFF;
SET GLOBAL innodb_stats_auto_recalc = ON;

-- ─────────────────────────────────────────────────────────────────────────────
-- MIGRATION COMPLETE
-- ─────────────────────────────────────────────────────────────────────────────

INSERT INTO `__MigrationHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20250101_001_baseline', '1.0.0')
ON DUPLICATE KEY UPDATE `AppliedAt` = CURRENT_TIMESTAMP;

-- Verification query
SELECT CONCAT(
    'Baseline migration complete. Tables created: ',
    (SELECT COUNT(*) FROM information_schema.TABLES 
     WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME NOT LIKE '\\_\\_%')
) AS Status;
