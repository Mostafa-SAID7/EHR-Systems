using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Common.Data.Abstractions;
using EHRPlatform.Services.Billing.Domain.Entities;
using EHRPlatform.Services.Billing.Domain.Enums;

namespace EHRPlatform.Services.Billing.Features.Claims.Queries;

// ─── Query ───────────────────────────────────────────────────────────────────

public record GetClaimStatusQuery(Guid ClaimId) : IQuery<ClaimStatusDto>;

// ─── Response DTO ─────────────────────────────────────────────────────────────

/// <summary>
/// Full claim status snapshot returned to callers.
/// Includes payer details, lifecycle dates, fraud score, and prior auth info.
/// </summary>
public record ClaimStatusDto(
    Guid Id,
    Guid InvoiceId,
    string ClaimNumber,
    ClaimStatus Status,
    string InsuranceProvider,
    string? MemberId,
    string? GroupNumber,
    string? PriorAuthorizationNumber,
    string? Npi,
    decimal Amount,
    decimal? ApprovedAmount,
    string? DenialReason,
    string? DenialCode,
    decimal FraudScore,
    string? FraudFlags,
    DateTime SubmittedAt,
    DateTime? ApprovedAt,
    DateTime? DeniedAt
);

// ─── Handler ─────────────────────────────────────────────────────────────────

public class GetClaimStatusQueryHandler : IQueryHandler<GetClaimStatusQuery, ClaimStatusDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetClaimStatusQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ClaimStatusDto> Handle(GetClaimStatusQuery request, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<InsuranceClaim>();
        var claim = await repo.FirstOrDefaultAsync(
            q => q.Where(c => c.Id == request.ClaimId),
            cancellationToken);

        if (claim == null)
            throw new InvalidOperationException($"Insurance claim {request.ClaimId} not found.");

        return new ClaimStatusDto(
            claim.Id,
            claim.InvoiceId,
            claim.ClaimNumber,
            claim.Status,
            claim.InsuranceProvider,
            claim.MemberId,
            claim.GroupNumber,
            claim.PriorAuthorizationNumber,
            claim.Npi,
            claim.Amount,
            claim.ApprovedAmount,
            claim.DenialReason,
            claim.DenialCode,
            claim.FraudScore,
            claim.FraudFlags,
            claim.SubmittedAt,
            claim.ApprovedAt,
            claim.DeniedAt
        );
    }
}


