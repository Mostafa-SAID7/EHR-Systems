namespace EHRPlatform.Services.Integration.Application.Features.HL7.Commands;

using MediatR;

/// <summary>
/// Command to transform HL7 message to FHIR format.
/// Creates FHIR resources (Patient, Observation, DiagnosticReport, etc.)
/// </summary>
public class TransformToFHIRCommand : IRequest<TransformToFHIRResponse>
{
    public Guid HL7MessageId { get; set; }
}

public class TransformToFHIRResponse
{
    public Guid HL7MessageId { get; set; }
    public Guid FHIRTransformationId { get; set; }
    public string ResourceType { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public string? FHIRContent { get; set; }
    public List<string> ValidationErrors { get; set; } = new();
}
