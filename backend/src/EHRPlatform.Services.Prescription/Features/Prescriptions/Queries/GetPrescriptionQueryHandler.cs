using EHRPlatform.Common.Application.CQRS;
using EHRPlatform.Common.Data;
using EHRPlatform.Services.Prescription.Application.PrescriptionManagement.Responses;
using EHRPlatform.Services.Prescription.Domain.Entities;
using Mapster;

namespace EHRPlatform.Services.Prescription.Features.Prescriptions.Queries;

/// <summary>
/// Get prescription by ID handler.
/// Single Responsibility: Fetch and project a single prescription by ID including refills.
/// </summary>
public class GetPrescriptionQueryHandler : IQueryHandler<GetPrescriptionQuery, PrescriptionResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetPrescriptionQueryHandler> _logger;

    public GetPrescriptionQueryHandler(IUnitOfWork unitOfWork, ILogger<GetPrescriptionQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PrescriptionResponseDto> Handle(
        GetPrescriptionQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching prescription {PrescriptionId}", request.PrescriptionId);

        var repo = _unitOfWork.Repository<PrescriptionEntity>();
        var prescription = await repo.FirstOrDefaultAsync(
            q => q.Where(p => p.Id == request.PrescriptionId),
            cancellationToken);

        if (prescription == null)
            throw new InvalidOperationException($"Prescription {request.PrescriptionId} not found");

        var dto = prescription.Adapt<PrescriptionResponseDto>();
        dto.Refills = prescription.Refills.Select(r => new RefillDto
        {
            Id = r.Id,
            RequestedAt = r.RequestedAt,
            ApprovedAt = r.ApprovedAt,
            Status = r.Status,
            PharmacyId = r.PharmacyId
        }).ToList();

        return dto;
    }
}


