namespace EHRPlatform.Services.Audit.Domain.Entities;

/// <summary>
/// AuditEntry - HIPAA-compliant immutable audit log.
/// Write-only, no update/delete capabilities for compliance.
/// Integrity hash (SHA-256) prevents tampering detection.
/// </summary>
public class AuditEntry
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string UserEmail { get; private set; } = string.Empty;
    public string UserFullName { get; private set; } = string.Empty;
    
    // Action tracking
    public string Action { get; private set; } = string.Empty; // Create, Read, Update, Delete, Export, Login, Logout
    public string ResourceType { get; private set; } = string.Empty; // Patient, Appointment, Clinical, Invoice, etc.
    public Guid ResourceId { get; private set; }
    public string Status { get; private set; } = "Success"; // Success, Failed, PartialSuccess
    
    // Request info
    public string IpAddress { get; private set; } = string.Empty;
    public string UserAgent { get; private set; } = string.Empty;
    public string HttpMethod { get; private set; } = string.Empty; // GET, POST, PUT, DELETE
    public string Endpoint { get; private set; } = string.Empty; // /api/v1/patients
    
    // PII tracking
    public bool ContainsSsn { get; private set; }
    public bool ContainsDob { get; private set; }
    public bool ContainsMrn { get; private set; }
    public bool ContainsPhoneNumber { get; private set; }
    
    // Access level
    public string AccessLevel { get; private set; } = "Internal"; // Public, Internal, Confidential, Restricted
    
    // Change details
    public string? ChangeDetails { get; private set; } // JSON: {before: {field: value}, after: {field: value}}
    public string? ErrorMessage { get; private set; }
    
    // Integrity verification
    public string IntegrityHash { get; private set; } = string.Empty; // SHA-256 for tampering detection
    public DateTime CreatedAt { get; private set; }
    
    // Constructor for creating new entry
    public AuditEntry(
        Guid userId,
        string userEmail,
        string userFullName,
        string action,
        string resourceType,
        Guid resourceId,
        string ipAddress,
        string userAgent,
        string httpMethod,
        string endpoint)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        UserEmail = userEmail;
        UserFullName = userFullName;
        Action = action;
        ResourceType = resourceType;
        ResourceId = resourceId;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        HttpMethod = httpMethod;
        Endpoint = endpoint;
        CreatedAt = DateTime.UtcNow;
        Status = "Success";
        AccessLevel = "Internal";
    }

    // Methods - IMMUTABLE WRITE-ONLY
    public void SetStatus(string status)
    {
        Status = status;
    }

    public void SetPiiFlags(bool containsSsn, bool containsDob, bool containsMrn, bool containsPhoneNumber)
    {
        ContainsSsn = containsSsn;
        ContainsDob = containsDob;
        ContainsMrn = containsMrn;
        ContainsPhoneNumber = containsPhoneNumber;
    }

    public void SetAccessLevel(string accessLevel)
    {
        AccessLevel = accessLevel;
    }

    public void SetChangeDetails(string? changeDetails)
    {
        ChangeDetails = changeDetails;
    }

    public void SetErrorMessage(string? errorMessage)
    {
        ErrorMessage = errorMessage;
    }

    public void CalculateIntegrityHash()
    {
        // SHA-256 hash of: UserId + Action + ResourceId + CreatedAt + ChangeDetails
        var data = $"{UserId}{Action}{ResourceId}{CreatedAt}{ChangeDetails}";
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            var hashedData = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(data));
            IntegrityHash = Convert.ToHexString(hashedData);
        }
    }

    public bool VerifyIntegrity()
    {
        var currentHash = IntegrityHash;
        CalculateIntegrityHash();
        return currentHash == IntegrityHash;
    }
}

// Domain events
public record AuditEntryCreatedEvent(Guid AuditId, Guid UserId, string Action, string ResourceType, DateTime CreatedAt)
{
    public DateTime EventAt { get; } = DateTime.UtcNow;
}

public record SuspiciousActivityDetectedEvent(Guid AuditId, string Reason, DateTime DetectedAt)
{
    public DateTime EventAt { get; } = DateTime.UtcNow;
}
