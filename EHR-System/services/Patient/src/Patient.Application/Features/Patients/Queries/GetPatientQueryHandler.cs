namespace EHRPlatform.Services.Patient.Application.Features.Patients.Queries;

using MediatR;
using EHRPlatform.Services.Patient.Persistence;
using EHRPlatform.Services.Patient.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handler for GetPatientQuery - Retrieves patient by ID with caching.
/// </summary>
public class GetPatientQueryHandler : IRequestHandler<GetPatientQuery, GetPatientResponse>
{
    private readonly IPatientDbContext _context;
    private readonly IPatientCacheService _cacheService;
    private readonly ILogger<GetPatientQueryHandler> _logger;

    public GetPatientQueryHandler(
        IPatientDbContext context,
        IPatientCacheService cacheService,
        ILogger<GetPatientQueryHandler> logger)
    {
        _context = context;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<GetPatientResponse> Handle(GetPatientQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting patient {PatientId}", request.PatientId);

        try
        {
            // Check cache first
            var cachedPatient = await _cacheService.GetPatientAsync(request.PatientId, cancellationToken);
            if (cachedPatient != null)
            {
                _logger.LogInformation("Patient found in cache");
                return new GetPatientResponse
                {
                    Success = true,
                    Patient = cachedPatient
                };
            }

            // Query database
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.Id == request.PatientId, cancellationToken);

            if (patient == null)
            {
                return new GetPatientResponse
                {
                    Success = false,
                    Message = "Patient not found"
                };
            }

            var patientDto = new PatientDto
            {
                Id = patient.Id,
                Mrn = patient.Mrn,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                Email = patient.Email,
                Phone = patient.Phone,
                DateOfBirth = patient.DateOfBirth,
                Gender = patient.Gender,
                Age = patient.GetAge(),
                BloodType = patient.BloodType,
                Status = patient.Status,
                IsArchived = patient.IsArchived,
                CreatedAt = patient.CreatedAt
            };

            // Cache for 10 minutes
            await _cacheService.SetPatientAsync(request.PatientId, patientDto, TimeSpan.FromMinutes(10), cancellationToken);

            return new GetPatientResponse
            {
                Success = true,
                Patient = patientDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting patient");
            return new GetPatientResponse
            {
                Success = false,
                Message = "An error occurred while retrieving the patient"
            };
        }
    }
}
