namespace EHRPlatform.Services.Integration.Application.Services;

using EHRPlatform.Services.Integration.Domain.Entities;

/// <summary>
/// Interface for NPHIES (National Program for Health Insurance) integration.
/// Submits claims and retrieves responses from NPHIES.
/// </summary>
public interface INPHIESService
{
    /// <summary>
    /// Submits claim to NPHIES.
    /// </summary>
    Task<NPHIESSubmissionResult> SubmitClaimAsync(
        HL7Message message,
        string claimType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks claim status from NPHIES.
    /// </summary>
    Task<NPHIESClaimStatus> GetClaimStatusAsync(
        string claimNumber,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Converts FHIR claim to NPHIES format.
    /// </summary>
    Task<string> ConvertToNPHIESFormatAsync(
        string fhirContent,
        CancellationToken cancellationToken = default);
}

public class NPHIESSubmissionResult
{
    public string ClaimNumber { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public bool IsAccepted { get; set; }
}

public class NPHIESClaimStatus
{
    public string ClaimNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Submitted, Accepted, Rejected, Paid
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
