-- ═══════════════════════════════════════════════════════════════════════════════
-- EHR Platform - MySQL Database Initialization
-- Purpose: Create service-specific databases for MySQL-using services
-- Version: 1.0
-- Created: 2025-01-01
-- ═══════════════════════════════════════════════════════════════════════════════

-- Create user if not exists
CREATE USER IF NOT EXISTS 'ehr_user'@'%' IDENTIFIED BY 'ehr_password';

-- ─────────────────────────────────────────────────────────────────────────────
-- Create Service-Specific Databases (MySQL)
-- ─────────────────────────────────────────────────────────────────────────────

-- Appointment Service Database
CREATE DATABASE IF NOT EXISTS ehr_appointment_db 
  CHARACTER SET utf8mb4 
  COLLATE utf8mb4_unicode_ci;

-- Notification Service Database
CREATE DATABASE IF NOT EXISTS ehr_notification_db 
  CHARACTER SET utf8mb4 
  COLLATE utf8mb4_unicode_ci;

-- Billing Service Database
CREATE DATABASE IF NOT EXISTS ehr_billing_db 
  CHARACTER SET utf8mb4 
  COLLATE utf8mb4_unicode_ci;

-- Analytics Service Database
CREATE DATABASE IF NOT EXISTS ehr_analytics_db 
  CHARACTER SET utf8mb4 
  COLLATE utf8mb4_unicode_ci;

-- Outbox Processor Database
CREATE DATABASE IF NOT EXISTS ehr_outbox_db 
  CHARACTER SET utf8mb4 
  COLLATE utf8mb4_unicode_ci;

-- ─────────────────────────────────────────────────────────────────────────────
-- Grant Permissions
-- ─────────────────────────────────────────────────────────────────────────────

GRANT ALL PRIVILEGES ON ehr_appointment_db.* TO 'ehr_user'@'%';
GRANT ALL PRIVILEGES ON ehr_notification_db.* TO 'ehr_user'@'%';
GRANT ALL PRIVILEGES ON ehr_billing_db.* TO 'ehr_user'@'%';
GRANT ALL PRIVILEGES ON ehr_analytics_db.* TO 'ehr_user'@'%';
GRANT ALL PRIVILEGES ON ehr_outbox_db.* TO 'ehr_user'@'%';

-- Permissions for root from localhost (for management)
GRANT ALL PRIVILEGES ON *.* TO 'ehr_user'@'%' WITH GRANT OPTION;

FLUSH PRIVILEGES;

-- ─────────────────────────────────────────────────────────────────────────────
-- Verify Creation
-- ─────────────────────────────────────────────────────────────────────────────

SELECT 'MySQL initialization complete' as status;
SHOW DATABASES LIKE 'ehr_%';

COMMIT;
