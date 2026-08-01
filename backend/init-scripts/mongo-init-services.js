// ═══════════════════════════════════════════════════════════════════════════════
// EHR Platform - MongoDB Initialization
// Purpose: Create service-specific databases and collections for document services
// Version: 1.0
// Created: 2025-01-01
// ═══════════════════════════════════════════════════════════════════════════════

// ─────────────────────────────────────────────────────────────────────────────
// Patient Service Database - Document Store
// ─────────────────────────────────────────────────────────────────────────────

db = db.getSiblingDB('ehr_patient_documents');

db.createCollection('PatientPreferences', {
  validator: {
    $jsonSchema: {
      bsonType: "object",
      required: ["patientId"],
      properties: {
        patientId: { 
          bsonType: "string",
          description: "Reference to patient in PostgreSQL"
        },
        preferences: { 
          bsonType: "object",
          description: "User preferences (flexible schema)"
        },
        language: { bsonType: "string", default: "en" },
        timezone: { bsonType: "string" },
        createdAt: { bsonType: "date" },
        updatedAt: { bsonType: "date" }
      }
    }
  }
});

db.PatientPreferences.createIndex({ patientId: 1 }, { unique: true });
print("✅ Patient Service: ehr_patient_documents database created");

// ─────────────────────────────────────────────────────────────────────────────
// Clinical Service Database - Clinical Documents
// ─────────────────────────────────────────────────────────────────────────────

db = db.getSiblingDB('ehr_clinical_documents');

db.createCollection('ClinicalDocuments', {
  validator: {
    $jsonSchema: {
      bsonType: "object",
      required: ["patientId", "documentType", "content"],
      properties: {
        patientId: { bsonType: "string" },
        documentType: { 
          bsonType: "string",
          enum: ["progress_note", "discharge_summary", "lab_result", "imaging_report"]
        },
        content: { bsonType: "string" },
        providerName: { bsonType: "string" },
        createdAt: { bsonType: "date" },
        updatedAt: { bsonType: "date" },
        tags: { bsonType: "array", items: { bsonType: "string" } }
      }
    }
  }
});

db.ClinicalDocuments.createIndex({ patientId: 1 });
db.ClinicalDocuments.createIndex({ patientId: 1, documentType: 1, createdAt: -1 });
db.ClinicalDocuments.createIndex({ content: "text" }, { name: "clinical_full_text" });
print("✅ Clinical Service: ehr_clinical_documents database created");

// ─────────────────────────────────────────────────────────────────────────────
// Notification Service Database - Notification Queue & Templates
// ─────────────────────────────────────────────────────────────────────────────

db = db.getSiblingDB('ehr_notification_documents');

db.createCollection('NotificationQueue', {
  validator: {
    $jsonSchema: {
      bsonType: "object",
      required: ["userId", "type", "message"],
      properties: {
        userId: { bsonType: "string" },
        type: { 
          bsonType: "string",
          enum: ["email", "sms", "push"]
        },
        message: { bsonType: "string" },
        status: { 
          bsonType: "string",
          enum: ["pending", "sent", "failed"],
          default: "pending"
        },
        retryCount: { bsonType: "int", default: 0 },
        createdAt: { bsonType: "date" },
        sentAt: { bsonType: "date" }
      }
    }
  }
});

// TTL index: Auto-delete after 90 days
db.NotificationQueue.createIndex({ createdAt: 1 }, { expireAfterSeconds: 7776000 });
db.NotificationQueue.createIndex({ userId: 1, status: 1 });

db.createCollection('NotificationTemplates');
db.NotificationTemplates.createIndex({ type: 1, language: 1 });

print("✅ Notification Service: ehr_notification_documents database created");

// ─────────────────────────────────────────────────────────────────────────────
// Audit Service Database - HIPAA Audit Logs
// ─────────────────────────────────────────────────────────────────────────────

db = db.getSiblingDB('ehr_audit_documents');

db.createCollection('AuditLogs', {
  validator: {
    $jsonSchema: {
      bsonType: "object",
      required: ["userId", "action", "resourceType", "timestamp"],
      properties: {
        userId: { bsonType: "string" },
        action: { 
          bsonType: "string",
          enum: ["READ", "CREATE", "UPDATE", "DELETE"]
        },
        resourceType: { bsonType: "string" },
        resourceId: { bsonType: "string" },
        oldValue: { bsonType: "object" },
        newValue: { bsonType: "object" },
        ipAddress: { bsonType: "string" },
        timestamp: { bsonType: "date" }
      }
    }
  }
});

// TTL index: HIPAA requires 7 years retention (252 months = 7 years in seconds)
db.AuditLogs.createIndex({ timestamp: 1 }, { expireAfterSeconds: 220752000 });
db.AuditLogs.createIndex({ userId: 1, timestamp: -1 });
db.AuditLogs.createIndex({ resourceType: 1, resourceId: 1 });

print("✅ Audit Service: ehr_audit_documents database created");

// ─────────────────────────────────────────────────────────────────────────────
// Prescription Service Database - Medication History
// ─────────────────────────────────────────────────────────────────────────────

db = db.getSiblingDB('ehr_prescription_documents');

db.createCollection('MedicationHistory', {
  validator: {
    $jsonSchema: {
      bsonType: "object",
      required: ["patientId", "medicationName"],
      properties: {
        patientId: { bsonType: "string" },
        medicationName: { bsonType: "string" },
        dosage: { bsonType: "string" },
        frequency: { bsonType: "string" },
        startDate: { bsonType: "date" },
        endDate: { bsonType: "date" },
        prescribedBy: { bsonType: "string" },
        createdAt: { bsonType: "date" }
      }
    }
  }
});

db.MedicationHistory.createIndex({ patientId: 1 });
db.MedicationHistory.createIndex({ patientId: 1, startDate: -1 });

print("✅ Prescription Service: ehr_prescription_documents database created");

// ─────────────────────────────────────────────────────────────────────────────
// Outbox Processor Database - Cross-Service Events
// ─────────────────────────────────────────────────────────────────────────────

db = db.getSiblingDB('ehr_outbox_documents');

db.createCollection('OutboxEvents', {
  validator: {
    $jsonSchema: {
      bsonType: "object",
      required: ["eventType", "aggregateId"],
      properties: {
        eventType: { bsonType: "string" },
        eventData: { bsonType: "object" },
        aggregateId: { bsonType: "string" },
        isPublished: { bsonType: "bool", default: false },
        createdAt: { bsonType: "date" },
        publishedAt: { bsonType: "date" }
      }
    }
  }
});

db.OutboxEvents.createIndex({ isPublished: 1, createdAt: 1 });

print("✅ Outbox Processor: ehr_outbox_documents database created");

// ─────────────────────────────────────────────────────────────────────────────
// Summary
// ─────────────────────────────────────────────────────────────────────────────

print("\n════════════════════════════════════════════════════════════════");
print("✅ MongoDB Initialization Complete");
print("════════════════════════════════════════════════════════════════");

const databases = db.adminCommand('listDatabases');
const ehrDatabases = databases.databases.filter(d => d.name.startsWith('ehr_'));
print(`Total EHR databases created: ${ehrDatabases.length}`);
ehrDatabases.forEach(db => print(`  - ${db.name}`));

print("\n📋 Service-Database Mapping:");
print("  Patient Service:      ehr_patient_documents");
print("  Clinical Service:     ehr_clinical_documents");
print("  Notification Service: ehr_notification_documents");
print("  Audit Service:        ehr_audit_documents");
print("  Prescription Service: ehr_prescription_documents");
print("  Outbox Processor:     ehr_outbox_documents");
print("\n✅ All services ready for deployment");
