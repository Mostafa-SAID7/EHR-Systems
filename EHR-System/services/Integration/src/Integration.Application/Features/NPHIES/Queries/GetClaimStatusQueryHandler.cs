namespace EHRPlatform.Services.Integration.Application.Features.NPHIES.Queries;

using MediatR;
using EHRPlatform.Services.Integration.Persistence;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handler for GetClaimStatusQuery - Returns NPHIES claim status.
/// </summary>
public class GetClaimStatusQueryHandler : IRequestHandler<GetClaimStatusQuery, ClaimStatusDto>
{
    private readonly IIntegrationDbContext _context;
    private readonly ILogger<GetClaimStatusQueryHandler> _logger;

    public GetClaimStatusQueryHandler(
        IIntegrationDbContext context,
        ILogger<GetClaimStatusQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ClaimStatusDto> Handle(GetClaimStatusQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting claim status for {ClaimId}", request.ClaimId);

        var claim = await _context.NPHIESClaims.FindAsync(new object[] { request.ClaimId }, cancellationToken);
        if (claim == null)
        {
            throw new InvalidOperationException($"Claim {request.ClaimId} not found");
        }

        return new ClaimStatusDto
        {
            ClaimId = claim.Id,
            ClaimNumber = claim.ClaimNumber,
            Status = claim.Status,
            ClaimType = claim.ClaimType,
            TotalAmount = claim.TotalAmount,
            RetryCount = claim.RetryCount,
            SubmissionResponse = claim.SubmissionResponse,
            SubmittedAt = claim.SubmittedAt,
            ExpiresAt = claim.ExpiresAt,
            CreatedAt = claim.CreatedAt
        };
    }
}
