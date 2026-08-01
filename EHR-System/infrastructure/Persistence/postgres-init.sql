-- ═══════════════════════════════════════════════════════════════════════════════
-- EHR Platform - PostgreSQL Database Initialization
-- Purpose: Create databases for 7 new microservices (schema-per-service pattern)
-- Version: 2.0
-- Created: 2025-01-01
-- ═══════════════════════════════════════════════════════════════════════════════

-- Create user if not exists
DO
$do$
BEGIN
   IF NOT EXISTS (
      SELECT FROM pg_user
      WHERE usename = 'ehruser'
   ) THEN
      CREATE USER ehruser WITH PASSWORD 'ChangeMe123!';
   END IF;
END
$do$;

-- ─────────────────────────────────────────────────────────────────────────────
-- Create Databases for 7 Microservices
-- ─────────────────────────────────────────────────────────────────────────────

-- Identity Service Database
DO
$$
BEGIN
  IF NOT EXISTS(SELECT 1 FROM pg_database WHERE datname='ehr_identity') THEN
    CREATE DATABASE ehr_identity 
      OWNER ehruser 
      ENCODING 'UTF8' 
      TEMPLATE template0;
  END IF;
END
$$;

-- Patient Service Database
DO
$$
BEGIN
  IF NOT EXISTS(SELECT 1 FROM pg_database WHERE datname='ehr_patient') THEN
    CREATE DATABASE ehr_patient 
      OWNER ehruser 
      ENCODING 'UTF8' 
      TEMPLATE template0;
  END IF;
END
$$;

-- Appointment Service Database
DO
$$
BEGIN
  IF NOT EXISTS(SELECT 1 FROM pg_database WHERE datname='ehr_appointment') THEN
    CREATE DATABASE ehr_appointment 
      OWNER ehruser 
      ENCODING 'UTF8' 
      TEMPLATE template0;
  END IF;
END
$$;

-- Integration Service Database
DO
$$
BEGIN
  IF NOT EXISTS(SELECT 1 FROM pg_database WHERE datname='ehr_integration') THEN
    CREATE DATABASE ehr_integration 
      OWNER ehruser 
      ENCODING 'UTF8' 
      TEMPLATE template0;
  END IF;
END
$$;

-- Terminology Service Database
DO
$$
BEGIN
  IF NOT EXISTS(SELECT 1 FROM pg_database WHERE datname='ehr_terminology') THEN
    CREATE DATABASE ehr_terminology 
      OWNER ehruser 
      ENCODING 'UTF8' 
      TEMPLATE template0;
  END IF;
END
$$;

-- FileStorage Service Database
DO
$$
BEGIN
  IF NOT EXISTS(SELECT 1 FROM pg_database WHERE datname='ehr_filestorage') THEN
    CREATE DATABASE ehr_filestorage 
      OWNER ehruser 
      ENCODING 'UTF8' 
      TEMPLATE template0;
  END IF;
END
$$;

-- AI Service Database
DO
$$
BEGIN
  IF NOT EXISTS(SELECT 1 FROM pg_database WHERE datname='ehr_ai') THEN
    CREATE DATABASE ehr_ai 
      OWNER ehruser 
      ENCODING 'UTF8' 
      TEMPLATE template0;
  END IF;
END
$$;

-- ─────────────────────────────────────────────────────────────────────────────
-- Grant Permissions
-- ─────────────────────────────────────────────────────────────────────────────

GRANT ALL PRIVILEGES ON DATABASE ehr_identity TO ehruser;
GRANT ALL PRIVILEGES ON DATABASE ehr_patient TO ehruser;
GRANT ALL PRIVILEGES ON DATABASE ehr_appointment TO ehruser;
GRANT ALL PRIVILEGES ON DATABASE ehr_integration TO ehruser;
GRANT ALL PRIVILEGES ON DATABASE ehr_terminology TO ehruser;
GRANT ALL PRIVILEGES ON DATABASE ehr_filestorage TO ehruser;
GRANT ALL PRIVILEGES ON DATABASE ehr_ai TO ehruser;

-- ─────────────────────────────────────────────────────────────────────────────
-- Verify Creation
-- ─────────────────────────────────────────────────────────────────────────────

\c ehr_identity
CREATE SCHEMA IF NOT EXISTS identity;
GRANT ALL PRIVILEGES ON SCHEMA identity TO ehruser;

\c ehr_patient
CREATE SCHEMA IF NOT EXISTS patient;
GRANT ALL PRIVILEGES ON SCHEMA patient TO ehruser;

\c ehr_appointment
CREATE SCHEMA IF NOT EXISTS appointment;
GRANT ALL PRIVILEGES ON SCHEMA appointment TO ehruser;

\c ehr_integration
CREATE SCHEMA IF NOT EXISTS integration;
GRANT ALL PRIVILEGES ON SCHEMA integration TO ehruser;

\c ehr_terminology
CREATE SCHEMA IF NOT EXISTS terminology;
GRANT ALL PRIVILEGES ON SCHEMA terminology TO ehruser;

\c ehr_filestorage
CREATE SCHEMA IF NOT EXISTS filestorage;
GRANT ALL PRIVILEGES ON SCHEMA filestorage TO ehruser;

\c ehr_ai
CREATE SCHEMA IF NOT EXISTS ai;
GRANT ALL PRIVILEGES ON SCHEMA ai TO ehruser;

-- Return to postgres database
\c postgres

SELECT 'PostgreSQL initialization complete' as status;
SELECT datname FROM pg_database WHERE datname LIKE 'ehr_%' ORDER BY datname;
