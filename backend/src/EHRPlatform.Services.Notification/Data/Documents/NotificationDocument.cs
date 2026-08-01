using EHRPlatform.BuildingBlocks.Common.Data;
using MongoDB.Bson.Serialization.Attributes;

namespace EHRPlatform.Services.Notification.Data.Documents;

/// <summary>
/// MongoDB document for a Notification.
/// TemplateVars is a dictionary — natural fit for MongoDB's flexible schema.
/// Each channel (Email/SMS/Push/InApp) can have a different payload shape
/// without requiring nullable columns.
/// EntityId = Notification domain Id (Guid).
/// </summary>
public sealed class NotificationDocument : MongoBaseDocument
{
    [BsonElement("recipientId")]     public Guid   RecipientId      { get; set; }
    [BsonElement("channel")]         public string Channel          { get; set; } = string.Empty;
    [BsonElement("notificationType")]public string NotificationType { get; set; } = string.Empty;
    [BsonElement("subject")]         public string Subject          { get; set; } = string.Empty;
    [BsonElement("body")]            public string Body             { get; set; } = string.Empty;
    [BsonElement("status")]          public string Status           { get; set; } = "Pending";
    [BsonElement("retryCount")]      public int    RetryCount       { get; set; }
    [BsonElement("scheduledFor")][BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? ScheduledFor { get; set; }
    [BsonElement("sentAt")][BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? SentAt { get; set; }
    [BsonElement("failureReason")]   public string? FailureReason   { get; set; }

    /// <summary>
    /// Template variable substitution bag — varies per notification type and channel.
    /// Stored as an embedded document in MongoDB rather than a serialized JSON string.
    /// </summary>
    [BsonElement("templateVars")]
    public Dictionary<string, string> TemplateVars { get; set; } = new();
}

/// <summary>
/// MongoDB document for a user's notification channel preference.
/// Stored in the same database as notifications for co-location.
/// </summary>
public sealed class NotificationPreferenceDocument : MongoBaseDocument
{
    [BsonElement("userId")]          public Guid   UserId           { get; set; }
    [BsonElement("channel")]         public string Channel          { get; set; } = string.Empty;
    [BsonElement("notificationType")]public string NotificationType { get; set; } = string.Empty;
    [BsonElement("isEnabled")]       public bool   IsEnabled        { get; set; } = true;
}

