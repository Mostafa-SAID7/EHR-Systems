namespace EHRPlatform.Services.Integration.Application.Features.NPHIES.Queries;

using MediatR;

/// <summary>
/// Query to get NPHIES claim status and submission details.
/// </summary>
public class GetClaimStatusQuery : IRequest<ClaimStatusDto>
{
    public Guid ClaimId { get; set; }
}

public class ClaimStatusDto
{
    public Guid ClaimId { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ClaimType { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int RetryCount { get; set; }
    public string? SubmissionResponse { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
