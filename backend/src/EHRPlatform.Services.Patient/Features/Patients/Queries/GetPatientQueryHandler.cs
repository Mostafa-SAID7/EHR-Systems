using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Common.Data;
using EHRPlatform.BuildingBlocks.Common.Search;
// Domain entities via GlobalUsings (Domain.Entities)
using EHRPlatform.Services.Patient.Application.PatientManagement.Responses;
using Mapster;

namespace EHRPlatform.Services.Patient.Features.Patients.Queries;

/// <summary>
/// Get patient by ID query handler.
/// Automatically cached by CachingBehavior.
/// </summary>
public class GetPatientQueryHandler : IQueryHandler<GetPatientQuery, PatientResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetPatientQueryHandler> _logger;

    public GetPatientQueryHandler(IUnitOfWork unitOfWork, ILogger<GetPatientQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PatientResponseDto> Handle(
        GetPatientQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching patient {PatientId}", request.PatientId);

        var repo = _unitOfWork.Repository<PatientEntity>();
        var patient = await repo.FirstOrDefaultAsync(
            q => q.Where(p => p.Id == request.PatientId),
            cancellationToken);

        if (patient == null)
            throw new InvalidOperationException($"Patient {request.PatientId} not found");

        return patient.Adapt<PatientResponseDto>();
    }
}

/// <summary>
/// Search patients query handler.
/// Uses Elasticsearch for full-text search.
/// </summary>
public class SearchPatientsQueryHandler : IQueryHandler<SearchPatientsQuery, SearchResultDto<PatientResponseDto>>
{
    private readonly ISearchService _searchService;
    private readonly ILogger<SearchPatientsQueryHandler> _logger;

    public SearchPatientsQueryHandler(ISearchService searchService, ILogger<SearchPatientsQueryHandler> logger)
    {
        _searchService = searchService;
        _logger = logger;
    }

    public async Task<SearchResultDto<PatientResponseDto>> Handle(
        SearchPatientsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Searching patients: {SearchTerm}", request.SearchTerm);

        var results = await _searchService.SearchAsync<Dictionary<string, object>>(
            new EHRPlatform.Common.Search.SearchQuery
            {
                QueryText = request.SearchTerm,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            },
            cancellationToken);

        var patients = results.Hits
            .Select(hit => System.Text.Json.JsonSerializer.Deserialize<PatientResponseDto>(
                System.Text.Json.JsonSerializer.Serialize(hit.Document)))
            .Where(p => p != null)
            .Cast<PatientResponseDto>()
            .ToList();

        return new SearchResultDto<PatientResponseDto>
        {
            Items = patients,
            Total = (int)results.TotalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}

/// <summary>
/// List patients query handler.
/// Paginated list from database.
/// </summary>
public class ListPatientsQueryHandler : IQueryHandler<ListPatientsQuery, SearchResultDto<PatientResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ListPatientsQueryHandler> _logger;

    public ListPatientsQueryHandler(IUnitOfWork unitOfWork, ILogger<ListPatientsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<SearchResultDto<PatientResponseDto>> Handle(
        ListPatientsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Listing patients page {PageNumber}", request.PageNumber);

        var repo = _unitOfWork.Repository<PatientEntity>();

        var skip = (request.PageNumber - 1) * request.PageSize;
        var total = await repo.CountAsync(cancellationToken: cancellationToken);
        var patients = await repo.ToListAsync(
            q => q.OrderByDescending(p => p.CreatedAt).Skip(skip).Take(request.PageSize),
            cancellationToken);

        return new SearchResultDto<PatientResponseDto>
        {
            Items = patients.Adapt<List<PatientResponseDto>>(),
            Total = total,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}

/// <summary>
/// Get patient detail query handler.
/// Includes allergies and conditions.
/// </summary>
public class GetPatientDetailQueryHandler : IQueryHandler<GetPatientDetailQuery, PatientDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetPatientDetailQueryHandler> _logger;

    public GetPatientDetailQueryHandler(IUnitOfWork unitOfWork, ILogger<GetPatientDetailQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PatientDetailDto> Handle(
        GetPatientDetailQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching patient detail {PatientId}", request.PatientId);

        var repo = _unitOfWork.Repository<PatientEntity>();
        var patient = await repo.FirstOrDefaultAsync(
            q => q.Where(p => p.Id == request.PatientId),
            cancellationToken);

        if (patient == null)
            throw new InvalidOperationException($"Patient {request.PatientId} not found");

        var age = DateTime.Now.Year - patient.DateOfBirth.Year;

        var detail = new PatientDetailDto
        {
            Id = patient.Id,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            Email = patient.Email,
            PhoneNumber = patient.PhoneNumber,
            DateOfBirth = patient.DateOfBirth,
            Age = age,
            Gender = patient.Gender,
            MRN = patient.MRN,
            BloodType = patient.BloodType,
            EmergencyContact = patient.EmergencyContact,
            EmergencyPhone = patient.EmergencyPhone,
            Status = patient.Status,
            CreatedAt = patient.CreatedAt,
            LastModifiedAt = patient.UpdatedAt,
            Allergies = patient.Allergies.Select(a => new AllergyDetailDto
            {
                Id = a.Id,
                Allergen = a.Allergen,
                Severity = a.Severity,
                Notes = a.Notes,
                CreatedAt = a.CreatedAt
            }).ToList(),
            Conditions = patient.Conditions.Select(c => new ConditionDetailDto
            {
                Id = c.Id,
                Condition = c.Condition,
                ICD10Code = c.ICD10Code,
                OnsetDate = c.OnsetDate,
                ResolvedDate = c.ResolvedDate,
                CreatedAt = c.CreatedAt
            }).ToList()
        };

        return detail;
    }
}


