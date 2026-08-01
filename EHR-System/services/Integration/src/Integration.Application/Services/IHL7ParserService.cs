namespace EHRPlatform.Services.Integration.Application.Services;

/// <summary>
/// Interface for HL7 message parsing service.
/// Parses HL7v2 messages and extracts structured data.
/// </summary>
public interface IHL7ParserService
{
    /// <summary>
    /// Parses HL7 message content and extracts segments.
    /// </summary>
    Task<HL7ParseResult> ParseHL7Async(string hl7Content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates HL7 message format and structure.
    /// </summary>
    Task<HL7ValidationResult> ValidateHL7Async(string hl7Content, CancellationToken cancellationToken = default);
}

public class HL7ParseResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public string MessageId { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string SegmentType { get; set; } = string.Empty;
    public Guid? PatientId { get; set; }
    public Guid? EncounterId { get; set; }
    public DateTime MessageDateTime { get; set; }
    public Dictionary<string, string> Segments { get; set; } = new();
}

public class HL7ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}
