/**
 * ═══════════════════════════════════════════════════════════════════════════════
 * EHR Platform - MongoDB Migration Template
 * Version: YYYYMMDD_NNN
 * Created: YYYY-MM-DD
 * Description: [Brief description of changes]
 * Author: [Your Name]
 * ═══════════════════════════════════════════════════════════════════════════════
 * 
 * MIGRATION GUIDELINES:
 * 1. Version Format: YYYYMMDD_NNN (e.g., 20250115_001)
 * 2. NNN: Sequential number (001, 002, 003, etc. per day)
 * 3. Use db.collection.updateMany() for bulk operations (not forEach loops)
 * 4. Always use upsert for idempotency: { upsert: true }
 * 5. Create indexes after schema changes
 * 6. Include rollback information at the end
 * 7. Test migrations on development first
 * 8. Use transactions when modifying multiple collections
 * ═══════════════════════════════════════════════════════════════════════════════
 */

// ─────────────────────────────────────────────────────────────────────────────
// COLLECTION CREATION EXAMPLE
// ─────────────────────────────────────────────────────────────────────────────

// Create collection with validation schema
db.createCollection("ClinicalDocuments", {
    validator: {
        $jsonSchema: {
            bsonType: "object",
            required: ["_id", "patientId", "documentType", "createdAt"],
            properties: {
                _id: { bsonType: "objectId" },
                patientId: { bsonType: "uuid", description: "Reference to patient" },
                documentType: { 
                    enum: ["NoteText", "LabResult", "Imaging", "Prescription"],
                    description: "Type of clinical document"
                },
                content: { bsonType: "string" },
                metadata: {
                    bsonType: "object",
                    properties: {
                        tags: { bsonType: "array", items: { bsonType: "string" } },
                        confidential: { bsonType: "bool" }
                    }
                },
                createdAt: { bsonType: "date" },
                updatedAt: { bsonType: "date" },
                isDeleted: { bsonType: "bool", default: false }
            }
        }
    }
});

// ─────────────────────────────────────────────────────────────────────────────
// INDEX CREATION EXAMPLE
// ─────────────────────────────────────────────────────────────────────────────

// Create single field index
db.ClinicalDocuments.createIndex({ patientId: 1 });

// Create compound index
db.ClinicalDocuments.createIndex({ patientId: 1, documentType: 1, createdAt: -1 });

// Create unique index
db.ClinicalDocuments.createIndex({ referenceNumber: 1 }, { unique: true });

// Create text index for full-text search
db.ClinicalDocuments.createIndex({ content: "text", "metadata.tags": "text" });

// ─────────────────────────────────────────────────────────────────────────────
// DATA TRANSFORMATION EXAMPLE
// ─────────────────────────────────────────────────────────────────────────────

// Add new field to all documents
db.ClinicalDocuments.updateMany(
    {},
    { $set: { version: 1, lastReviewedAt: null } },
    { upsert: false }
);

// Rename field across all documents
db.ClinicalDocuments.updateMany(
    {},
    { $rename: { "oldFieldName": "newFieldName" } }
);

// Convert field type (string to ObjectId)
db.ClinicalDocuments.updateMany(
    { departmentId: { $type: "string" } },
    [
        {
            $set: {
                departmentId: { $toObjectId: "$departmentId" }
            }
        }
    ]
);

// ─────────────────────────────────────────────────────────────────────────────
// BULK OPERATIONS EXAMPLE
// ─────────────────────────────────────────────────────────────────────────────

// Use bulkWrite for multiple operations
db.ClinicalDocuments.bulkWrite([
    {
        updateMany: {
            filter: { status: "draft" },
            update: { $set: { status: "inactive" } }
        }
    },
    {
        insertOne: {
            document: {
                patientId: "uuid",
                documentType: "NoteText",
                content: "Migration audit record",
                createdAt: new Date()
            }
        }
    }
]);

// ─────────────────────────────────────────────────────────────────────────────
// TRANSACTION EXAMPLE (if modifying multiple collections)
// ─────────────────────────────────────────────────────────────────────────────

var session = db.getMongo().startSession();

try {
    session.startTransaction();

    db.ClinicalDocuments.updateMany(
        { status: "pending" },
        { $set: { status: "processing" } }
    );

    db.AuditLog.insertOne({
        action: "bulk_status_update",
        timestamp: new Date(),
        count: db.ClinicalDocuments.countDocuments({ status: "processing" })
    });

    session.commitTransaction();
} catch (error) {
    session.abortTransaction();
    throw error;
} finally {
    session.endSession();
}

// ─────────────────────────────────────────────────────────────────────────────
// MIGRATION TRACKING
// ─────────────────────────────────────────────────────────────────────────────

// Record this migration in history collection
db.__MigrationHistory.insertOne({
    migrationId: "YYYYMMDD_NNN_description",
    appliedAt: new Date(),
    productVersion: "1.0.0"
});

// ═════════════════════════════════════════════════════════════════════════════
// ROLLBACK PROCEDURE
// ═════════════════════════════════════════════════════════════════════════════
// In case of emergency, manually execute the reverse operations:
//
// db.ClinicalDocuments.dropIndex("patientId_1");
// db.ClinicalDocuments.updateMany(
//     {},
//     { $unset: { newField: "" } }
// );
// db.__MigrationHistory.deleteOne({ migrationId: "YYYYMMDD_NNN_description" });
//
// Then delete the migration file from db/migrations/mongo/
// ═════════════════════════════════════════════════════════════════════════════
