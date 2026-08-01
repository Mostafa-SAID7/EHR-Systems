-- ═══════════════════════════════════════════════════════════════════════════════
-- Patient Service - PostgreSQL Baseline Migration
-- Version: 20250101_001
-- Purpose: Create patient service schema (Master patient data, demographics)
-- ═══════════════════════════════════════════════════════════════════════════════

-- ─────────────────────────────────────────────────────────────────────────────
-- Patients Table (Core Master Data)
-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS "Patients" (
    "Id" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "MedicalRecordNumber" character varying(50) NOT NULL UNIQUE,
    "FirstName" character varying(100) NOT NULL,
    "LastName" character varying(100) NOT NULL,
    "MiddleName" character varying(100),
    "DateOfBirth" date NOT NULL,
    "Gender" character varying(20),
    "Email" character varying(255),
    "PhoneNumber" character varying(20),
    "Status" character varying(50) NOT NULL DEFAULT 'Active',
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" timestamp with time zone,
    "DeletedAt" timestamp with time zone
);

-- ─────────────────────────────────────────────────────────────────────────────
-- Patient Contact Information
-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS "PatientContacts" (
    "Id" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "PatientId" uuid NOT NULL,
    "AddressLine1" character varying(255),
    "AddressLine2" character varying(255),
    "City" character varying(100),
    "State" character varying(50),
    "PostalCode" character varying(20),
    "Country" character varying(100),
    "IsPrimary" boolean NOT NULL DEFAULT true,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "FK_PatientContacts_Patients" FOREIGN KEY ("PatientId")
        REFERENCES "Patients" ("Id") ON DELETE CASCADE
);

-- ─────────────────────────────────────────────────────────────────────────────
-- Patient Allergies
-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS "PatientAllergies" (
    "Id" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "PatientId" uuid NOT NULL,
    "AllergenName" character varying(255) NOT NULL,
    "AllergenType" character varying(50),
    "Severity" character varying(50),
    "Reaction" text,
    "OnsetDate" date,
    "ResolvedDate" date,
    "IsCurrent" boolean NOT NULL DEFAULT true,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "FK_PatientAllergies_Patients" FOREIGN KEY ("PatientId")
        REFERENCES "Patients" ("Id") ON DELETE CASCADE
);

-- ─────────────────────────────────────────────────────────────────────────────
-- Patient Medical Conditions
-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS "PatientConditions" (
    "Id" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "PatientId" uuid NOT NULL,
    "ConditionName" character varying(255) NOT NULL,
    "ICD10Code" character varying(20),
    "OnsetDate" date,
    "ResolutionDate" date,
    "Status" character varying(50),
    "Notes" text,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "FK_PatientConditions_Patients" FOREIGN KEY ("PatientId")
        REFERENCES "Patients" ("Id") ON DELETE CASCADE
);

-- ─────────────────────────────────────────────────────────────────────────────
-- Patient Insurance Information
-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS "PatientInsurance" (
    "Id" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "PatientId" uuid NOT NULL,
    "InsuranceCompanyName" character varying(255) NOT NULL,
    "PolicyNumber" character varying(100) NOT NULL,
    "GroupNumber" character varying(100),
    "MemberId" character varying(100),
    "EffectiveDate" date NOT NULL,
    "TerminationDate" date,
    "IsPrimary" boolean NOT NULL DEFAULT true,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "FK_PatientInsurance_Patients" FOREIGN KEY ("PatientId")
        REFERENCES "Patients" ("Id") ON DELETE CASCADE
);

-- ─────────────────────────────────────────────────────────────────────────────
-- Patient Emergency Contacts
-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS "PatientEmergencyContacts" (
    "Id" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "PatientId" uuid NOT NULL,
    "ContactName" character varying(255) NOT NULL,
    "Relationship" character varying(100),
    "PhoneNumber" character varying(20),
    "Email" character varying(255),
    "IsPrimary" boolean NOT NULL DEFAULT true,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "FK_PatientEmergencyContacts_Patients" FOREIGN KEY ("PatientId")
        REFERENCES "Patients" ("Id") ON DELETE CASCADE
);

-- ─────────────────────────────────────────────────────────────────────────────
-- Patient Medical History (Summary)
-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS "PatientMedicalHistory" (
    "Id" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "PatientId" uuid NOT NULL UNIQUE,
    "BloodType" character varying(20),
    "Height" decimal(5, 2),
    "Weight" decimal(7, 2),
    "SurgicalHistory" text,
    "FamilyHistory" text,
    "SocialHistory" text,
    "LastUpdatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "FK_PatientMedicalHistory_Patients" FOREIGN KEY ("PatientId")
        REFERENCES "Patients" ("Id") ON DELETE CASCADE
);

-- ─────────────────────────────────────────────────────────────────────────────
-- Patient Communication Preferences
-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS "PatientCommunicationPreferences" (
    "Id" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "PatientId" uuid NOT NULL UNIQUE,
    "PreferredContactMethod" character varying(50),
    "AllowEmailNotifications" boolean NOT NULL DEFAULT true,
    "AllowSmsNotifications" boolean NOT NULL DEFAULT true,
    "AllowPhoneNotifications" boolean NOT NULL DEFAULT true,
    "Language" character varying(10),
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "FK_PatientCommunicationPreferences_Patients" FOREIGN KEY ("PatientId")
        REFERENCES "Patients" ("Id") ON DELETE CASCADE
);

-- ─────────────────────────────────────────────────────────────────────────────
-- Migration History Table
-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS "__MigrationHistory" (
    "MigrationId" character varying(150) PRIMARY KEY,
    "ProductVersion" character varying(32) NOT NULL,
    "AppliedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- ─────────────────────────────────────────────────────────────────────────────
-- Indexes for Performance
-- ─────────────────────────────────────────────────────────────────────────────

CREATE INDEX IF NOT EXISTS "IX_Patients_MRN" ON "Patients" ("MedicalRecordNumber");
CREATE INDEX IF NOT EXISTS "IX_Patients_Email" ON "Patients" ("Email");
CREATE INDEX IF NOT EXISTS "IX_Patients_PhoneNumber" ON "Patients" ("PhoneNumber");
CREATE INDEX IF NOT EXISTS "IX_Patients_Status" ON "Patients" ("Status");
CREATE INDEX IF NOT EXISTS "IX_Patients_DeletedAt" ON "Patients" ("DeletedAt");
CREATE INDEX IF NOT EXISTS "IX_Patients_CreatedAt" ON "Patients" ("CreatedAt");
CREATE INDEX IF NOT EXISTS "IX_PatientContacts_PatientId" ON "PatientContacts" ("PatientId");
CREATE INDEX IF NOT EXISTS "IX_PatientAllergies_PatientId" ON "PatientAllergies" ("PatientId");
CREATE INDEX IF NOT EXISTS "IX_PatientAllergies_IsCurrent" ON "PatientAllergies" ("IsCurrent");
CREATE INDEX IF NOT EXISTS "IX_PatientConditions_PatientId" ON "PatientConditions" ("PatientId");
CREATE INDEX IF NOT EXISTS "IX_PatientConditions_ICD10Code" ON "PatientConditions" ("ICD10Code");
CREATE INDEX IF NOT EXISTS "IX_PatientInsurance_PatientId" ON "PatientInsurance" ("PatientId");
CREATE INDEX IF NOT EXISTS "IX_PatientInsurance_PolicyNumber" ON "PatientInsurance" ("PolicyNumber");
CREATE INDEX IF NOT EXISTS "IX_PatientEmergencyContacts_PatientId" ON "PatientEmergencyContacts" ("PatientId");
CREATE INDEX IF NOT EXISTS "IX_PatientMedicalHistory_PatientId" ON "PatientMedicalHistory" ("PatientId");

-- ─────────────────────────────────────────────────────────────────────────────
-- Record Migration History
-- ─────────────────────────────────────────────────────────────────────────────

INSERT INTO "__MigrationHistory" ("MigrationId", "ProductVersion")
VALUES ('20250101_001_baseline', '8.0.0')
ON CONFLICT DO NOTHING;

COMMIT;
