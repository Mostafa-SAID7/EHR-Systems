namespace EHRPlatform.Services.Patient.Application.Features.Patients.Commands;

using MediatR;
using EHRPlatform.Services.Patient.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handler for AddAllergyCommand - Adds allergy to patient record.
/// </summary>
public class AddAllergyCommandHandler : IRequestHandler<AddAllergyCommand, AddAllergyResponse>
{
    private readonly IPatientDbContext _context;
    private readonly ILogger<AddAllergyCommandHandler> _logger;

    public AddAllergyCommandHandler(
        IPatientDbContext context,
        ILogger<AddAllergyCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AddAllergyResponse> Handle(AddAllergyCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding allergy to patient {PatientId}: {AllergyName}", request.PatientId, request.AllergyName);

        try
        {
            var patient = await _context.Patients
                .Include(p => p.Allergies)
                .FirstOrDefaultAsync(p => p.Id == request.PatientId, cancellationToken);

            if (patient == null)
            {
                return new AddAllergyResponse
                {
                    Success = false,
                    Message = "Patient not found"
                };
            }

            // Check for duplicate allergy
            var existingAllergy = patient.Allergies
                .FirstOrDefault(a => a.AllergyCode == request.AllergyCode);

            if (existingAllergy != null)
            {
                return new AddAllergyResponse
                {
                    Success = false,
                    Message = "This allergy is already recorded"
                };
            }

            patient.AddAllergy(request.AllergyCode, request.AllergyName, request.Severity, request.ReactionDescription);
            await _context.SaveChangesAsync(cancellationToken);

            var allergyId = patient.Allergies.Last().Id;

            _logger.LogInformation("Allergy added successfully: {AllergyId}", allergyId);

            return new AddAllergyResponse
            {
                Success = true,
                AllergyId = allergyId,
                Message = "Allergy added successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding allergy");
            return new AddAllergyResponse
            {
                Success = false,
                Message = "An error occurred while adding the allergy"
            };
        }
    }
}
