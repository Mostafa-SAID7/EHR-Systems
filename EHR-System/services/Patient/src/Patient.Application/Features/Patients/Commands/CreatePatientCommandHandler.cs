namespace EHRPlatform.Services.Patient.Application.Features.Patients.Commands;

using MediatR;
using EHRPlatform.Services.Patient.Domain.Entities;
using EHRPlatform.Services.Patient.Persistence;
using EHRPlatform.Services.Patient.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handler for CreatePatientCommand - Creates new patient with auto-generated MRN.
/// </summary>
public class CreatePatientCommandHandler : IRequestHandler<CreatePatientCommand, CreatePatientResponse>
{
    private readonly IPatientDbContext _context;
    private readonly IMrnGenerationService _mrnService;
    private readonly IElasticsearchService _elasticsearchService;
    private readonly ILogger<CreatePatientCommandHandler> _logger;

    public CreatePatientCommandHandler(
        IPatientDbContext context,
        IMrnGenerationService mrnService,
        IElasticsearchService elasticsearchService,
        ILogger<CreatePatientCommandHandler> logger)
    {
        _context = context;
        _mrnService = mrnService;
        _elasticsearchService = elasticsearchService;
        _logger = logger;
    }

    public async Task<CreatePatientResponse> Handle(CreatePatientCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating patient: {FirstName} {LastName}", request.FirstName, request.LastName);

        try
        {
            // Check if patient already exists by email
            var existingPatient = await _context.Patients
                .FirstOrDefaultAsync(p => p.Email == request.Email, cancellationToken);

            if (existingPatient != null)
            {
                return new CreatePatientResponse
                {
                    Success = false,
                    Message = "Patient with this email already exists"
                };
            }

            // Generate MRN
            var mrn = await _mrnService.GenerateMrnAsync(cancellationToken);

            // Create patient
            var patient = new Patient
            {
                Id = Guid.NewGuid(),
                Mrn = mrn,
                FirstName = request.FirstName,
                LastName = request.LastName,
                MiddleName = request.MiddleName,
                Email = request.Email,
                Phone = request.Phone,
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender,
                Street = request.Street,
                City = request.City,
                State = request.State,
                ZipCode = request.ZipCode,
                Country = request.Country,
                EmergencyContactName = request.EmergencyContactName,
                EmergencyContactPhone = request.EmergencyContactPhone,
                EmergencyContactRelationship = request.EmergencyContactRelationship,
                BloodType = request.BloodType,
                PreferredContactMethod = request.PreferredContactMethod,
                CreatedAt = DateTime.UtcNow
            };

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync(cancellationToken);

            // Index in Elasticsearch for full-text search
            var elasticsearchDocument = new
            {
                id = patient.Id,
                mrn = patient.Mrn,
                firstName = patient.FirstName,
                lastName = patient.LastName,
                email = patient.Email,
                phone = patient.Phone,
                dateOfBirth = patient.DateOfBirth,
                gender = patient.Gender,
                city = patient.City,
                state = patient.State,
                createdAt = patient.CreatedAt
            };

            await _elasticsearchService.IndexPatientAsync(patient.Id.ToString(), elasticsearchDocument, cancellationToken);

            _logger.LogInformation("Patient created successfully: MRN={Mrn}, Email={Email}", mrn, request.Email);

            return new CreatePatientResponse
            {
                Success = true,
                PatientId = patient.Id,
                Mrn = mrn,
                Message = "Patient created successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating patient");
            return new CreatePatientResponse
            {
                Success = false,
                Message = "An error occurred while creating the patient"
            };
        }
    }
}
