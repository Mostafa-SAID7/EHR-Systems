-- ═══════════════════════════════════════════════════════════════════════════════
-- EHR Platform - PostgreSQL Database Initialization
-- Purpose: Create one database per microservice for database-per-service pattern
-- Version: 1.0
-- Created: 2025-01-01
-- ═══════════════════════════════════════════════════════════════════════════════

-- Create user if not exists
DO
$do$
BEGIN
   IF NOT EXISTS (
      SELECT FROM pg_user
      WHERE usename = 'ehr_user'
   ) THEN
      CREATE USER ehr_user WITH PASSWORD 'ehr_password';
   END IF;
END
$do$;

-- ─────────────────────────────────────────────────────────────────────────────
-- Create Service-Specific Databases
-- ─────────────────────────────────────────────────────────────────────────────

-- Identity Service (PostgreSQL only)
CREATE DATABASE ehr_identity_db 
  OWNER ehr_user 
  ENCODING 'UTF8' 
  TEMPLATE template0;

-- Patient Service (PostgreSQL - master data)
CREATE DATABASE ehr_patient_db 
  OWNER ehr_user 
  ENCODING 'UTF8' 
  TEMPLATE template0;

-- Clinical Service (PostgreSQL - clinical data)
CREATE DATABASE ehr_clinical_db 
  OWNER ehr_user 
  ENCODING 'UTF8' 
  TEMPLATE template0;

-- Appointment Service (PostgreSQL + MySQL)
CREATE DATABASE ehr_appointment_db 
  OWNER ehr_user 
  ENCODING 'UTF8' 
  TEMPLATE template0;

-- Notification Service (PostgreSQL + MySQL + MongoDB)
CREATE DATABASE ehr_notification_db 
  OWNER ehr_user 
  ENCODING 'UTF8' 
  TEMPLATE template0;

-- Audit Service (PostgreSQL + MongoDB)
CREATE DATABASE ehr_audit_db 
  OWNER ehr_user 
  ENCODING 'UTF8' 
  TEMPLATE template0;

-- Billing Service (PostgreSQL + MySQL)
CREATE DATABASE ehr_billing_db 
  OWNER ehr_user 
  ENCODING 'UTF8' 
  TEMPLATE template0;

-- Prescription Service (PostgreSQL + MongoDB)
CREATE DATABASE ehr_prescription_db 
  OWNER ehr_user 
  ENCODING 'UTF8' 
  TEMPLATE template0;

-- Analytics Service (PostgreSQL + MySQL)
CREATE DATABASE ehr_analytics_db 
  OWNER ehr_user 
  ENCODING 'UTF8' 
  TEMPLATE template0;

-- Outbox Processor (All databases - event publisher)
CREATE DATABASE ehr_outbox_db 
  OWNER ehr_user 
  ENCODING 'UTF8' 
  TEMPLATE template0;

-- ─────────────────────────────────────────────────────────────────────────────
-- Grant Permissions
-- ─────────────────────────────────────────────────────────────────────────────

GRANT ALL PRIVILEGES ON DATABASE ehr_identity_db TO ehr_user;
GRANT ALL PRIVILEGES ON DATABASE ehr_patient_db TO ehr_user;
GRANT ALL PRIVILEGES ON DATABASE ehr_clinical_db TO ehr_user;
GRANT ALL PRIVILEGES ON DATABASE ehr_appointment_db TO ehr_user;
GRANT ALL PRIVILEGES ON DATABASE ehr_notification_db TO ehr_user;
GRANT ALL PRIVILEGES ON DATABASE ehr_audit_db TO ehr_user;
GRANT ALL PRIVILEGES ON DATABASE ehr_billing_db TO ehr_user;
GRANT ALL PRIVILEGES ON DATABASE ehr_prescription_db TO ehr_user;
GRANT ALL PRIVILEGES ON DATABASE ehr_analytics_db TO ehr_user;
GRANT ALL PRIVILEGES ON DATABASE ehr_outbox_db TO ehr_user;

-- ─────────────────────────────────────────────────────────────────────────────
-- Verify Creation
-- ─────────────────────────────────────────────────────────────────────────────

SELECT 'PostgreSQL initialization complete' as status;
SELECT datname FROM pg_database WHERE datname LIKE 'ehr_%' ORDER BY datname;

COMMIT;
