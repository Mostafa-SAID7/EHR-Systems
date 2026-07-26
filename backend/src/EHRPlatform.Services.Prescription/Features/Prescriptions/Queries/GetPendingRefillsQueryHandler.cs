using EHRPlatform.Common.CQRS;
using EHRPlatform.Common.Data;
using EHRPlatform.Services.Prescription.Application.PrescriptionManagement.Responses;
using EHRPlatform.Services.Prescription.Domain.Entities;

namespace EHRPlatform.Services.Prescription.Features.Prescriptions.Queries;

/// <summary>
/// Get pending refills handler.
/// Single Responsibility: Retrieve paginated pending refill requests for a given provider.
/// </summary>
public class GetPendingRefillsQueryHandler : IQueryHandler<GetPendingRefillsQuery, RefillRequestListDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetPendingRefillsQueryHandler> _logger;

    public GetPendingRefillsQueryHandler(IUnitOfWork unitOfWork, ILogger<GetPendingRefillsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<RefillRequestListDto> Handle(
        GetPendingRefillsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching pending refills for provider {ProviderId}", request.ProviderId);

        var prescriptionRepo = _unitOfWork.Repository<PrescriptionEntity>();
        var prescriptions = await prescriptionRepo.ToListAsync(
            q => q.Where(p => p.ProviderId == request.ProviderId),
            cancellationToken);

        var pendingRefills = prescriptions
            .SelectMany(p => p.Refills
                .Where(r => r.Status == "Pending")
                .Select(r => new RefillRequestDto
                {
                    RefillId = r.Id,
                    PrescriptionId = p.Id,
                    PatientId = p.PatientId,
                    MedicationName = p.MedicationName,
                    RequestedAt = r.RequestedAt,
                    Status = r.Status,
                    PharmacyId = r.PharmacyId
                }))
            .OrderBy(r => r.RequestedAt)
            .ToList();

        var skip = (request.PageNumber - 1) * request.PageSize;
        var items = pendingRefills.Skip(skip).Take(request.PageSize).ToList();

        return new RefillRequestListDto
        {
            Items = items,
            Total = pendingRefills.Count,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}

