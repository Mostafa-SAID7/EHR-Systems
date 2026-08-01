namespace EHRPlatform.Services.Integration.Application.Features.NPHIES.Commands;

using MediatR;

/// <summary>
/// Command to submit claim to NPHIES (National Program for Health Insurance).
/// Converts FHIR resources to NPHIES format and submits via API.
/// </summary>
public class SubmitNPHIESClaimCommand : IRequest<SubmitNPHIESClaimResponse>
{
    public Guid HL7MessageId { get; set; }
    public Guid? FHIRTransformationId { get; set; }
    public string ClaimType { get; set; } = string.Empty; // Professional, Institutional
}

public class SubmitNPHIESClaimResponse
{
    public Guid ClaimId { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public bool SubmissionSuccessful { get; set; }
    public string? SubmissionResponse { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime SubmittedAt { get; set; }
}
