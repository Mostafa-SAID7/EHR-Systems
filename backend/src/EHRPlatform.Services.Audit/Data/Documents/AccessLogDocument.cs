#nullable enable

using EHRPlatform.BuildingBlocks.Common.Data;
using MongoDB.Bson.Serialization.Attributes;

namespace EHRPlatform.Services.Audit.Data.Documents;

/// <summary>
/// MongoDB document for a resource access log entry.
///
/// Access logs record every time a user views or downloads PHI — a HIPAA §164.312
/// requirement.  They are pure append-only, extremely high-volume, and benefit from
/// MongoDB's write throughput over PostgreSQL.
///
/// Retention: 7 years minimum (HIPAA).
/// Collection: "access-logs" (MongoRepository convention: kebab-plural of type name).
/// </summary>
public class AccessLogDocument : MongoBaseDocument
{
    /// <summary>Links back to the PostgreSQL AccessLog.Id when a PG row also exists.</summary>
    [BsonElement("pgAccessLogId")]
    public Guid? PgAccessLogId { get; set; }

    [BsonElement("userId")]
    public Guid UserId { get; set; }

    [BsonElement("userEmail")]
    public string UserEmail { get; set; } = string.Empty;

    /// <summary>Patient | ClinicalNote | Appointment | Prescription | Invoice | Report | …</summary>
    [BsonElement("resourceType")]
    public string ResourceType { get; set; } = string.Empty;

    [BsonElement("resourceId")]
    public Guid ResourceId { get; set; }

    [BsonElement("accessedAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime AccessedAt { get; set; }

    /// <summary>How many seconds the user actively viewed the resource.</summary>
    [BsonElement("durationSeconds")]
    public int DurationSeconds { get; set; }

    [BsonElement("ipAddress")]
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>True when the access resulted in data being exported to a file.</summary>
    [BsonElement("isExport")]
    public bool IsExport { get; set; }

    /// <summary>True when the access resulted in a print event.</summary>
    [BsonElement("isPrint")]
    public bool IsPrint { get; set; }

    /// <summary>HTTP method: GET | POST | PUT | DELETE</summary>
    [BsonElement("httpMethod")]
    public string? HttpMethod { get; set; }

    /// <summary>Relative URL path that was accessed, e.g. /api/patients/{id}</summary>
    [BsonElement("path")]
    public string? Path { get; set; }

    /// <summary>User agent string from the browser/client.</summary>
    [BsonElement("userAgent")]
    public string? UserAgent { get; set; }

    /// <summary>
    /// Session ID for grouping multiple access log entries within one user session.
    /// Supports "what did this user do during this session?" audit queries.
    /// </summary>
    [BsonElement("sessionId")]
    public string? SessionId { get; set; }
}

