-- ═══════════════════════════════════════════════════════════════════════════════
-- EHR Platform - MySQL Database Initialization
-- Purpose: Create service-specific databases for MySQL-using services
-- Version: 2.0 (Updated for 7 microservices)
-- Created: 2025-01-01
-- ═══════════════════════════════════════════════════════════════════════════════

-- Create ehr_user if not exists
CREATE USER IF NOT EXISTS 'ehruser'@'%' IDENTIFIED BY 'ChangeMe123!';

-- ─────────────────────────────────────────────────────────────────────────────
-- Create Service-Specific Databases (MySQL)
-- ─────────────────────────────────────────────────────────────────────────────

-- Appointment Service Database (MySQL + PostgreSQL hybrid)
CREATE DATABASE IF NOT EXISTS ehr_appointment_mysql 
  CHARACTER SET utf8mb4 
  COLLATE utf8mb4_unicode_ci;

-- Billing Service Database
CREATE DATABASE IF NOT EXISTS ehr_billing_mysql 
  CHARACTER SET utf8mb4 
  COLLATE utf8mb4_unicode_ci;

-- Notification Service Database (MySQL + MongoDB hybrid)
CREATE DATABASE IF NOT EXISTS ehr_notification_mysql 
  CHARACTER SET utf8mb4 
  COLLATE utf8mb4_unicode_ci;

-- Analytics Service Database
CREATE DATABASE IF NOT EXISTS ehr_analytics_mysql 
  CHARACTER SET utf8mb4 
  COLLATE utf8mb4_unicode_ci;

-- ─────────────────────────────────────────────────────────────────────────────
-- Grant Permissions to ehr_user
-- ─────────────────────────────────────────────────────────────────────────────

GRANT ALL PRIVILEGES ON ehr_appointment_mysql.* TO 'ehruser'@'%';
GRANT ALL PRIVILEGES ON ehr_billing_mysql.* TO 'ehruser'@'%';
GRANT ALL PRIVILEGES ON ehr_notification_mysql.* TO 'ehruser'@'%';
GRANT ALL PRIVILEGES ON ehr_analytics_mysql.* TO 'ehruser'@'%';

-- Permissions for management
GRANT ALL PRIVILEGES ON *.* TO 'ehruser'@'%' WITH GRANT OPTION;

FLUSH PRIVILEGES;

-- ─────────────────────────────────────────────────────────────────────────────
-- Use Appointment Database
-- ─────────────────────────────────────────────────────────────────────────────

USE ehr_appointment_mysql;

-- Appointment Slots Cache (for quick lookups)
CREATE TABLE IF NOT EXISTS `appointment_slots` (
  `id` BIGINT PRIMARY KEY AUTO_INCREMENT,
  `provider_id` VARCHAR(36) NOT NULL,
  `appointment_date` DATE NOT NULL,
  `start_time` TIME NOT NULL,
  `end_time` TIME NOT NULL,
  `status` ENUM('available', 'booked', 'blocked') NOT NULL DEFAULT 'available',
  `appointment_type` VARCHAR(100) NOT NULL,
  `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  `updated_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  UNIQUE KEY `unique_slot` (`provider_id`, `appointment_date`, `start_time`),
  INDEX `idx_provider_date` (`provider_id`, `appointment_date`),
  INDEX `idx_status_date` (`status`, `appointment_date`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Appointment Notifications Queue
CREATE TABLE IF NOT EXISTS `appointment_notifications` (
  `id` BIGINT PRIMARY KEY AUTO_INCREMENT,
  `appointment_id` VARCHAR(36) NOT NULL,
  `patient_id` VARCHAR(36) NOT NULL,
  `notification_type` ENUM('reminder', 'confirmation', 'cancellation', 'rescheduling') NOT NULL,
  `status` ENUM('pending', 'sent', 'failed') NOT NULL DEFAULT 'pending',
  `retry_count` INT DEFAULT 0,
  `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  `sent_at` TIMESTAMP NULL,
  INDEX `idx_appointment_id` (`appointment_id`),
  INDEX `idx_patient_id` (`patient_id`),
  INDEX `idx_status` (`status`),
  INDEX `idx_created_at` (`created_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Appointment Metrics (Analytics)
CREATE TABLE IF NOT EXISTS `appointment_metrics` (
  `id` BIGINT PRIMARY KEY AUTO_INCREMENT,
  `provider_id` VARCHAR(36) NOT NULL,
  `appointment_date` DATE NOT NULL,
  `total_appointments` INT DEFAULT 0,
  `completed_appointments` INT DEFAULT 0,
  `cancelled_appointments` INT DEFAULT 0,
  `no_show_appointments` INT DEFAULT 0,
  `average_duration_minutes` INT DEFAULT 0,
  `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  UNIQUE KEY `unique_metric` (`provider_id`, `appointment_date`),
  INDEX `idx_provider_date` (`provider_id`, `appointment_date`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ─────────────────────────────────────────────────────────────────────────────
-- Use Billing Database
-- ─────────────────────────────────────────────────────────────────────────────

USE ehr_billing_mysql;

-- Invoice Details (Denormalized for reporting)
CREATE TABLE IF NOT EXISTS `invoice_items` (
  `id` BIGINT PRIMARY KEY AUTO_INCREMENT,
  `invoice_id` VARCHAR(36) NOT NULL,
  `patient_id` VARCHAR(36) NOT NULL,
  `service_code` VARCHAR(50) NOT NULL,
  `service_description` VARCHAR(255) NOT NULL,
  `quantity` INT NOT NULL DEFAULT 1,
  `unit_price` DECIMAL(10, 2) NOT NULL,
  `total_price` DECIMAL(12, 2) NOT NULL,
  `tax_amount` DECIMAL(10, 2) DEFAULT 0.00,
  `discount_amount` DECIMAL(10, 2) DEFAULT 0.00,
  `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  INDEX `idx_invoice_id` (`invoice_id`),
  INDEX `idx_patient_id` (`patient_id`),
  INDEX `idx_service_code` (`service_code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Payment Transactions (Audit Trail)
CREATE TABLE IF NOT EXISTS `payment_transactions` (
  `id` BIGINT PRIMARY KEY AUTO_INCREMENT,
  `invoice_id` VARCHAR(36) NOT NULL,
  `patient_id` VARCHAR(36) NOT NULL,
  `amount` DECIMAL(12, 2) NOT NULL,
  `payment_method` ENUM('credit_card', 'debit_card', 'bank_transfer', 'insurance', 'cash') NOT NULL,
  `payment_status` ENUM('pending', 'success', 'failed', 'refunded') NOT NULL,
  `transaction_id` VARCHAR(100) UNIQUE NOT NULL,
  `gateway_response` JSON,
  `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  `processed_at` TIMESTAMP NULL,
  INDEX `idx_invoice_id` (`invoice_id`),
  INDEX `idx_patient_id` (`patient_id`),
  INDEX `idx_status` (`payment_status`),
  INDEX `idx_created_at` (`created_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Billing Reports (Pre-aggregated for performance)
CREATE TABLE IF NOT EXISTS `billing_reports` (
  `id` BIGINT PRIMARY KEY AUTO_INCREMENT,
  `report_date` DATE NOT NULL,
  `total_invoices` INT DEFAULT 0,
  `total_revenue` DECIMAL(15, 2) DEFAULT 0.00,
  `total_taxes` DECIMAL(15, 2) DEFAULT 0.00,
  `total_discounts` DECIMAL(15, 2) DEFAULT 0.00,
  `paid_amount` DECIMAL(15, 2) DEFAULT 0.00,
  `pending_amount` DECIMAL(15, 2) DEFAULT 0.00,
  `overdue_amount` DECIMAL(15, 2) DEFAULT 0.00,
  `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  UNIQUE KEY `unique_report_date` (`report_date`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ─────────────────────────────────────────────────────────────────────────────
-- Use Notification Database
-- ─────────────────────────────────────────────────────────────────────────────

USE ehr_notification_mysql;

-- Notification Preferences (Email, SMS, Push)
CREATE TABLE IF NOT EXISTS `notification_preferences` (
  `id` BIGINT PRIMARY KEY AUTO_INCREMENT,
  `user_id` VARCHAR(36) NOT NULL UNIQUE,
  `email_notifications` BOOLEAN DEFAULT TRUE,
  `sms_notifications` BOOLEAN DEFAULT TRUE,
  `push_notifications` BOOLEAN DEFAULT TRUE,
  `in_app_notifications` BOOLEAN DEFAULT TRUE,
  `appointment_reminders` BOOLEAN DEFAULT TRUE,
  `clinical_alerts` BOOLEAN DEFAULT TRUE,
  `billing_notifications` BOOLEAN DEFAULT TRUE,
  `system_notifications` BOOLEAN DEFAULT TRUE,
  `quiet_hours_start` TIME DEFAULT '22:00:00',
  `quiet_hours_end` TIME DEFAULT '08:00:00',
  `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  `updated_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  INDEX `idx_user_id` (`user_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Notification Delivery Log
CREATE TABLE IF NOT EXISTS `notification_deliveries` (
  `id` BIGINT PRIMARY KEY AUTO_INCREMENT,
  `notification_id` VARCHAR(36) NOT NULL,
  `user_id` VARCHAR(36) NOT NULL,
  `channel` ENUM('email', 'sms', 'push', 'in-app') NOT NULL,
  `status` ENUM('pending', 'sent', 'delivered', 'failed', 'bounced') NOT NULL,
  `recipient_address` VARCHAR(255),
  `error_message` TEXT,
  `retry_count` INT DEFAULT 0,
  `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  `delivered_at` TIMESTAMP NULL,
  INDEX `idx_notification_id` (`notification_id`),
  INDEX `idx_user_id` (`user_id`),
  INDEX `idx_status` (`status`),
  INDEX `idx_created_at` (`created_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ─────────────────────────────────────────────────────────────────────────────
-- Use Analytics Database
-- ─────────────────────────────────────────────────────────────────────────────

USE ehr_analytics_mysql;

-- Patient Visit Analytics
CREATE TABLE IF NOT EXISTS `patient_visit_analytics` (
  `id` BIGINT PRIMARY KEY AUTO_INCREMENT,
  `visit_date` DATE NOT NULL,
  `total_visits` INT DEFAULT 0,
  `unique_patients` INT DEFAULT 0,
  `average_visit_duration_minutes` INT DEFAULT 0,
  `completed_visits` INT DEFAULT 0,
  `cancelled_visits` INT DEFAULT 0,
  `no_show_visits` INT DEFAULT 0,
  `emergency_visits` INT DEFAULT 0,
  `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  UNIQUE KEY `unique_visit_date` (`visit_date`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Clinical Analytics
CREATE TABLE IF NOT EXISTS `clinical_analytics` (
  `id` BIGINT PRIMARY KEY AUTO_INCREMENT,
  `analytics_date` DATE NOT NULL,
  `total_diagnoses` INT DEFAULT 0,
  `total_prescriptions` INT DEFAULT 0,
  `total_lab_results` INT DEFAULT 0,
  `total_imaging_reports` INT DEFAULT 0,
  `average_treatment_days` INT DEFAULT 0,
  `recovery_rate_percentage` DECIMAL(5, 2) DEFAULT 0.00,
  `readmission_rate_percentage` DECIMAL(5, 2) DEFAULT 0.00,
  `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  UNIQUE KEY `unique_analytics_date` (`analytics_date`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- System Performance Metrics
CREATE TABLE IF NOT EXISTS `system_performance` (
  `id` BIGINT PRIMARY KEY AUTO_INCREMENT,
  `measurement_timestamp` TIMESTAMP NOT NULL,
  `service_name` VARCHAR(100) NOT NULL,
  `average_response_time_ms` INT DEFAULT 0,
  `p95_response_time_ms` INT DEFAULT 0,
  `p99_response_time_ms` INT DEFAULT 0,
  `error_rate_percentage` DECIMAL(5, 2) DEFAULT 0.00,
  `request_count` INT DEFAULT 0,
  `success_count` INT DEFAULT 0,
  `failed_count` INT DEFAULT 0,
  `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  INDEX `idx_service_timestamp` (`service_name`, `measurement_timestamp`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ─────────────────────────────────────────────────────────────────────────────
-- Create Views for Common Analytics Queries
-- ─────────────────────────────────────────────────────────────────────────────

USE ehr_analytics_mysql;

CREATE VIEW IF NOT EXISTS vw_daily_revenue AS
SELECT 
  DATE(created_at) as revenue_date,
  COUNT(DISTINCT invoice_id) as total_invoices,
  SUM(amount) as total_revenue
FROM (
  SELECT * FROM ehr_billing_mysql.payment_transactions
  WHERE payment_status = 'success'
) as successful_payments
GROUP BY DATE(created_at);

-- ─────────────────────────────────────────────────────────────────────────────
-- Enable Query Logging for Performance Monitoring
-- ─────────────────────────────────────────────────────────────────────────────

-- SET GLOBAL slow_query_log = 'ON';
-- SET GLOBAL long_query_time = 1;
-- SET GLOBAL log_queries_not_using_indexes = 'ON';

-- ─────────────────────────────────────────────────────────────────────────────
-- Verify Creation
-- ─────────────────────────────────────────────────────────────────────────────

SELECT 'MySQL initialization complete' as status;
SHOW DATABASES LIKE 'ehr_%';

-- Verify tables in each database
USE ehr_appointment_mysql;
SELECT COUNT(*) as appointment_tables FROM information_schema.TABLES WHERE TABLE_SCHEMA='ehr_appointment_mysql';

USE ehr_billing_mysql;
SELECT COUNT(*) as billing_tables FROM information_schema.TABLES WHERE TABLE_SCHEMA='ehr_billing_mysql';

USE ehr_notification_mysql;
SELECT COUNT(*) as notification_tables FROM information_schema.TABLES WHERE TABLE_SCHEMA='ehr_notification_mysql';

USE ehr_analytics_mysql;
SELECT COUNT(*) as analytics_tables FROM information_schema.TABLES WHERE TABLE_SCHEMA='ehr_analytics_mysql';

COMMIT;
