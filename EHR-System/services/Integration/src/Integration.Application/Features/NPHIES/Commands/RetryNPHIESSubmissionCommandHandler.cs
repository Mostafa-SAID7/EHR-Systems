namespace EHRPlatform.Services.Integration.Application.Features.NPHIES.Commands;

using MediatR;
using EHRPlatform.Services.Integration.Persistence;
using EHRPlatform.Services.Integration.Application.Services;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handler for RetryNPHIESSubmissionCommand - Retries failed NPHIES claims.
/// </summary>
public class RetryNPHIESSubmissionCommandHandler : IRequestHandler<RetryNPHIESSubmissionCommand, RetryNPHIESSubmissionResponse>
{
    private readonly IIntegrationDbContext _context;
    private readonly INPHIESService _nphiesService;
    private readonly ILogger<RetryNPHIESSubmissionCommandHandler> _logger;

    public RetryNPHIESSubmissionCommandHandler(
        IIntegrationDbContext context,
        INPHIESService nphiesService,
        ILogger<RetryNPHIESSubmissionCommandHandler> logger)
    {
        _context = context;
        _nphiesService = nphiesService;
        _logger = logger;
    }

    public async Task<RetryNPHIESSubmissionResponse> Handle(RetryNPHIESSubmissionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrying NPHIES submission for claim {ClaimId}", request.ClaimId);

        var claim = await _context.NPHIESClaims.FindAsync(new object[] { request.ClaimId }, cancellationToken);
        if (claim == null)
        {
            throw new InvalidOperationException($"Claim {request.ClaimId} not found");
        }

        if (claim.RetryCount >= request.MaxRetries)
        {
            return new RetryNPHIESSubmissionResponse
            {
                ClaimId = claim.Id,
                RetrySuccessful = false,
                CurrentRetryCount = claim.RetryCount,
                ErrorMessage = $"Max retries ({request.MaxRetries}) exceeded"
            };
        }

        try
        {
            // Get original HL7 message
            var message = await _context.HL7Messages.FindAsync(new object[] { claim.HL7MessageId }, cancellationToken);
            if (message == null)
            {
                throw new InvalidOperationException("Original HL7 message not found");
            }

            // Retry submission
            var submissionResult = await _nphiesService.SubmitClaimAsync(message, claim.ClaimType, cancellationToken);

            claim.RetryCount++;
            claim.SubmissionResponse = submissionResult.Response;
            claim.Status = "Submitted";
            claim.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("NPHIES claim retry successful: {ClaimNumber}", claim.ClaimNumber);

            return new RetryNPHIESSubmissionResponse
            {
                ClaimId = claim.Id,
                RetrySuccessful = true,
                CurrentRetryCount = claim.RetryCount,
                SubmissionResponse = submissionResult.Response
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying NPHIES submission");
            claim.RetryCount++;
            await _context.SaveChangesAsync(cancellationToken);

            return new RetryNPHIESSubmissionResponse
            {
                ClaimId = claim.Id,
                RetrySuccessful = false,
                CurrentRetryCount = claim.RetryCount,
                ErrorMessage = ex.Message
            };
        }
    }
}
