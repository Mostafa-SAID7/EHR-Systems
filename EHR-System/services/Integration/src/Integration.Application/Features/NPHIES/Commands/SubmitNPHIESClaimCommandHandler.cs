namespace EHRPlatform.Services.Integration.Application.Features.NPHIES.Commands;

using MediatR;
using EHRPlatform.Services.Integration.Domain.Entities;
using EHRPlatform.Services.Integration.Persistence;
using EHRPlatform.Services.Integration.Application.Services;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handler for SubmitNPHIESClaimCommand - Submits claim to NPHIES.
/// </summary>
public class SubmitNPHIESClaimCommandHandler : IRequestHandler<SubmitNPHIESClaimCommand, SubmitNPHIESClaimResponse>
{
    private readonly IIntegrationDbContext _context;
    private readonly INPHIESService _nphiesService;
    private readonly ILogger<SubmitNPHIESClaimCommandHandler> _logger;

    public SubmitNPHIESClaimCommandHandler(
        IIntegrationDbContext context,
        INPHIESService nphiesService,
        ILogger<SubmitNPHIESClaimCommandHandler> logger)
    {
        _context = context;
        _nphiesService = nphiesService;
        _logger = logger;
    }

    public async Task<SubmitNPHIESClaimResponse> Handle(SubmitNPHIESClaimCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Submitting NPHIES claim for HL7 message {MessageId}", request.HL7MessageId);

        var message = await _context.HL7Messages.FindAsync(new object[] { request.HL7MessageId }, cancellationToken);
        if (message == null)
        {
            throw new InvalidOperationException($"HL7 message {request.HL7MessageId} not found");
        }

        try
        {
            // Convert to NPHIES format and submit
            var submissionResult = await _nphiesService.SubmitClaimAsync(message, request.ClaimType, cancellationToken);

            // Store NPHIES claim
            var claim = new NPHIESClaim
            {
                Id = Guid.NewGuid(),
                HL7MessageId = message.Id,
                FHIRTransformationId = request.FHIRTransformationId,
                ClaimNumber = submissionResult.ClaimNumber,
                ClaimType = request.ClaimType,
                Status = "Submitted",
                SubmissionResponse = submissionResult.Response,
                TotalAmount = submissionResult.TotalAmount,
                SubmittedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                CreatedAt = DateTime.UtcNow
            };

            _context.NPHIESClaims.Add(claim);
            message.Status = "Submitted";
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("NPHIES claim submitted: {ClaimNumber}", submissionResult.ClaimNumber);

            return new SubmitNPHIESClaimResponse
            {
                ClaimId = claim.Id,
                ClaimNumber = submissionResult.ClaimNumber,
                SubmissionSuccessful = true,
                SubmissionResponse = submissionResult.Response,
                SubmittedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting NPHIES claim");
            message.MarkAsError(ex.Message);
            await _context.SaveChangesAsync(cancellationToken);

            return new SubmitNPHIESClaimResponse
            {
                ClaimId = Guid.Empty,
                SubmissionSuccessful = false,
                ErrorMessage = ex.Message,
                SubmittedAt = DateTime.UtcNow
            };
        }
    }
}
