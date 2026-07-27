/**
 * ═══════════════════════════════════════════════════════════════════════════════
 * EHR Platform - MongoDB Baseline Migration
 * Version: 20250101_001
 * Created: 2025-01-01
 * Description: Create base collections and indexes for Patient service
 * ═══════════════════════════════════════════════════════════════════════════════
 */

// ─────────────────────────────────────────────────────────────────────────────
// CREATE CLINICAL DOCUMENTS COLLECTION
// ─────────────────────────────────────────────────────────────────────────────

db.createCollection("ClinicalDocuments", {
    validator: {
        $jsonSchema: {
            bsonType: "object",
            required: ["_id", "patientId", "documentType", "createdAt"],
            properties: {
                _id: { bsonType: "objectId" },
                patientId: { bsonType: "string", description: "UUID of patient" },
                documentType: {
                    enum: ["NoteText", "LabResult", "Imaging", "Prescription", "Vital"],
                    description: "Type of clinical document"
                },
                title: { bsonType: "string" },
                content: { bsonType: "string", description: "Document content or raw data" },
                metadata: {
                    bsonType: "object",
                    properties: {
                        tags: { bsonType: "array", items: { bsonType: "string" } },
                        confidential: { bsonType: "bool", default: false },
                        sourceSystem: { bsonType: "string" },
                        externalId: { bsonType: "string" }
                    }
                },
                createdBy: { bsonType: "string", description: "Provider/User ID" },
                createdAt: { bsonType: "date" },
                updatedAt: { bsonType: "date" },
                isDeleted: { bsonType: "bool", default: false }
            }
        }
    }
});

print("✅ Created collection: ClinicalDocuments");

// ─────────────────────────────────────────────────────────────────────────────
// CREATE INDEXES FOR CLINICAL DOCUMENTS
// ─────────────────────────────────────────────────────────────────────────────

db.ClinicalDocuments.createIndex({ patientId: 1 });
print("✅ Created index: patientId");

db.ClinicalDocuments.createIndex({ patientId: 1, documentType: 1, createdAt: -1 });
print("✅ Created index: patientId_documentType_createdAt");

db.ClinicalDocuments.createIndex({ createdAt: -1 });
print("✅ Created index: createdAt (for time-series queries)");

db.ClinicalDocuments.createIndex({ documentType: 1, isDeleted: 1 });
print("✅ Created index: documentType_isDeleted");

db.ClinicalDocuments.createIndex({ content: "text", "metadata.tags": "text" });
print("✅ Created index: text search");

// ─────────────────────────────────────────────────────────────────────────────
// CREATE PATIENT PREFERENCES COLLECTION
// ─────────────────────────────────────────────────────────────────────────────

db.createCollection("PatientPreferences", {
    validator: {
        $jsonSchema: {
            bsonType: "object",
            required: ["_id", "patientId"],
            properties: {
                _id: { bsonType: "objectId" },
                patientId: { bsonType: "string", description: "UUID of patient" },
                preferences: {
                    bsonType: "object",
                    properties: {
                        communicationMethod: { enum: ["Email", "SMS", "Phone", "InApp"] },
                        notifyAppointmentReminders: { bsonType: "bool", default: true },
                        notifyLabResults: { bsonType: "bool", default: true },
                        privacyLevel: { enum: ["Public", "Private", "Confidential"] },
                        language: { bsonType: "string", default: "en-US" }
                    }
                },
                createdAt: { bsonType: "date" },
                updatedAt: { bsonType: "date" }
            }
        }
    }
});

print("✅ Created collection: PatientPreferences");

db.PatientPreferences.createIndex({ patientId: 1 }, { unique: true });
print("✅ Created unique index: patientId");

// ─────────────────────────────────────────────────────────────────────────────
// CREATE AUDIT LOG COLLECTION
// ─────────────────────────────────────────────────────────────────────────────

db.createCollection("DocumentAuditLog", {
    validator: {
        $jsonSchema: {
            bsonType: "object",
            required: ["_id", "documentId", "action", "timestamp"],
            properties: {
                _id: { bsonType: "objectId" },
                documentId: { bsonType: "string", description: "ID of clinical document" },
                patientId: { bsonType: "string" },
                action: { enum: ["Created", "Updated", "Viewed", "Deleted", "Exported"] },
                performedBy: { bsonType: "string", description: "User ID" },
                changes: {
                    bsonType: "object",
                    properties: {
                        before: { bsonType: "object" },
                        after: { bsonType: "object" }
                    }
                },
                timestamp: { bsonType: "date" },
                ipAddress: { bsonType: "string" }
            }
        }
    }
});

print("✅ Created collection: DocumentAuditLog");

db.DocumentAuditLog.createIndex({ documentId: 1, timestamp: -1 });
print("✅ Created index: documentId_timestamp");

db.DocumentAuditLog.createIndex({ patientId: 1, timestamp: -1 });
print("✅ Created index: patientId_timestamp");

db.DocumentAuditLog.createIndex({ timestamp: -1 }, { expireAfterSeconds: 2592000 });
print("✅ Created TTL index: auto-delete audit logs after 30 days");

// ─────────────────────────────────────────────────────────────────────────────
// CREATE MIGRATION TRACKING COLLECTION
// ─────────────────────────────────────────────────────────────────────────────

db.createCollection("__MigrationHistory");

db.__MigrationHistory.createIndex({ migrationId: 1 }, { unique: true });
print("✅ Created collection: __MigrationHistory");

// ─────────────────────────────────────────────────────────────────────────────
// RECORD THIS MIGRATION
// ─────────────────────────────────────────────────────────────────────────────

db.__MigrationHistory.insertOne({
    migrationId: "20250101_001_baseline",
    appliedAt: new Date(),
    productVersion: "1.0.0",
    collections: ["ClinicalDocuments", "PatientPreferences", "DocumentAuditLog"],
    status: "success"
});

print("✅ Recorded migration: 20250101_001_baseline");
print("✅ MongoDB baseline migration complete!");
