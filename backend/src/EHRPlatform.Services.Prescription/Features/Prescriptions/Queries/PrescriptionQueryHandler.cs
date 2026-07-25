using EHRPlatform.Common.CQRS;
using EHRPlatform.Common.Data;
using EHRPlatform.Services.Prescription.Application.PrescriptionManagement.Responses;
using EHRPlatform.Services.Prescription.Domain.Entities;
using Mapster;

namespace EHRPlatform.Services.Prescription.Features.Prescriptions.Queries;

/// <summary>
/// Get prescription by ID handler.
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

        var repo = _unitOfWork.Repository<Prescription>();
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

/// <summary>
/// Get patient active prescriptions handler.
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

        var repo = _unitOfWork.Repository<Prescription>();
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

/// <summary>
/// Get patient prescription history handler.
/// </summary>
public class GetPatientPrescriptionHistoryQueryHandler : IQueryHandler<GetPatientPrescriptionHistoryQuery, PrescriptionListDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetPatientPrescriptionHistoryQueryHandler> _logger;

    public GetPatientPrescriptionHistoryQueryHandler(IUnitOfWork unitOfWork, ILogger<GetPatientPrescriptionHistoryQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PrescriptionListDto> Handle(
        GetPatientPrescriptionHistoryQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching prescription history for patient {PatientId}", request.PatientId);

        var repo = _unitOfWork.Repository<Prescription>();
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

        return new PrescriptionListDto
        {
            Items = prescriptions.Adapt<List<PrescriptionResponseDto>>(),
            Total = total,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}

/// <summary>
/// Get pending refills handler.
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

        var prescriptionRepo = _unitOfWork.Repository<Prescription>();
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
