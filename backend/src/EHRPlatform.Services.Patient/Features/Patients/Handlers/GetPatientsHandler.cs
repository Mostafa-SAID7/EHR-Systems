using EHRPlatform.Common.CQRS;
using EHRPlatform.Common.Data;
using EHRPlatform.Services.Patient.Features.Patients.Queries;
using EHRPlatform.Services.Patient.Application.Patients.Responses;
using EHRPlatform.Services.Patient.Application.Patients.Mappers;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Patient.Features.Patients.Handlers;

/// <summary>
/// Get patients query handler.
/// Retrieves paginated list of patients with optional filtering.
/// </summary>
public class GetPatientsQueryHandler : IQueryHandler<GetPatientsQuery, PatientListDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly PatientMapper _mapper;
    private readonly ILogger<GetPatientsQueryHandler> _logger;

    public GetPatientsQueryHandler(
        IUnitOfWork unitOfWork,
        PatientMapper mapper,
        ILogger<GetPatientsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger;
    }

    public async Task<PatientListDto> Handle(GetPatientsQuery query, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Retrieving patients, page {PageNumber}, status filter: {Status}", query.PageNumber, query.Status);

        var repo = _unitOfWork.Repository<Domain.Entities.Patient>();

        var total = await repo.CountAsync(
            q => query.Status == null ? q : q.Where(p => p.Status == query.Status),
            cancellationToken);

        var skip = (query.PageNumber - 1) * query.PageSize;
        var patients = await repo.ToListAsync(
            q =>
            {
                var filtered = query.Status == null ? q : q.Where(p => p.Status == query.Status);
                return filtered.OrderByDescending(x => x.CreatedAt).Skip(skip).Take(query.PageSize);
            },
            cancellationToken);

        return _mapper.MapToListDto(patients, total, query.PageNumber, query.PageSize);
    }
}
