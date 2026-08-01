-- ═══════════════════════════════════════════════════════════════════════════════
-- Identity Service - PostgreSQL Baseline Migration
-- Version: 20250101_001
-- Purpose: Create identity service schema (Users, Roles, Permissions, JWT tokens)
-- ═══════════════════════════════════════════════════════════════════════════════

-- ─────────────────────────────────────────────────────────────────────────────
-- Users Table
-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS "Users" (
    "Id" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "Email" character varying(255) NOT NULL UNIQUE,
    "PasswordHash" character varying(512) NOT NULL,
    "FirstName" character varying(100),
    "LastName" character varying(100),
    "PhoneNumber" character varying(20),
    "IsEmailVerified" boolean NOT NULL DEFAULT false,
    "IsActive" boolean NOT NULL DEFAULT true,
    "LastLoginAt" timestamp with time zone,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" timestamp with time zone,
    "DeletedAt" timestamp with time zone
);

-- ─────────────────────────────────────────────────────────────────────────────
-- Roles Table
-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS "Roles" (
    "Id" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "Name" character varying(100) NOT NULL UNIQUE,
    "Description" character varying(500),
    "IsActive" boolean NOT NULL DEFAULT true,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- ─────────────────────────────────────────────────────────────────────────────
-- UserRoles Table (Many-to-Many)
-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS "UserRoles" (
    "Id" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" uuid NOT NULL,
    "RoleId" uuid NOT NULL,
    "AssignedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "AssignedBy" uuid,
    CONSTRAINT "FK_UserRoles_Users" FOREIGN KEY ("UserId") 
        REFERENCES "Users"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_UserRoles_Roles" FOREIGN KEY ("RoleId") 
        REFERENCES "Roles"("Id") ON DELETE CASCADE,
    CONSTRAINT "UQ_UserRoles_User_Role" UNIQUE ("UserId", "RoleId")
);

-- ─────────────────────────────────────────────────────────────────────────────
-- Permissions Table
-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS "Permissions" (
    "Id" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "Name" character varying(100) NOT NULL UNIQUE,
    "Description" character varying(500),
    "Resource" character varying(100),
    "Action" character varying(50),
    "IsActive" boolean NOT NULL DEFAULT true,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- ─────────────────────────────────────────────────────────────────────────────
-- RolePermissions Table (Many-to-Many)
-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS "RolePermissions" (
    "Id" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "RoleId" uuid NOT NULL,
    "PermissionId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "FK_RolePermissions_Roles" FOREIGN KEY ("RoleId") 
        REFERENCES "Roles"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_RolePermissions_Permissions" FOREIGN KEY ("PermissionId") 
        REFERENCES "Permissions"("Id") ON DELETE CASCADE,
    CONSTRAINT "UQ_RolePermissions_Role_Permission" UNIQUE ("RoleId", "PermissionId")
);

-- ─────────────────────────────────────────────────────────────────────────────
-- RefreshTokens Table
-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS "RefreshTokens" (
    "Id" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" uuid NOT NULL,
    "Token" character varying(512) NOT NULL UNIQUE,
    "ExpiresAt" timestamp with time zone NOT NULL,
    "RevokedAt" timestamp with time zone,
    "ReplacedByToken" character varying(512),
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "FK_RefreshTokens_Users" FOREIGN KEY ("UserId") 
        REFERENCES "Users"("Id") ON DELETE CASCADE
);

-- ─────────────────────────────────────────────────────────────────────────────
-- AuditLog Table (Local to Identity Service)
-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS "IdentityAuditLog" (
    "Id" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" uuid,
    "Action" character varying(50) NOT NULL,
    "Entity" character varying(100),
    "OldValues" text,
    "NewValues" text,
    "IpAddress" character varying(50),
    "UserAgent" text,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP
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

CREATE INDEX IF NOT EXISTS "IX_Users_Email" ON "Users" ("Email");
CREATE INDEX IF NOT EXISTS "IX_Users_IsActive" ON "Users" ("IsActive");
CREATE INDEX IF NOT EXISTS "IX_Users_DeletedAt" ON "Users" ("DeletedAt");
CREATE INDEX IF NOT EXISTS "IX_UserRoles_UserId" ON "UserRoles" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_UserRoles_RoleId" ON "UserRoles" ("RoleId");
CREATE INDEX IF NOT EXISTS "IX_RolePermissions_RoleId" ON "RolePermissions" ("RoleId");
CREATE INDEX IF NOT EXISTS "IX_RolePermissions_PermissionId" ON "RolePermissions" ("PermissionId");
CREATE INDEX IF NOT EXISTS "IX_RefreshTokens_UserId" ON "RefreshTokens" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_RefreshTokens_ExpiresAt" ON "RefreshTokens" ("ExpiresAt");
CREATE INDEX IF NOT EXISTS "IX_IdentityAuditLog_UserId" ON "IdentityAuditLog" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_IdentityAuditLog_CreatedAt" ON "IdentityAuditLog" ("CreatedAt");

-- ─────────────────────────────────────────────────────────────────────────────
-- Insert Default Roles and Permissions
-- ─────────────────────────────────────────────────────────────────────────────

INSERT INTO "Roles" ("Id", "Name", "Description") VALUES
    (gen_random_uuid(), 'Admin', 'System administrator with full access'),
    (gen_random_uuid(), 'Doctor', 'Healthcare provider'),
    (gen_random_uuid(), 'Nurse', 'Nursing staff'),
    (gen_random_uuid(), 'Patient', 'Patient user'),
    (gen_random_uuid(), 'Billing', 'Billing department staff'),
    (gen_random_uuid(), 'Auditor', 'Compliance and audit staff')
ON CONFLICT DO NOTHING;

INSERT INTO "Permissions" ("Id", "Name", "Resource", "Action") VALUES
    (gen_random_uuid(), 'read_patient', 'Patient', 'READ'),
    (gen_random_uuid(), 'create_patient', 'Patient', 'CREATE'),
    (gen_random_uuid(), 'update_patient', 'Patient', 'UPDATE'),
    (gen_random_uuid(), 'delete_patient', 'Patient', 'DELETE'),
    (gen_random_uuid(), 'read_appointment', 'Appointment', 'READ'),
    (gen_random_uuid(), 'create_appointment', 'Appointment', 'CREATE'),
    (gen_random_uuid(), 'read_clinical', 'Clinical', 'READ'),
    (gen_random_uuid(), 'create_clinical', 'Clinical', 'CREATE'),
    (gen_random_uuid(), 'read_billing', 'Billing', 'READ'),
    (gen_random_uuid(), 'create_billing', 'Billing', 'CREATE')
ON CONFLICT DO NOTHING;

-- ─────────────────────────────────────────────────────────────────────────────
-- Record Migration History
-- ─────────────────────────────────────────────────────────────────────────────

INSERT INTO "__MigrationHistory" ("MigrationId", "ProductVersion")
VALUES ('20250101_001_baseline', '8.0.0')
ON CONFLICT DO NOTHING;

COMMIT;
