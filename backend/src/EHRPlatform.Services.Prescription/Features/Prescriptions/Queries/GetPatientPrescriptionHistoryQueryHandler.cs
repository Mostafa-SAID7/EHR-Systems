using EHRPlatform.Common.CQRS;
using EHRPlatform.Common.Data;
using EHRPlatform.Common.DTOs;
using EHRPlatform.Services.Prescription.Application.PrescriptionManagement.Responses;
using EHRPlatform.Services.Prescription.Domain.Entities;
using Mapster;

namespace EHRPlatform.Services.Prescription.Features.Prescriptions.Queries;

/// <summary>
/// Get patient prescription history handler.
/// Single Responsibility: Retrieve paginated full prescription history for a given patient.
/// </summary>
public class GetPatientPrescriptionHistoryQueryHandler : IQueryHandler<GetPatientPrescriptionHistoryQuery, PagedResult<PrescriptionResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetPatientPrescriptionHistoryQueryHandler> _logger;

    public GetPatientPrescriptionHistoryQueryHandler(IUnitOfWork unitOfWork, ILogger<GetPatientPrescriptionHistoryQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PagedResult<PrescriptionResponseDto>> Handle(
        GetPatientPrescriptionHistoryQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching prescription history for patient {PatientId}", request.PatientId);

        var repo = _unitOfWork.Repository<PrescriptionEntity>();
        var skip = (request.PageNumber - 1) * request.PageSize;

        var total = await repo.CountAsync(
            q => q.Where(p => p.PatientId == request.PatientId),
            cancellationToken);

        var prescriptions = await repo.ToListAsync(
            q => q.Where(p => p.PatientId == request.PatientId)
                .OrderByDescending(p => p.StartDate)
                .Skip(skip)
                .Take(request.PageSize),
            cancellationToken);

        var items = prescriptions.Adapt<List<PrescriptionResponseDto>>();
        return PagedResult<PrescriptionResponseDto>.Create(items, total, request.PageNumber, request.PageSize);
    }
}

