namespace EHRPlatform.Services.Integration.Domain.Entities;

/// <summary>
/// HL7Message aggregate root - Represents received/sent HL7 messages.
/// Tracks message parsing, validation, and transformation status.
/// </summary>
public class HL7Message
{
    public Guid Id { get; set; }
    public string MessageId { get; set; } = string.Empty; // HL7 MSH-10
    public string SegmentType { get; set; } = string.Empty; // ADT, ORU, RGV, etc.
    public string HL7Content { get; set; } = string.Empty; // Raw HL7 message
    public string Status { get; set; } = "Received"; // Received, Parsed, Validated, Transformed, Sent, Error
    public Guid? PatientId { get; set; }
    public Guid? EncounterId { get; set; }
    public string MessageType { get; set; } = string.Empty; // ADT^A01, ORU^R01, etc.
    public string EventType { get; set; } = string.Empty; // Admit, Discharge, Lab Result, etc.
    public DateTime MessageDateTime { get; set; }
    public string? SendingApplication { get; set; }
    public string? ReceivingApplication { get; set; }
    public bool IsProcessed { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<HL7MessagePart> MessageParts { get; } = new List<HL7MessagePart>();
    public ICollection<FHIRTransformation> Transformations { get; } = new List<FHIRTransformation>();

    private readonly List<object> _domainEvents = new();

    public void MarkAsProcessed()
    {
        IsProcessed = true;
        Status = "Processed";
        UpdatedAt = DateTime.UtcNow;
        RaiseEvent(new HL7MessageProcessedEvent(Id, PatientId, MessageType));
    }

    public void MarkAsError(string error)
    {
        Status = "Error";
        ErrorMessage = error;
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncrementRetry()
    {
        RetryCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RaiseEvent(object @event) => _domainEvents.Add(@event);
    public IReadOnlyList<object> GetDomainEvents() => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();
}

/// <summary>
/// HL7MessagePart - Parsed segments from HL7 message (PID, OBR, OBX, etc.)
/// </summary>
public class HL7MessagePart
{
    public Guid Id { get; set; }
    public Guid HL7MessageId { get; set; }
    public string SegmentId { get; set; } = string.Empty; // PID, OBR, OBX, MSH
    public string SegmentContent { get; set; } = string.Empty;
    public int SequenceNumber { get; set; }
    public DateTime CreatedAt { get; set; }

    public HL7Message Message { get; set; } = null!;
}

/// <summary>
/// FHIRTransformation - Stores FHIR-transformed versions of HL7 messages
/// </summary>
public class FHIRTransformation
{
    public Guid Id { get; set; }
    public Guid HL7MessageId { get; set; }
    public string ResourceType { get; set; } = string.Empty; // Patient, Observation, DiagnosticReport, etc.
    public string FHIRContent { get; set; } = string.Empty; // JSON FHIR resource
    public bool IsValid { get; set; }
    public string? ValidationErrors { get; set; }
    public DateTime CreatedAt { get; set; }

    public HL7Message Message { get; set; } = null!;
}

/// <summary>
/// NPHIESClaim - NPHIES claim submission wrapper
/// </summary>
public class NPHIESClaim
{
    public Guid Id { get; set; }
    public Guid HL7MessageId { get; set; }
    public Guid? FHIRTransformationId { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public string ClaimType { get; set; } = string.Empty; // Professional, Institutional, Dental
    public string Status { get; set; } = "Draft"; // Draft, Submitted, Accepted, Rejected, Paid
    public string? SubmissionResponse { get; set; } // NPHIES response
    public decimal TotalAmount { get; set; }
    public int RetryCount { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public HL7Message Message { get; set; } = null!;
}

// Domain Events
public record HL7MessageReceivedEvent(Guid MessageId, string MessageType, Guid? PatientId, string Content)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}

public record HL7MessageProcessedEvent(Guid MessageId, Guid? PatientId, string MessageType)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}

public record FHIRTransformationCompleteEvent(Guid HL7MessageId, Guid FHIRTransformationId, string ResourceType)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}

public record NPHIESClaimSubmittedEvent(Guid ClaimId, string ClaimNumber, decimal Amount)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}
