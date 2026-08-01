using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EHRPlatform.Services.Clinical.Persistence.Documents;

/// <summary>
/// MongoDB document for clinical note - stores free-text SOAP fields and attachments.
/// </summary>
[BsonIgnoreExtraElements]
public class ClinicalNoteDocument
{
    [BsonId]
    public ObjectId InternalId { get; set; }

    [BsonElement("clinicalNoteId")]
    public Guid ClinicalNoteId { get; set; }

    [BsonElement("patientId")]
    public Guid PatientId { get; set; }

    [BsonElement("subjectiveFullText")]
    public string SubjectiveFullText { get; set; } = string.Empty;

    [BsonElement("objectiveFullText")]
    public string ObjectiveFullText { get; set; } = string.Empty;

    [BsonElement("assessmentFullText")]
    public string AssessmentFullText { get; set; } = string.Empty;

    [BsonElement("planFullText")]
    public string PlanFullText { get; set; } = string.Empty;

    [BsonElement("attachments")]
    public List<AttachmentInfo> Attachments { get; set; } = new();

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Attachment metadata stored in MongoDB.
/// </summary>
public class AttachmentInfo
{
    [BsonElement("id")]
    public Guid Id { get; set; }

    [BsonElement("fileName")]
    public string FileName { get; set; } = string.Empty;

    [BsonElement("contentType")]
    public string ContentType { get; set; } = string.Empty;

    [BsonElement("size")]
    public long Size { get; set; }

    [BsonElement("uploadedAt")]
    public DateTime UploadedAt { get; set; }

    [BsonElement("storagePath")]
    public string StoragePath { get; set; } = string.Empty;
}
