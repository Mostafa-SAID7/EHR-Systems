namespace EHRPlatform.Services.Integration.Application.Services;

using EHRPlatform.Services.Integration.Domain.Entities;

/// <summary>
/// Interface for HL7 to FHIR transformation service.
/// Converts HL7v2 messages to FHIR R4 resources.
/// </summary>
public interface IFHIRTransformerService
{
    /// <summary>
    /// Transforms HL7 message to FHIR format.
    /// </summary>
    Task<FHIRTransformResult> TransformHL7ToFHIRAsync(HL7Message message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Transforms specific HL7 segment to FHIR resource.
    /// </summary>
    Task<string> TransformSegmentToFHIRAsync(string segmentId, string segmentContent, CancellationToken cancellationToken = default);
}

public class FHIRTransformResult
{
    public string ResourceType { get; set; } = string.Empty; // Patient, Observation, etc.
    public string FHIRContent { get; set; } = string.Empty; // JSON FHIR
    public bool IsPartial { get; set; }
    public List<string> Warnings { get; set; } = new();
}
