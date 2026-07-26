using EHRPlatform.Common.CQRS;
using EHRPlatform.Common.Data;
using EHRPlatform.Services.Prescription.Application.PrescriptionManagement.Responses;
using EHRPlatform.Services.Prescription.Domain.Entities;
using Mapster;

namespace EHRPlatform.Services.Prescription.Features.Prescriptions.Queries;

/// <summary>
/// Get patient active prescriptions handler.
/// Single Responsibility: Retrieve paginated active prescriptions for a given patient.
/// </summary>
public class GetPatientActivePrescriptionsQueryHandler : IQueryHandler<GetPatientActivePrescriptionsQuery, PrescriptionListDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetPatientActivePrescriptionsQueryHandler> _logger;

    public GetPatientActivePrescriptionsQueryHandler(IUnitOfWork unitOfWork, ILogger<GetPatientActivePrescriptionsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PrescriptionListDto> Handle(
        GetPatientActivePrescriptionsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching active prescriptions for patient {PatientId}", request.PatientId);

        var repo = _unitOfWork.Repository<PrescriptionEntity>();
        var skip = (request.PageNumber - 1) * request.PageSize;

        var total = await repo.CountAsync(
            q => q.Where(p => p.PatientId == request.PatientId && p.Status == "Active"),
            cancellationToken);

        var prescriptions = await repo.ToListAsync(
            q => q.Where(p => p.PatientId == request.PatientId && p.Status == "Active")
                .OrderByDescending(p => p.StartDate)
                .Skip(skip)
                .Take(request.PageSize),
            cancellationToken);

        return new PrescriptionListDto
        {
            Items = prescriptions.Adapt<List<PrescriptionResponseDto>>(),
            Total = total,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}

