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
-- OUTBOX EVENTS (Common Infrastructure)
-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS `OutboxEvents` (
    `Id` CHAR(36) NOT NULL PRIMARY KEY COMMENT 'UUID',
    `EventType` VARCHAR(256) NOT NULL,
    `EventData` JSON NOT NULL,
    `CreatedAt` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `IsPublished` BOOLEAN DEFAULT FALSE,
    `PublishedAt` TIMESTAMP NULL,
    `PublishAttempts` INT DEFAULT 0,
    `MaxPublishAttempts` INT DEFAULT 3,
    `ErrorMessage` TEXT,
    `AggregateId` CHAR(36),
    `Transport` VARCHAR(50) DEFAULT 'kafka',
    `RoutingKey` VARCHAR(255),
    
    KEY `idx_unpublished` (`IsPublished`, `PublishAttempts`, `CreatedAt`),
    KEY `idx_aggregateid` (`AggregateId`),
    KEY `idx_eventtype` (`EventType`),
    KEY `idx_createdat` (`CreatedAt`),
    
    ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
) COMMENT='Event outbox for reliable event publishing';

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
    `Action` VARCHAR(100) NOT NULL,
    `EntityType` VARCHAR(100) NOT NULL,
    `EntityId` CHAR(36),
    `OldValues` JSON,
    `NewValues` JSON,
    `Timestamp` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `IpAddress` VARCHAR(50),
    
    KEY `idx_userid` (`UserId`),
    KEY `idx_entitytype` (`EntityType`),
    KEY `idx_timestamp` (`Timestamp`),
    KEY `idx_action_entitytype` (`Action`, `EntityType`),
    
    ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
) COMMENT='HIPAA-compliant audit trail';

CREATE TABLE IF NOT EXISTS `AccessLogs` (
    `Id` CHAR(36) NOT NULL PRIMARY KEY COMMENT 'UUID',
    `UserId` CHAR(36) NOT NULL,
    `ResourceType` VARCHAR(100) NOT NULL,
    `ResourceId` CHAR(36),
    `AccessType` VARCHAR(50) NOT NULL COMMENT 'Read, Write, Delete',
    `Timestamp` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `IpAddress` VARCHAR(50),
    
    KEY `idx_userid` (`UserId`),
    KEY `idx_resourcetype` (`ResourceType`),
    KEY `idx_timestamp` (`Timestamp`),
    
    ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
) COMMENT='Access control audit logs';

-- ─────────────────────────────────────────────────────────────────────────────
-- IDENTITY SERVICE TABLES
-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS `Users` (
    `Id` CHAR(36) NOT NULL PRIMARY KEY COMMENT 'UUID',
    `Username` VARCHAR(100) UNIQUE NOT NULL,
    `Email` VARCHAR(255) UNIQUE NOT NULL,
    `PasswordHash` VARCHAR(255) NOT NULL,
    `FirstName` VARCHAR(100),
    `LastName` VARCHAR(100),
    `IsActive` BOOLEAN DEFAULT TRUE,
    `LastLoginAt` TIMESTAMP NULL,
    `CreatedAt` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    KEY `idx_email` (`Email`),
    KEY `idx_isactive` (`IsActive`),
    KEY `idx_username` (`Username`),
    
    ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
) COMMENT='System users and identity';

-- ─────────────────────────────────────────────────────────────────────────────
-- CLINICAL SERVICE TABLES
-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS `ClinicalNotes` (
    `Id` CHAR(36) NOT NULL PRIMARY KEY COMMENT 'UUID',
    `PatientId` CHAR(36) NOT NULL,
    `ProviderId` CHAR(36) NOT NULL,
    `Content` LONGTEXT NOT NULL,
    `NoteType` INT DEFAULT 0 COMMENT '0=Progress, 1=Consultation, 2=Discharge',
    `CreatedAt` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    `DeletedAt` TIMESTAMP NULL,
    
    KEY `idx_patientid` (`PatientId`),
    KEY `idx_providerid` (`ProviderId`),
    KEY `idx_createdat` (`CreatedAt`),
    KEY `idx_notenotetype` (`NoteType`),
    
    CONSTRAINT `fk_clinicalnotes_patients` FOREIGN KEY (`PatientId`) 
        REFERENCES `Patients`(`Id`) ON DELETE RESTRICT ON UPDATE CASCADE,
    
    ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
) COMMENT='Patient clinical notes';

CREATE TABLE IF NOT EXISTS `VitalSigns` (
    `Id` CHAR(36) NOT NULL PRIMARY KEY COMMENT 'UUID',
    `PatientId` CHAR(36) NOT NULL,
    `Temperature` DECIMAL(5,2),
    `BloodPressureSystolic` INT,
    `BloodPressureDiastolic` INT,
    `HeartRate` INT,
    `RespiratoryRate` INT,
    `RecordedAt` TIMESTAMP NOT NULL,
    `CreatedAt` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    `DeletedAt` TIMESTAMP NULL,
    
    KEY `idx_patientid` (`PatientId`),
    KEY `idx_recordedat` (`RecordedAt`),
    
    CONSTRAINT `fk_vitalsigns_patients` FOREIGN KEY (`PatientId`) 
        REFERENCES `Patients`(`Id`) ON DELETE RESTRICT ON UPDATE CASCADE,
    
    ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
) COMMENT='Patient vital signs measurements';

CREATE TABLE IF NOT EXISTS `ClinicalDiagnoses` (
    `Id` CHAR(36) NOT NULL PRIMARY KEY COMMENT 'UUID',
    `PatientId` CHAR(36) NOT NULL,
    `DiagnosisCode` VARCHAR(20) NOT NULL COMMENT 'ICD-10 code',
    `DiagnosisText` VARCHAR(255) NOT NULL,
    `DiagnosedDate` DATE NOT NULL,
    `Status` INT DEFAULT 0 COMMENT '0=Active, 1=Resolved, 2=Inactive',
    `CreatedAt` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    `DeletedAt` TIMESTAMP NULL,
    
    KEY `idx_patientid` (`PatientId`),
    KEY `idx_diagnosiscode` (`DiagnosisCode`),
    KEY `idx_status` (`Status`),
    
    CONSTRAINT `fk_diagnoses_patients` FOREIGN KEY (`PatientId`) 
        REFERENCES `Patients`(`Id`) ON DELETE RESTRICT ON UPDATE CASCADE,
    
    ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
) COMMENT='Patient clinical diagnoses';

CREATE TABLE IF NOT EXISTS `ClinicalProcedures` (
    `Id` CHAR(36) NOT NULL PRIMARY KEY COMMENT 'UUID',
    `PatientId` CHAR(36) NOT NULL,
    `ProcedureName` VARCHAR(255) NOT NULL,
    `ProcedureCode` VARCHAR(20),
    `ProcedureDate` DATE NOT NULL,
    `Status` INT DEFAULT 0 COMMENT '0=Scheduled, 1=Completed, 2=Cancelled',
    `Notes` TEXT,
    `CreatedAt` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    `DeletedAt` TIMESTAMP NULL,
    
    KEY `idx_patientid` (`PatientId`),
    KEY `idx_proceduredate` (`ProcedureDate`),
    KEY `idx_status` (`Status`),
    
    CONSTRAINT `fk_procedures_patients` FOREIGN KEY (`PatientId`) 
        REFERENCES `Patients`(`Id`) ON DELETE RESTRICT ON UPDATE CASCADE,
    
    ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
) COMMENT='Patient clinical procedures';

-- ─────────────────────────────────────────────────────────────────────────────
-- NOTIFICATION SERVICE TABLES
-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS `Notifications` (
    `Id` CHAR(36) NOT NULL PRIMARY KEY COMMENT 'UUID',
    `UserId` CHAR(36) NOT NULL,
    `Subject` VARCHAR(255) NOT NULL,
    `Message` TEXT NOT NULL,
    `Type` INT DEFAULT 0 COMMENT '0=Email, 1=SMS, 2=Push',
    `Status` INT DEFAULT 0 COMMENT '0=Pending, 1=Sent, 2=Failed',
    `IsRead` BOOLEAN DEFAULT FALSE,
    `ReadAt` TIMESTAMP NULL,
    `CreatedAt` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    KEY `idx_userid` (`UserId`),
    KEY `idx_isread` (`IsRead`),
    KEY `idx_createdat` (`CreatedAt`),
    KEY `idx_status` (`Status`),
    
    ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
) COMMENT='User notifications';

CREATE TABLE IF NOT EXISTS `NotificationTemplates` (
    `Id` CHAR(36) NOT NULL PRIMARY KEY COMMENT 'UUID',
    `Name` VARCHAR(100) UNIQUE NOT NULL,
    `Subject` VARCHAR(255) NOT NULL,
    `Body` LONGTEXT NOT NULL,
    `Type` INT DEFAULT 0 COMMENT '0=Email, 1=SMS, 2=Push',
    `CreatedAt` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    KEY `idx_name` (`Name`),
    KEY `idx_type` (`Type`),
    
    ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
) COMMENT='Notification templates';

CREATE TABLE IF NOT EXISTS `NotificationPreferences` (
    `Id` CHAR(36) NOT NULL PRIMARY KEY COMMENT 'UUID',
    `UserId` CHAR(36) NOT NULL UNIQUE,
    `EmailNotifications` BOOLEAN DEFAULT TRUE,
    `SmsNotifications` BOOLEAN DEFAULT FALSE,
    `PushNotifications` BOOLEAN DEFAULT TRUE,
    `CreatedAt` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    KEY `idx_userid` (`UserId`),
    
    ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
) COMMENT='User notification preferences';

-- ─────────────────────────────────────────────────────────────────────────────
-- PRESCRIPTION SERVICE TABLES
-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS `Prescriptions` (
    `Id` CHAR(36) NOT NULL PRIMARY KEY COMMENT 'UUID',
    `PatientId` CHAR(36) NOT NULL,
    `ProviderId` CHAR(36) NOT NULL,
    `MedicationName` VARCHAR(255) NOT NULL,
    `Dosage` VARCHAR(100) NOT NULL,
    `Frequency` VARCHAR(100) NOT NULL,
    `StartDate` DATE NOT NULL,
    `EndDate` DATE,
    `Quantity` INT,
    `Refills` INT DEFAULT 0,
    `Status` INT DEFAULT 0 COMMENT '0=Active, 1=Inactive, 2=Expired',
    `CreatedAt` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    `DeletedAt` TIMESTAMP NULL,
    
    KEY `idx_patientid` (`PatientId`),
    KEY `idx_status` (`Status`),
    KEY `idx_startdate` (`StartDate`),
    KEY `idx_enddate` (`EndDate`),
    
    CONSTRAINT `fk_prescriptions_patients` FOREIGN KEY (`PatientId`) 
        REFERENCES `Patients`(`Id`) ON DELETE RESTRICT ON UPDATE CASCADE,
    
    ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
) COMMENT='Patient prescriptions';

CREATE TABLE IF NOT EXISTS `PrescriptionRefills` (
    `Id` CHAR(36) NOT NULL PRIMARY KEY COMMENT 'UUID',
    `PrescriptionId` CHAR(36) NOT NULL,
    `RequestDate` DATE NOT NULL,
    `Status` INT DEFAULT 0 COMMENT '0=Pending, 1=Approved, 2=Denied',
    `ApprovedDate` DATE,
    `ApprovedBy` CHAR(36),
    `CreatedAt` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    KEY `idx_prescriptionid` (`PrescriptionId`),
    KEY `idx_status` (`Status`),
    KEY `idx_requestdate` (`RequestDate`),
    
    CONSTRAINT `fk_refills_prescriptions` FOREIGN KEY (`PrescriptionId`) 
        REFERENCES `Prescriptions`(`Id`) ON DELETE CASCADE ON UPDATE CASCADE,
    
    ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
) COMMENT='Prescription refill requests';

-- ─────────────────────────────────────────────────────────────────────────────
-- ANALYTICS SERVICE TABLES
-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS `Reports` (
    `Id` CHAR(36) NOT NULL PRIMARY KEY COMMENT 'UUID',
    `Name` VARCHAR(255) NOT NULL,
    `Description` TEXT,
    `Type` INT NOT NULL COMMENT '0=Patient, 1=Financial, 2=Clinical, 3=Operational',
    `Query` LONGTEXT,
    `Status` INT DEFAULT 0 COMMENT '0=Draft, 1=Published, 2=Archived',
    `CreatedAt` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAt` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    `CreatedBy` CHAR(36) NOT NULL,
    
    KEY `idx_type` (`Type`),
    KEY `idx_createdby` (`CreatedBy`),
    KEY `idx_status` (`Status`),
    
    ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
) COMMENT='Analytics reports';

-- ─────────────────────────────────────────────────────────────────────────────
-- RECORD INITIAL MIGRATION
-- ─────────────────────────────────────────────────────────────────────────────

INSERT INTO `__MigrationHistory` (`MigrationId`, `ProductVersion`) 
VALUES ('20250101_001_baseline', '8.0.0')
ON DUPLICATE KEY UPDATE `AppliedAt` = NOW();
