-- ═══════════════════════════════════════════════════════════════════════════════
-- EHR Platform Database - Baseline Migration
-- Version: 20250101_001
-- Created: 2025-01-01
-- Description: Initial schema setup for all microservices
-- ═══════════════════════════════════════════════════════════════════════════════

-- Enable UUID extension for PostgreSQL
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ─────────────────────────────────────────────────────────────────────────────
-- SCHEMA: Common Infrastructure Tables
-- ─────────────────────────────────────────────────────────────────────────────

-- Outbox Events (for all services - atomic event publishing)
CREATE TABLE IF NOT EXISTS "OutboxEvents" (
    "Id" uuid NOT NULL PRIMARY KEY,
    "EventType" character varying(256) NOT NULL,
    "EventData" jsonb NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "IsPublished" boolean NOT NULL DEFAULT false,
    "PublishedAt" timestamp with time zone,
    "PublishAttempts" integer NOT NULL DEFAULT 0,
    "MaxPublishAttempts" integer NOT NULL DEFAULT 3,
    "ErrorMessage" text,
    "AggregateId" uuid,
    "Transport" character varying(50) NOT NULL DEFAULT 'kafka',
    "RoutingKey" character varying(255)
);

-- Create indexes for efficient querying
CREATE INDEX IF NOT EXISTS "IX_OutboxEvent_Unpublished" ON "OutboxEvents" 
    ("IsPublished", "PublishAttempts", "CreatedAt");
CREATE INDEX IF NOT EXISTS "IX_OutboxEvent_AggregateId" ON "OutboxEvents" ("AggregateId");
CREATE INDEX IF NOT EXISTS "IX_OutboxEvent_EventType" ON "OutboxEvents" ("EventType");
CREATE INDEX IF NOT EXISTS "IX_OutboxEvent_CreatedAt" ON "OutboxEvents" ("CreatedAt");

-- ─────────────────────────────────────────────────────────────────────────────
-- SCHEMA: Patient Service
-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS "Patients" (
    "Id" uuid NOT NULL PRIMARY KEY,
    "FirstName" character varying(100) NOT NULL,
    "LastName" character varying(100) NOT NULL,
    "DateOfBirth" date NOT NULL,
    "Email" character varying(255),
    "PhoneNumber" character varying(20),
    "MedicalRecordNumber" character varying(50) UNIQUE,
    "Status" integer NOT NULL DEFAULT 0,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "IsDeleted" boolean NOT NULL DEFAULT false
);

CREATE INDEX IF NOT EXISTS "IX_Patients_MRN" ON "Patients" ("MedicalRecordNumber");
CREATE INDEX IF NOT EXISTS "IX_Patients_Email" ON "Patients" ("Email");
CREATE INDEX IF NOT EXISTS "IX_Patients_Status" ON "Patients" ("Status");

-- ─────────────────────────────────────────────────────────────────────────────
-- SCHEMA: Appointment Service
-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS "Appointments" (
    "Id" uuid NOT NULL PRIMARY KEY,
    "PatientId" uuid NOT NULL,
    "ProviderId" uuid NOT NULL,
    "ScheduledStart" timestamp with time zone NOT NULL,
    "ScheduledEnd" timestamp with time zone NOT NULL,
    "Status" integer NOT NULL DEFAULT 0,
    "ReasonForVisit" text,
    "Notes" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "IsDeleted" boolean NOT NULL DEFAULT false
);

CREATE INDEX IF NOT EXISTS "IX_Appointments_PatientId" ON "Appointments" ("PatientId");
CREATE INDEX IF NOT EXISTS "IX_Appointments_ProviderId" ON "Appointments" ("ProviderId");
CREATE INDEX IF NOT EXISTS "IX_Appointments_ScheduledStart" ON "Appointments" ("ScheduledStart");
CREATE INDEX IF NOT EXISTS "IX_Appointments_Status" ON "Appointments" ("Status");

-- ─────────────────────────────────────────────────────────────────────────────
-- SCHEMA: Billing Service
-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS "Invoices" (
    "Id" uuid NOT NULL PRIMARY KEY,
    "PatientId" uuid NOT NULL,
    "InvoiceNumber" character varying(50) UNIQUE NOT NULL,
    "Amount" numeric(18,2) NOT NULL,
    "DueDate" date NOT NULL,
    "Status" integer NOT NULL DEFAULT 0,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL
);

CREATE INDEX IF NOT EXISTS "IX_Invoices_PatientId" ON "Invoices" ("PatientId");
CREATE INDEX IF NOT EXISTS "IX_Invoices_Status" ON "Invoices" ("Status");
CREATE INDEX IF NOT EXISTS "IX_Invoices_DueDate" ON "Invoices" ("DueDate");

-- ─────────────────────────────────────────────────────────────────────────────
-- SCHEMA: Audit Service
-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS "AuditEntries" (
    "Id" uuid NOT NULL PRIMARY KEY,
    "UserId" uuid,
    "Action" character varying(100) NOT NULL,
    "EntityType" character varying(100) NOT NULL,
    "EntityId" uuid,
    "OldValues" jsonb,
    "NewValues" jsonb,
    "Timestamp" timestamp with time zone NOT NULL,
    "IpAddress" character varying(50)
);

CREATE INDEX IF NOT EXISTS "IX_AuditEntries_UserId" ON "AuditEntries" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_AuditEntries_EntityType" ON "AuditEntries" ("EntityType");
CREATE INDEX IF NOT EXISTS "IX_AuditEntries_Timestamp" ON "AuditEntries" ("Timestamp");

-- ─────────────────────────────────────────────────────────────────────────────
-- SCHEMA: Identity Service
-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS "Users" (
    "Id" uuid NOT NULL PRIMARY KEY,
    "Username" character varying(100) UNIQUE NOT NULL,
    "Email" character varying(255) UNIQUE NOT NULL,
    "PasswordHash" character varying(255) NOT NULL,
    "FirstName" character varying(100),
    "LastName" character varying(100),
    "IsActive" boolean NOT NULL DEFAULT true,
    "LastLoginAt" timestamp with time zone,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL
);

CREATE INDEX IF NOT EXISTS "IX_Users_Email" ON "Users" ("Email");
CREATE INDEX IF NOT EXISTS "IX_Users_IsActive" ON "Users" ("IsActive");

-- ─────────────────────────────────────────────────────────────────────────────
-- SCHEMA: Analytics Service
-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS "Reports" (
    "Id" uuid NOT NULL PRIMARY KEY,
    "Name" character varying(255) NOT NULL,
    "Description" text,
    "Type" integer NOT NULL,
    "Query" text,
    "Status" integer NOT NULL DEFAULT 0,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid NOT NULL
);

CREATE INDEX IF NOT EXISTS "IX_Reports_Type" ON "Reports" ("Type");
CREATE INDEX IF NOT EXISTS "IX_Reports_CreatedBy" ON "Reports" ("CreatedBy");

-- ─────────────────────────────────────────────────────────────────────────────
-- MIGRATION TRACKING
-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS "__MigrationHistory" (
    "MigrationId" character varying(150) NOT NULL PRIMARY KEY,
    "ProductVersion" character varying(32) NOT NULL,
    "AppliedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Track this migration
INSERT INTO "__MigrationHistory" ("MigrationId", "ProductVersion")
VALUES ('20250101_001_baseline', '8.0.0')
ON CONFLICT DO NOTHING;

COMMIT;
