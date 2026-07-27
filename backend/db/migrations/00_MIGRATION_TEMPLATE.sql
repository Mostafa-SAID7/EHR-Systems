-- ═══════════════════════════════════════════════════════════════════════════════
-- EHR Platform Database - Migration Template
-- Version: YYYYMMDD_NNN
-- Created: YYYY-MM-DD
-- Description: [Brief description of changes]
-- Author: [Your Name]
-- ═══════════════════════════════════════════════════════════════════════════════
-- 
-- MIGRATION GUIDELINES:
-- 1. Version Format: YYYYMMDD_NNN (e.g., 20250115_001)
-- 2. NNN: Sequential number (001, 002, 003, etc. per day)
-- 3. Always use IF NOT EXISTS / IF EXISTS clauses
-- 4. Create indexes immediately after table creation
-- 5. Add rollback information at the end
-- 6. Test migrations on development first
-- 7. NEVER use DROP without CASCADE (risky!)
-- 8. Include data transformation scripts if applicable
-- ═══════════════════════════════════════════════════════════════════════════════

-- ─────────────────────────────────────────────────────────────────────────────
-- NEW TABLE EXAMPLE
-- ─────────────────────────────────────────────────────────────────────────────

-- Create new table with constraints
CREATE TABLE IF NOT EXISTS "NewTableName" (
    "Id" uuid NOT NULL PRIMARY KEY,
    "Name" character varying(255) NOT NULL,
    "Description" text,
    "Status" integer NOT NULL DEFAULT 0,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "UQ_NewTableName_Name" UNIQUE ("Name")
);

-- Create indexes for commonly queried columns
CREATE INDEX IF NOT EXISTS "IX_NewTableName_Status" ON "NewTableName" ("Status");
CREATE INDEX IF NOT EXISTS "IX_NewTableName_CreatedAt" ON "NewTableName" ("CreatedAt");

-- Add comment for documentation
COMMENT ON TABLE "NewTableName" IS 'Description of what this table contains';

-- ─────────────────────────────────────────────────────────────────────────────
-- COLUMN ADDITION EXAMPLE
-- ─────────────────────────────────────────────────────────────────────────────

-- Add new column with default value
ALTER TABLE "ExistingTable"
ADD COLUMN IF NOT EXISTS "NewColumn" character varying(100) DEFAULT 'default_value';

-- Drop column (use cautiously)
-- ALTER TABLE "ExistingTable" DROP COLUMN IF EXISTS "OldColumn";

-- Rename column
-- ALTER TABLE "ExistingTable" RENAME COLUMN "OldName" TO "NewName";

-- ─────────────────────────────────────────────────────────────────────────────
-- INDEX MANAGEMENT EXAMPLE
-- ─────────────────────────────────────────────────────────────────────────────

-- Create index on existing table
CREATE INDEX IF NOT EXISTS "IX_TableName_ColumnName" ON "TableName" ("ColumnName");

-- Create composite index
CREATE INDEX IF NOT EXISTS "IX_TableName_Composite" ON "TableName" ("Column1", "Column2");

-- Create unique index (for uniqueness constraint)
CREATE UNIQUE INDEX IF NOT EXISTS "UX_TableName_Email" ON "TableName" ("Email");

-- Drop index
-- DROP INDEX IF EXISTS "IX_TableName_OldIndex";

-- ─────────────────────────────────────────────────────────────────────────────
-- DATA TRANSFORMATION EXAMPLE
-- ─────────────────────────────────────────────────────────────────────────────

-- Update existing data
UPDATE "Patients"
SET "Status" = 1
WHERE "Status" = 0 AND "CreatedAt" < NOW() - INTERVAL '30 days';

-- Copy data from old to new column
UPDATE "Users"
SET "NewField" = COALESCE("OldField", 'DEFAULT')
WHERE "NewField" IS NULL;

-- ─────────────────────────────────────────────────────────────────────────────
-- CONSTRAINT MANAGEMENT EXAMPLE
-- ─────────────────────────────────────────────────────────────────────────────

-- Add foreign key constraint
ALTER TABLE "Orders"
ADD CONSTRAINT "FK_Orders_Customers" FOREIGN KEY ("CustomerId")
REFERENCES "Customers" ("Id") ON DELETE CASCADE;

-- Add check constraint
ALTER TABLE "Payments"
ADD CONSTRAINT "CK_Payments_Amount" CHECK ("Amount" > 0);

-- Add unique constraint
ALTER TABLE "Emails"
ADD CONSTRAINT "UQ_Emails_Value" UNIQUE ("EmailValue");

-- ─────────────────────────────────────────────────────────────────────────────
-- MIGRATION TRACKING
-- ─────────────────────────────────────────────────────────────────────────────

-- Record this migration in history table
INSERT INTO "__MigrationHistory" ("MigrationId", "ProductVersion")
VALUES ('YYYYMMDD_NNN_description', '8.0.0')
ON CONFLICT DO NOTHING;

-- ═════════════════════════════════════════════════════════════════════════════
-- ROLLBACK PROCEDURE
-- ═════════════════════════════════════════════════════════════════════════════
-- In case of emergency, manually execute the reverse operations:
--
-- DROP TABLE IF EXISTS "NewTableName" CASCADE;
-- ALTER TABLE "ExistingTable" DROP COLUMN IF EXISTS "NewColumn";
-- DELETE FROM "__MigrationHistory" WHERE "MigrationId" = 'YYYYMMDD_NNN_description';
--
-- Then delete the migration file from db/migrations/
-- ═════════════════════════════════════════════════════════════════════════════

COMMIT;
