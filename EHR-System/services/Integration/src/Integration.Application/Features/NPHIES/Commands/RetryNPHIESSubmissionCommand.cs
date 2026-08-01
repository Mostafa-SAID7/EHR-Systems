namespace EHRPlatform.Services.Integration.Application.Features.NPHIES.Commands;

using MediatR;

/// <summary>
/// Command to retry failed NPHIES claim submission.
/// Implements exponential backoff retry logic.
/// </summary>
public class RetryNPHIESSubmissionCommand : IRequest<RetryNPHIESSubmissionResponse>
{
    public Guid ClaimId { get; set; }
    public int MaxRetries { get; set; } = 3;
}

public class RetryNPHIESSubmissionResponse
{
    public Guid ClaimId { get; set; }
    public bool RetrySuccessful { get; set; }
    public int CurrentRetryCount { get; set; }
    public string? SubmissionResponse { get; set; }
    public string? ErrorMessage { get; set; }
}
