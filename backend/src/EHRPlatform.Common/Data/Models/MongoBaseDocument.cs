#nullable enable

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using EHRPlatform.Common.Shared.Utilities.Helpers;

namespace EHRPlatform.Common.Data.Models;

/// <summary>
/// Base class for all MongoDB documents in the EHR platform.
/// Provides common fields: Id (ObjectId), CreatedAt, UpdatedAt, soft-delete.
/// Used for high-volume, schema-flexible data: clinical notes, audit logs,
/// device-generated vitals, scanned document metadata.
/// </summary>
public abstract class MongoBaseDocument
{
    /// <summary>
    /// MongoDB ObjectId — auto-generated on insert.
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; } = GuidHelper.NewGuidString();

    /// <summary>
    /// Logical entity ID (links back to the relational service domain ID).
    /// </summary>
    [BsonElement("entityId")]
    public Guid EntityId { get; set; }

    /// <summary>
    /// Tenant / hospital ID for multi-tenancy isolation.
    /// </summary>
    [BsonElement("tenantId")]
    public Guid? TenantId { get; set; }

    /// <summary>
    /// UTC creation timestamp.
    /// </summary>
    [BsonElement("createdAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; } = DateTimeHelper.UtcNow;

    /// <summary>
    /// UTC last-modified timestamp.
    /// </summary>
    [BsonElement("updatedAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime UpdatedAt { get; set; } = DateTimeHelper.UtcNow;

    /// <summary>
    /// Soft-delete timestamp. Null means the document is active.
    /// Queries should filter on IsDeleted == false unless doing audit/recovery work.
    /// </summary>
    [BsonElement("deletedAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Convenience property — true when soft-deleted.
    /// </summary>
    [BsonIgnore]
    public bool IsDeleted => DeletedAt.HasValue;

    /// <summary>
    /// Schema version — increment when document shape changes so consumers
    /// can handle multiple versions during rolling upgrades.
    /// </summary>
    [BsonElement("schemaVersion")]
    public int SchemaVersion { get; set; } = 1;
}

