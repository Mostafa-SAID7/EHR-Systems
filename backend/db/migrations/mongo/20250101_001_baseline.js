/**
 * ═══════════════════════════════════════════════════════════════════════════════
 * EHR Platform - MongoDB Baseline Migration
 * Version: 20250101_001
 * Created: 2025-01-01
 * Description: Create base collections and indexes for all microservices
 * ═══════════════════════════════════════════════════════════════════════════════
 */

// ─────────────────────────────────────────────────────────────────────────────
// MIGRATION TRACKING COLLECTION
// ─────────────────────────────────────────────────────────────────────────────

db.createCollection("__MigrationHistory", {
    validator: {
        $jsonSchema: {
            bsonType: "object",
            required: ["_id", "migrationId", "appliedAt"],
            properties: {
                _id: { bsonType: "objectId" },
                migrationId: { bsonType: "string", description: "Migration version ID" },
                productVersion: { bsonType: "string" },
                appliedAt: { bsonType: "date" }
            }
        }
    }
});

db.__MigrationHistory.createIndex({ migrationId: 1 }, { unique: true });
db.__MigrationHistory.createIndex({ appliedAt: -1 });

db.__MigrationHistory.insertOne({
    migrationId: "20250101_001_baseline",
    productVersion: "8.0.0",
    appliedAt: new Date()
});

print("✅ Created collection: __MigrationHistory");

// ─────────────────────────────────────────────────────────────────────────────
// PATIENT SERVICE - CLINICAL DOCUMENTS (Document Store)
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

db.ClinicalDocuments.createIndex({ patientId: 1, createdAt: -1 });
db.ClinicalDocuments.createIndex({ documentType: 1 });
db.ClinicalDocuments.createIndex({ createdBy: 1 });
db.ClinicalDocuments.createIndex({ "metadata.tags": 1 });
db.ClinicalDocuments.createIndex({ isDeleted: 1, patientId: 1 });

print("✅ Created collection: ClinicalDocuments");

// ─────────────────────────────────────────────────────────────────────────────
// AUDIT SERVICE - AUDIT LOGS (Document Store)
// ─────────────────────────────────────────────────────────────────────────────

db.createCollection("AuditLogs", {
    validator: {
        $jsonSchema: {
            bsonType: "object",
            required: ["_id", "userId", "action", "timestamp"],
            properties: {
                _id: { bsonType: "objectId" },
                userId: { bsonType: "string" },
                action: { bsonType: "string", description: "Action performed (Create, Read, Update, Delete)" },
                resourceType: { bsonType: "string" },
                resourceId: { bsonType: "string" },
                oldValue: { bsonType: "object" },
                newValue: { bsonType: "object" },
                ipAddress: { bsonType: "string" },
                userAgent: { bsonType: "string" },
                timestamp: { bsonType: "date" },
                status: { enum: ["Success", "Failure"], description: "Action result" },
                errorMessage: { bsonType: "string" }
            }
        }
    }
});

db.AuditLogs.createIndex({ userId: 1, timestamp: -1 });
db.AuditLogs.createIndex({ resourceType: 1, resourceId: 1 });
db.AuditLogs.createIndex({ action: 1 });
db.AuditLogs.createIndex({ timestamp: -1 });
db.AuditLogs.createIndex({ userId: 1, action: 1, timestamp: -1 });

print("✅ Created collection: AuditLogs");

// ─────────────────────────────────────────────────────────────────────────────
// NOTIFICATION SERVICE - NOTIFICATION QUEUE (Document Store)
// ─────────────────────────────────────────────────────────────────────────────

db.createCollection("NotificationQueue", {
    validator: {
        $jsonSchema: {
            bsonType: "object",
            required: ["_id", "userId", "message", "createdAt"],
            properties: {
                _id: { bsonType: "objectId" },
                userId: { bsonType: "string" },
                type: { enum: ["Email", "SMS", "Push", "InApp"], default: "InApp" },
                subject: { bsonType: "string" },
                message: { bsonType: "string" },
                status: { enum: ["Pending", "Sent", "Failed", "Archived"], default: "Pending" },
                priority: { enum: ["Low", "Normal", "High", "Critical"], default: "Normal" },
                recipientEmail: { bsonType: "string" },
                recipientPhone: { bsonType: "string" },
                sentAt: { bsonType: "date" },
                failureReason: { bsonType: "string" },
                retryCount: { bsonType: "int", default: 0 },
                maxRetries: { bsonType: "int", default: 3 },
                nextRetryAt: { bsonType: "date" },
                createdAt: { bsonType: "date" },
                updatedAt: { bsonType: "date" }
            }
        }
    }
});

db.NotificationQueue.createIndex({ userId: 1, createdAt: -1 });
db.NotificationQueue.createIndex({ status: 1, nextRetryAt: 1 });
db.NotificationQueue.createIndex({ type: 1 });
db.NotificationQueue.createIndex({ priority: 1, status: 1 });
db.NotificationQueue.createIndex({ createdAt: -1 }, { expireAfterSeconds: 7776000 }); // 90 days TTL

print("✅ Created collection: NotificationQueue");

// ─────────────────────────────────────────────────────────────────────────────
// PRESCRIPTION SERVICE - MEDICATION HISTORY (Document Store)
// ─────────────────────────────────────────────────────────────────────────────

db.createCollection("MedicationHistory", {
    validator: {
        $jsonSchema: {
            bsonType: "object",
            required: ["_id", "patientId", "medicationName", "startDate"],
            properties: {
                _id: { bsonType: "objectId" },
                patientId: { bsonType: "string" },
                medicationName: { bsonType: "string" },
                dosage: { bsonType: "string" },
                frequency: { bsonType: "string" },
                route: { enum: ["Oral", "IV", "IM", "SC", "Transdermal", "Inhalation"], default: "Oral" },
                startDate: { bsonType: "date" },
                endDate: { bsonType: "date" },
                indication: { bsonType: "string", description: "Reason for medication" },
                prescribedBy: { bsonType: "string" },
                status: { enum: ["Active", "Completed", "Discontinued"], default: "Active" },
                allergies: { bsonType: "array", items: { bsonType: "string" } },
                sideEffects: { bsonType: "array", items: { bsonType: "string" } },
                notes: { bsonType: "string" },
                createdAt: { bsonType: "date" },
                updatedAt: { bsonType: "date" }
            }
        }
    }
});

db.MedicationHistory.createIndex({ patientId: 1, startDate: -1 });
db.MedicationHistory.createIndex({ status: 1 });
db.MedicationHistory.createIndex({ medicationName: 1 });
db.MedicationHistory.createIndex({ patientId: 1, status: 1 });

print("✅ Created collection: MedicationHistory");

// ─────────────────────────────────────────────────────────────────────────────
// ANALYTICS SERVICE - ANALYTICS EVENTS (Event Stream)
// ─────────────────────────────────────────────────────────────────────────────

db.createCollection("AnalyticsEvents", {
    validator: {
        $jsonSchema: {
            bsonType: "object",
            required: ["_id", "eventType", "timestamp"],
            properties: {
                _id: { bsonType: "objectId" },
                eventType: { bsonType: "string", description: "Type of analytics event" },
                userId: { bsonType: "string" },
                resourceType: { bsonType: "string" },
                resourceId: { bsonType: "string" },
                action: { bsonType: "string" },
                metadata: { bsonType: "object" },
                timestamp: { bsonType: "date" },
                duration: { bsonType: "int", description: "Duration in ms" },
                status: { bsonType: "string" },
                tags: { bsonType: "array", items: { bsonType: "string" } }
            }
        }
    }
});

db.AnalyticsEvents.createIndex({ eventType: 1, timestamp: -1 });
db.AnalyticsEvents.createIndex({ userId: 1, timestamp: -1 });
db.AnalyticsEvents.createIndex({ timestamp: -1 });
db.AnalyticsEvents.createIndex({ resourceType: 1, resourceId: 1 });
db.AnalyticsEvents.createIndex({ timestamp: 1 }, { expireAfterSeconds: 2592000 }); // 30 days TTL

print("✅ Created collection: AnalyticsEvents");

// ─────────────────────────────────────────────────────────────────────────────
// COMMON - OUTBOX EVENTS (Event Publishing Pattern)
// ─────────────────────────────────────────────────────────────────────────────

db.createCollection("OutboxEvents", {
    validator: {
        $jsonSchema: {
            bsonType: "object",
            required: ["_id", "eventType", "eventData", "createdAt"],
            properties: {
                _id: { bsonType: "objectId" },
                eventType: { bsonType: "string" },
                eventData: { bsonType: "object" },
                aggregateId: { bsonType: "string" },
                transport: { enum: ["Kafka", "RabbitMQ", "AzureBus"], default: "Kafka" },
                routingKey: { bsonType: "string" },
                createdAt: { bsonType: "date" },
                isPublished: { bsonType: "bool", default: false },
                publishedAt: { bsonType: "date" },
                publishAttempts: { bsonType: "int", default: 0 },
                maxPublishAttempts: { bsonType: "int", default: 3 },
                errorMessage: { bsonType: "string" }
            }
        }
    }
});

db.OutboxEvents.createIndex({ isPublished: 1, createdAt: 1 });
db.OutboxEvents.createIndex({ eventType: 1 });
db.OutboxEvents.createIndex({ aggregateId: 1 });
db.OutboxEvents.createIndex({ createdAt: -1 });
db.OutboxEvents.createIndex({ isPublished: 1, publishAttempts: 1 });

print("✅ Created collection: OutboxEvents");

print("\n═══════════════════════════════════════════════════════════════════════════════");
print("✅ MongoDB Baseline Migration Complete - All collections and indexes created");
print("═══════════════════════════════════════════════════════════════════════════════");
