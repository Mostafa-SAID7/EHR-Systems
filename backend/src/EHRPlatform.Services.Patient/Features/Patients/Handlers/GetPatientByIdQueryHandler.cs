using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Common.Data.Abstractions;
using EHRPlatform.Services.Patient.Features.Patients.Queries;
using EHRPlatform.Services.Patient.Application.Patients.Responses;
using EHRPlatform.Services.Patient.Application.Patients.Mappers;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Patient.Features.Patients.Handlers;

/// <summary>
/// Get patient by ID query handler.
/// Retrieves single patient with allergies and conditions.
/// </summary>
public class GetPatientByIdQueryHandler : IQueryHandler<GetPatientByIdQuery, PatientResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly PatientMapper _mapper;
    private readonly ILogger<GetPatientByIdQueryHandler> _logger;

    public GetPatientByIdQueryHandler(
        IUnitOfWork unitOfWork,
        PatientMapper mapper,
        ILogger<GetPatientByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger;
    }

    public async Task<PatientResponse> Handle(GetPatientByIdQuery query, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Retrieving patient {PatientId}", query.PatientId);

        var repo = _unitOfWork.Repository<Domain.Entities.Patient>();
        var patient = await repo.GetByIdAsync(query.PatientId, cancellationToken);

        if (patient == null)
            throw new KeyNotFoundException($"Patient {query.PatientId} not found");

        return _mapper.MapToResponse(patient);
    }
}


