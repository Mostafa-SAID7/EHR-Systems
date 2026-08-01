using System;

namespace EHRPlatform.Common.Shared.DTOs
{
    /// <summary>
    /// Shared DTO for Audit Log Communication
    /// HIPAA Compliance: Track all data access and modifications
    /// </summary>
    public class AuditLogDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Action { get; set; }          // e.g., "READ", "CREATE", "UPDATE", "DELETE"
        public string ResourceType { get; set; }    // e.g., "Patient", "Prescription", "Invoice"
        public Guid ResourceId { get; set; }
        public string OldValues { get; set; }       // JSON of previous values
        public string NewValues { get; set; }       // JSON of new values
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Event: Data Access Logged
    /// Published by Audit Service when data is read
    /// Subscribed by: Compliance systems for HIPAA tracking
    /// </summary>
    public class DataAccessLoggedEvent
    {
        public Guid AuditLogId { get; set; }
        public Guid UserId { get; set; }
        public string ResourceType { get; set; }
        public Guid ResourceId { get; set; }
        public string IpAddress { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Event: Data Modified Logged
    /// Published by Audit Service when data is created/updated/deleted
    /// Subscribed by: Compliance systems, Change tracking
    /// </summary>
    public class DataModificationLoggedEvent
    {
        public Guid AuditLogId { get; set; }
        public Guid UserId { get; set; }
        public string Action { get; set; }
        public string ResourceType { get; set; }
        public Guid ResourceId { get; set; }
        public string OldValues { get; set; }
        public string NewValues { get; set; }
        public string IpAddress { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Event: Security Incident Logged
    /// Published by Audit Service when suspicious activity is detected
    /// Subscribed by: Security monitoring, Compliance alerts
    /// </summary>
    public class SecurityIncidentLoggedEvent
    {
        public Guid AuditLogId { get; set; }
        public Guid? UserId { get; set; }
        public string IncidentType { get; set; }   // e.g., "Failed Login", "Unauthorized Access", "Invalid Token"
        public string Details { get; set; }
        public string IpAddress { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }
}
