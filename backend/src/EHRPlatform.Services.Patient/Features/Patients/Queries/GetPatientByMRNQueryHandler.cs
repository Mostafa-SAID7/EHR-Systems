using EHRPlatform.Common.Application.CQRS;
using EHRPlatform.Common.Data.Abstractions;
using EHRPlatform.Common.Slugs;
// Domain entities via GlobalUsings (Domain.Entities)
using EHRPlatform.Services.Patient.Application.PatientManagement.Responses;
using Mapster;

namespace EHRPlatform.Services.Patient.Features.Patients.Queries;

/// <summary>
/// Get patient by MRN query handler.
/// Automatically cached by CachingBehavior.
/// Generates MRN slug for response.
/// </summary>
public class GetPatientByMRNQueryHandler : IQueryHandler<GetPatientByMRNQuery, PatientResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISlugGenerator _slugGenerator;
    private readonly ILogger<GetPatientByMRNQueryHandler> _logger;

    public GetPatientByMRNQueryHandler(
        IUnitOfWork unitOfWork,
        ISlugGenerator slugGenerator,
        ILogger<GetPatientByMRNQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _slugGenerator = slugGenerator;
        _logger = logger;
    }

    public async Task<PatientResponseDto> Handle(
        GetPatientByMRNQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching patient by MRN {MRN}", request.MRN);

        var repo = _unitOfWork.Repository<PatientEntity>();
        var patient = await repo.FirstOrDefaultAsync(
            q => q.Where(p => p.MRN == request.MRN),
            cancellationToken);

        if (patient == null)
            throw new InvalidOperationException($"Patient with MRN {request.MRN} not found");

        var dto = patient.Adapt<PatientResponseDto>();
        
        // Generate MRN slug for URL-friendly access
        dto.MRNSlug = _slugGenerator.Generate($"mrn-{patient.MRN}");
        dto.Slug = dto.MRNSlug;
        dto.SlugDisplayName = patient.MRN;

        return dto;
    }
}

/// <summary>
/// Get patient detail by MRN query handler.
/// Includes allergies and conditions.
/// Automatically cached.
/// </summary>
public class GetPatientDetailByMRNQueryHandler : IQueryHandler<GetPatientDetailByMRNQuery, PatientDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISlugGenerator _slugGenerator;
    private readonly ILogger<GetPatientDetailByMRNQueryHandler> _logger;

    public GetPatientDetailByMRNQueryHandler(
        IUnitOfWork unitOfWork,
        ISlugGenerator slugGenerator,
        ILogger<GetPatientDetailByMRNQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _slugGenerator = slugGenerator;
        _logger = logger;
    }

    public async Task<PatientDetailDto> Handle(
        GetPatientDetailByMRNQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching patient detail by MRN {MRN}", request.MRN);

        var repo = _unitOfWork.Repository<PatientEntity>();
        var patient = await repo.FirstOrDefaultAsync(
            q => q.Where(p => p.MRN == request.MRN),
            cancellationToken);

        if (patient == null)
            throw new InvalidOperationException($"Patient with MRN {request.MRN} not found");

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

        // Generate MRN slug for URL-friendly access
        detail.MRNSlug = _slugGenerator.Generate($"mrn-{patient.MRN}");
        detail.Slug = detail.MRNSlug;
        detail.SlugDisplayName = patient.MRN;

        return detail;
    }
}

