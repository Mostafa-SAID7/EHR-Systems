// ═══════════════════════════════════════════════════════════════════════════════
// EHR Platform - MongoDB Initialization
// Purpose: Create service-specific databases and collections for document services
// Version: 2.0 (Updated for 7 microservices + 2 gateways)
// Created: 2025-01-01
// ═══════════════════════════════════════════════════════════════════════════════

// Authenticate as admin
const adminDb = db.getSiblingDB('admin');
adminDb.auth('ehr_admin', process.env.MONGO_ADMIN_PASSWORD || 'ehr_mongo_password');

// ─────────────────────────────────────────────────────────────────────────────
// Patient Service Database - Document Store (Flexible Schema)
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
        communicationPreferences: { bsonType: "object" },
        createdAt: { bsonType: "date" },
        updatedAt: { bsonType: "date" }
      }
    }
  }
});

db.PatientPreferences.createIndex({ patientId: 1 }, { unique: true });
db.PatientPreferences.createIndex({ createdAt: -1 });

print("✅ Patient Service: ehr_patient_documents database created");

// ─────────────────────────────────────────────────────────────────────────────
// Clinical Service Database - Clinical Documents & Progress Notes
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
          enum: ["progress_note", "discharge_summary", "lab_result", "imaging_report", "consultation_note"]
        },
        content: { bsonType: "string" },
        providerName: { bsonType: "string" },
        providerId: { bsonType: "string" },
        departmentId: { bsonType: "string" },
        createdAt: { bsonType: "date" },
        updatedAt: { bsonType: "date" },
        tags: { bsonType: "array", items: { bsonType: "string" } },
        attachments: { 
          bsonType: "array",
          items: { 
            bsonType: "object",
            properties: {
              fileId: { bsonType: "string" },
              fileName: { bsonType: "string" },
              contentType: { bsonType: "string" }
            }
          }
        }
      }
    }
  }
});

db.ClinicalDocuments.createIndex({ patientId: 1 });
db.ClinicalDocuments.createIndex({ patientId: 1, documentType: 1, createdAt: -1 });
db.ClinicalDocuments.createIndex({ content: "text" }, { name: "clinical_full_text", sparse: true });
db.ClinicalDocuments.createIndex({ providerId: 1, createdAt: -1 });

// Progress Notes Collection
db.createCollection('ProgressNotes', {
  validator: {
    $jsonSchema: {
      bsonType: "object",
      required: ["patientId", "encounterId", "note"],
      properties: {
        patientId: { bsonType: "string" },
        encounterId: { bsonType: "string" },
        note: { bsonType: "string" },
        createdBy: { bsonType: "string" },
        createdAt: { bsonType: "date" },
        updatedAt: { bsonType: "date" }
      }
    }
  }
});

db.ProgressNotes.createIndex({ patientId: 1, encounterId: 1 });
db.ProgressNotes.createIndex({ createdAt: -1 });

print("✅ Clinical Service: ehr_clinical_documents database created");

// ─────────────────────────────────────────────────────────────────────────────
// Appointment Service Database - Appointment History & Metadata
// ─────────────────────────────────────────────────────────────────────────────

db = db.getSiblingDB('ehr_appointment_documents');

db.createCollection('AppointmentHistory', {
  validator: {
    $jsonSchema: {
      bsonType: "object",
      required: ["appointmentId", "patientId"],
      properties: {
        appointmentId: { bsonType: "string" },
        patientId: { bsonType: "string" },
        providerId: { bsonType: "string" },
        appointmentType: { bsonType: "string" },
        status: { bsonType: "string" },
        notes: { bsonType: "string" },
        createdAt: { bsonType: "date" },
        updatedAt: { bsonType: "date" },
        visitSummary: { bsonType: "object" }
      }
    }
  }
});

db.AppointmentHistory.createIndex({ appointmentId: 1 }, { unique: true });
db.AppointmentHistory.createIndex({ patientId: 1, createdAt: -1 });
db.AppointmentHistory.createIndex({ providerId: 1, createdAt: -1 });

print("✅ Appointment Service: ehr_appointment_documents database created");

// ─────────────────────────────────────────────────────────────────────────────
// Integration Service Database - External API Logs & Sync State
// ─────────────────────────────────────────────────────────────────────────────

db = db.getSiblingDB('ehr_integration_documents');

db.createCollection('IntegrationLogs', {
  validator: {
    $jsonSchema: {
      bsonType: "object",
      required: ["integrationType", "externalSystem"],
      properties: {
        integrationType: { 
          bsonType: "string",
          enum: ["HL7", "FHIR", "NPHIES", "PaymentGateway", "GovernmentAPI"]
        },
        externalSystem: { bsonType: "string" },
        request: { bsonType: "object" },
        response: { bsonType: "object" },
        status: { 
          bsonType: "string",
          enum: ["success", "failed", "pending"]
        },
        errorMessage: { bsonType: "string" },
        createdAt: { bsonType: "date" },
        processingTime: { bsonType: "int" }
      }
    }
  }
});

db.IntegrationLogs.createIndex({ integrationType: 1, externalSystem: 1, createdAt: -1 });
db.IntegrationLogs.createIndex({ status: 1, createdAt: -1 });
// TTL: Keep logs for 90 days
db.IntegrationLogs.createIndex({ createdAt: 1 }, { expireAfterSeconds: 7776000 });

// Sync State Collection
db.createCollection('SyncState', {
  validator: {
    $jsonSchema: {
      bsonType: "object",
      required: ["resourceType", "externalSystem"],
      properties: {
        resourceType: { bsonType: "string" },
        resourceId: { bsonType: "string" },
        externalSystem: { bsonType: "string" },
        externalId: { bsonType: "string" },
        lastSyncedAt: { bsonType: "date" },
        syncStatus: { bsonType: "string" },
        failureReason: { bsonType: "string" }
      }
    }
  }
});

db.SyncState.createIndex({ resourceType: 1, resourceId: 1, externalSystem: 1 }, { unique: true });

print("✅ Integration Service: ehr_integration_documents database created");

// ─────────────────────────────────────────────────────────────────────────────
// Terminology Service Database - Medical Code Mappings
// ─────────────────────────────────────────────────────────────────────────────

db = db.getSiblingDB('ehr_terminology_documents');

db.createCollection('CodeMappings', {
  validator: {
    $jsonSchema: {
      bsonType: "object",
      required: ["sourceCodeSystem", "sourceCode"],
      properties: {
        sourceCodeSystem: { 
          bsonType: "string",
          enum: ["ICD10", "SNOMED", "LOINC", "CPT", "RXNorm"]
        },
        sourceCode: { bsonType: "string" },
        sourceDescription: { bsonType: "string" },
        targetCodeSystem: { bsonType: "string" },
        targetCode: { bsonType: "string" },
        targetDescription: { bsonType: "string" },
        mappingType: { 
          bsonType: "string",
          enum: ["exact", "approximate", "partial"]
        },
        confidence: { bsonType: "double" },
        lastUpdated: { bsonType: "date" }
      }
    }
  }
});

db.CodeMappings.createIndex({ sourceCodeSystem: 1, sourceCode: 1 });
db.CodeMappings.createIndex({ targetCodeSystem: 1, targetCode: 1 });
db.CodeMappings.createIndex({ mappingType: 1 });

// Code Descriptions Collection
db.createCollection('CodeDescriptions', {
  validator: {
    $jsonSchema: {
      bsonType: "object",
      required: ["codeSystem", "code"],
      properties: {
        codeSystem: { bsonType: "string" },
        code: { bsonType: "string" },
        description: { bsonType: "string" },
        shortDescription: { bsonType: "string" },
        category: { bsonType: "string" },
        parentCode: { bsonType: "string" },
        children: { 
          bsonType: "array",
          items: { bsonType: "string" }
        },
        metadata: { bsonType: "object" }
      }
    }
  }
});

db.CodeDescriptions.createIndex({ codeSystem: 1, code: 1 }, { unique: true });
db.CodeDescriptions.createIndex({ codeSystem: 1, category: 1 });

print("✅ Terminology Service: ehr_terminology_documents database created");

// ─────────────────────────────────────────────────────────────────────────────
// FileStorage Service Database - Document Metadata & Storage References
// ─────────────────────────────────────────────────────────────────────────────

db = db.getSiblingDB('ehr_filestorage_documents');

db.createCollection('DocumentMetadata', {
  validator: {
    $jsonSchema: {
      bsonType: "object",
      required: ["fileId", "fileName", "s3Key"],
      properties: {
        fileId: { bsonType: "string" },
        fileName: { bsonType: "string" },
        s3Key: { bsonType: "string" },
        contentType: { bsonType: "string" },
        fileSize: { bsonType: "long" },
        uploadedBy: { bsonType: "string" },
        uploadedAt: { bsonType: "date" },
        patientId: { bsonType: "string" },
        documentType: { bsonType: "string" },
        virusScanStatus: { 
          bsonType: "string",
          enum: ["pending", "passed", "failed"]
        },
        scannedAt: { bsonType: "date" },
        tags: { 
          bsonType: "array",
          items: { bsonType: "string" }
        },
        isDeleted: { bsonType: "bool", default: false },
        deletedAt: { bsonType: "date" }
      }
    }
  }
});

db.DocumentMetadata.createIndex({ fileId: 1 }, { unique: true });
db.DocumentMetadata.createIndex({ patientId: 1, uploadedAt: -1 });
db.DocumentMetadata.createIndex({ s3Key: 1 });
db.DocumentMetadata.createIndex({ virusScanStatus: 1 });

// Versioning Collection
db.createCollection('DocumentVersions', {
  validator: {
    $jsonSchema: {
      bsonType: "object",
      required: ["fileId", "versionNumber", "s3Key"],
      properties: {
        fileId: { bsonType: "string" },
        versionNumber: { bsonType: "int" },
        s3Key: { bsonType: "string" },
        createdAt: { bsonType: "date" },
        createdBy: { bsonType: "string" },
        changeDescription: { bsonType: "string" }
      }
    }
  }
});

db.DocumentVersions.createIndex({ fileId: 1, versionNumber: -1 });

print("✅ FileStorage Service: ehr_filestorage_documents database created");

// ─────────────────────────────────────────────────────────────────────────────
// AI Service Database - ML Model Data & Predictions Cache
// ─────────────────────────────────────────────────────────────────────────────

db = db.getSiblingDB('ehr_ai_documents');

db.createCollection('PredictionResults', {
  validator: {
    $jsonSchema: {
      bsonType: "object",
      required: ["patientId", "predictionType"],
      properties: {
        patientId: { bsonType: "string" },
        predictionType: { 
          bsonType: "string",
          enum: ["diagnosis", "readmission_risk", "fraud_detection", "coding_recommendation"]
        },
        modelVersion: { bsonType: "string" },
        input: { bsonType: "object" },
        prediction: { bsonType: "object" },
        confidence: { bsonType: "double" },
        createdAt: { bsonType: "date" },
        expiresAt: { bsonType: "date" }
      }
    }
  }
});

db.PredictionResults.createIndex({ patientId: 1, predictionType: 1, createdAt: -1 });
db.PredictionResults.createIndex({ expiresAt: 1 }, { expireAfterSeconds: 0 });

// Training Data Collection
db.createCollection('TrainingData', {
  validator: {
    $jsonSchema: {
      bsonType: "object",
      required: ["datasetId", "recordCount"],
      properties: {
        datasetId: { bsonType: "string" },
        modelType: { bsonType: "string" },
        recordCount: { bsonType: "long" },
        trainingPeriodStart: { bsonType: "date" },
        trainingPeriodEnd: { bsonType: "date" },
        metrics: { bsonType: "object" },
        createdAt: { bsonType: "date" }
      }
    }
  }
});

db.TrainingData.createIndex({ datasetId: 1 }, { unique: true });
db.TrainingData.createIndex({ modelType: 1, createdAt: -1 });

print("✅ AI Service: ehr_ai_documents database created");

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
          enum: ["email", "sms", "push", "in-app"]
        },
        message: { bsonType: "string" },
        subject: { bsonType: "string" },
        status: { 
          bsonType: "string",
          enum: ["pending", "sent", "failed", "bounced"],
          default: "pending"
        },
        retryCount: { bsonType: "int", default: 0 },
        maxRetries: { bsonType: "int", default: 3 },
        createdAt: { bsonType: "date" },
        sentAt: { bsonType: "date" },
        errorDetails: { bsonType: "object" }
      }
    }
  }
});

// TTL index: Auto-delete after 90 days
db.NotificationQueue.createIndex({ createdAt: 1 }, { expireAfterSeconds: 7776000 });
db.NotificationQueue.createIndex({ userId: 1, status: 1 });
db.NotificationQueue.createIndex({ status: 1, createdAt: 1 });

// Notification Templates Collection
db.createCollection('NotificationTemplates', {
  validator: {
    $jsonSchema: {
      bsonType: "object",
      required: ["templateId", "type", "language"],
      properties: {
        templateId: { bsonType: "string" },
        type: { 
          bsonType: "string",
          enum: ["email", "sms", "push"]
        },
        language: { bsonType: "string" },
        subject: { bsonType: "string" },
        body: { bsonType: "string" },
        variables: { 
          bsonType: "array",
          items: { bsonType: "string" }
        },
        createdAt: { bsonType: "date" },
        updatedAt: { bsonType: "date" }
      }
    }
  }
});

db.NotificationTemplates.createIndex({ templateId: 1, type: 1, language: 1 }, { unique: true });

print("✅ Notification Service: ehr_notification_documents database created");

// ─────────────────────────────────────────────────────────────────────────────
// Audit Service Database - HIPAA Audit Logs (Immutable)
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
          enum: ["READ", "CREATE", "UPDATE", "DELETE", "LOGIN", "LOGOUT", "EXPORT"]
        },
        resourceType: { bsonType: "string" },
        resourceId: { bsonType: "string" },
        oldValue: { bsonType: "object" },
        newValue: { bsonType: "object" },
        ipAddress: { bsonType: "string" },
        userAgent: { bsonType: "string" },
        timestamp: { bsonType: "date" },
        correlationId: { bsonType: "string" },
        severity: { 
          bsonType: "string",
          enum: ["info", "warning", "critical"]
        }
      }
    }
  }
});

// TTL index: HIPAA requires 7 years retention (220752000 seconds)
db.AuditLogs.createIndex({ timestamp: 1 }, { expireAfterSeconds: 220752000, name: "hipaa_retention_ttl" });
db.AuditLogs.createIndex({ userId: 1, timestamp: -1 });
db.AuditLogs.createIndex({ resourceType: 1, resourceId: 1, timestamp: -1 });
db.AuditLogs.createIndex({ action: 1, timestamp: -1 });
db.AuditLogs.createIndex({ correlationId: 1 });

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
        route: { bsonType: "string" },
        startDate: { bsonType: "date" },
        endDate: { bsonType: "date" },
        prescribedBy: { bsonType: "string" },
        indication: { bsonType: "string" },
        sideEffects: { 
          bsonType: "array",
          items: { bsonType: "string" }
        },
        createdAt: { bsonType: "date" }
      }
    }
  }
});

db.MedicationHistory.createIndex({ patientId: 1 });
db.MedicationHistory.createIndex({ patientId: 1, startDate: -1 });
db.MedicationHistory.createIndex({ medicationName: 1 });

print("✅ Prescription Service: ehr_prescription_documents database created");

// ─────────────────────────────────────────────────────────────────────────────
// Outbox Processor Database - Transactional Outbox Pattern
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
        aggregateType: { bsonType: "string" },
        isPublished: { bsonType: "bool", default: false },
        createdAt: { bsonType: "date" },
        publishedAt: { bsonType: "date" },
        retryCount: { bsonType: "int", default: 0 }
      }
    }
  }
});

db.OutboxEvents.createIndex({ isPublished: 1, createdAt: 1 });
db.OutboxEvents.createIndex({ aggregateId: 1, aggregateType: 1 });
// TTL: Keep unpublished events for 7 days (604800 seconds)
db.OutboxEvents.createIndex({ createdAt: 1 }, { expireAfterSeconds: 604800 });

print("✅ Outbox Processor: ehr_outbox_documents database created");

// ─────────────────────────────────────────────────────────────────────────────
// Summary & Verification
// ─────────────────────────────────────────────────────────────────────────────

print("\n════════════════════════════════════════════════════════════════");
print("✅ MongoDB Initialization Complete");
print("════════════════════════════════════════════════════════════════");

const adminDbList = db.getSiblingDB('admin');
const databases = adminDbList.adminCommand('listDatabases');
const ehrDatabases = databases.databases.filter(d => d.name.startsWith('ehr_'));

print(`\n📊 Total EHR databases created: ${ehrDatabases.length}`);
print("Databases:");
ehrDatabases.forEach(d => print(`  ✓ ${d.name}`));

print("\n📋 Service-Database Mapping:");
print("  ✓ Patient Service:        ehr_patient_documents");
print("  ✓ Clinical Service:       ehr_clinical_documents");
print("  ✓ Appointment Service:    ehr_appointment_documents");
print("  ✓ Integration Service:    ehr_integration_documents");
print("  ✓ Terminology Service:    ehr_terminology_documents");
print("  ✓ FileStorage Service:    ehr_filestorage_documents");
print("  ✓ AI Service:             ehr_ai_documents");
print("  ✓ Notification Service:   ehr_notification_documents");
print("  ✓ Audit Service:          ehr_audit_documents");
print("  ✓ Prescription Service:   ehr_prescription_documents");
print("  ✓ Outbox Processor:       ehr_outbox_documents");

print("\n✅ All databases and collections ready for microservices deployment");
