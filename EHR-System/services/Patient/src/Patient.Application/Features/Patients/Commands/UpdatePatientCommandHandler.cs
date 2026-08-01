namespace EHRPlatform.Services.Patient.Application.Features.Patients.Commands;

using MediatR;
using EHRPlatform.Services.Patient.Persistence;
using EHRPlatform.Services.Patient.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handler for UpdatePatientCommand - Updates patient information.
/// Invalidates cache after update.
/// </summary>
public class UpdatePatientCommandHandler : IRequestHandler<UpdatePatientCommand, UpdatePatientResponse>
{
    private readonly IPatientDbContext _context;
    private readonly IPatientCacheService _cacheService;
    private readonly ILogger<UpdatePatientCommandHandler> _logger;

    public UpdatePatientCommandHandler(
        IPatientDbContext context,
        IPatientCacheService cacheService,
        ILogger<UpdatePatientCommandHandler> logger)
    {
        _context = context;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<UpdatePatientResponse> Handle(UpdatePatientCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating patient {PatientId}", request.PatientId);

        try
        {
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.Id == request.PatientId, cancellationToken);

            if (patient == null)
            {
                return new UpdatePatientResponse
                {
                    Success = false,
                    Message = "Patient not found"
                };
            }

            // Update only provided fields
            if (!string.IsNullOrEmpty(request.Email))
                patient.Email = request.Email;
            
            if (!string.IsNullOrEmpty(request.Phone))
                patient.Phone = request.Phone;
            
            if (!string.IsNullOrEmpty(request.PreferredContactMethod))
                patient.PreferredContactMethod = request.PreferredContactMethod;

            if (!string.IsNullOrEmpty(request.Street))
            {
                patient.Street = request.Street;
                patient.City = request.City ?? patient.City;
                patient.State = request.State ?? patient.State;
                patient.ZipCode = request.ZipCode ?? patient.ZipCode;
                patient.Country = request.Country ?? patient.Country;
            }

            if (!string.IsNullOrEmpty(request.BloodType))
                patient.BloodType = request.BloodType;

            patient.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            // Invalidate cache
            await _cacheService.InvalidatePatientAsync(request.PatientId, cancellationToken);

            _logger.LogInformation("Patient updated successfully");

            return new UpdatePatientResponse
            {
                Success = true,
                Message = "Patient updated successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating patient");
            return new UpdatePatientResponse
            {
                Success = false,
                Message = "An error occurred while updating the patient"
            };
        }
    }
}
