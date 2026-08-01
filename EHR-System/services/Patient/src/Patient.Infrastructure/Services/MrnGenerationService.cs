namespace EHRPlatform.Services.Patient.Infrastructure.Services;

using EHRPlatform.Services.Patient.Application.Services;
using EHRPlatform.Services.Patient.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Service for generating unique Medical Record Numbers.
/// Format: MRN-YYYY-XXXXXX (6-digit sequential per year)
/// </summary>
public class MrnGenerationService : IMrnGenerationService
{
    private readonly IPatientDbContext _context;
    private readonly ILogger<MrnGenerationService> _logger;

    public MrnGenerationService(IPatientDbContext context, ILogger<MrnGenerationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<string> GenerateMrnAsync(CancellationToken cancellationToken = default)
    {
        var currentYear = DateTime.UtcNow.Year;
        var lastPatient = await _context.Patients
            .Where(p => p.Mrn.StartsWith($"MRN-{currentYear}"))
            .OrderByDescending(p => p.Mrn)
            .FirstOrDefaultAsync(cancellationToken);

        int nextSequence = 1;

        if (lastPatient != null && IsValidMrn(lastPatient.Mrn))
        {
            var (year, sequence) = ParseMrn(lastPatient.Mrn);
            if (year == currentYear)
            {
                nextSequence = sequence + 1;
            }
        }

        var mrn = $"MRN-{currentYear}-{nextSequence:D6}";
        _logger.LogInformation("Generated MRN: {Mrn}", mrn);

        return mrn;
    }

    public bool IsValidMrn(string mrn)
    {
        // Format: MRN-YYYY-XXXXXX
        if (string.IsNullOrEmpty(mrn) || !mrn.StartsWith("MRN-"))
            return false;

        var parts = mrn.Split('-');
        if (parts.Length != 3)
            return false;

        if (!int.TryParse(parts[1], out var year) || year < 2000 || year > DateTime.UtcNow.Year)
            return false;

        if (!int.TryParse(parts[2], out var sequence) || sequence < 1 || sequence > 999999)
            return false;

        return true;
    }

    public (int Year, int Sequence) ParseMrn(string mrn)
    {
        var parts = mrn.Split('-');
        var year = int.Parse(parts[1]);
        var sequence = int.Parse(parts[2]);
        return (year, sequence);
    }
}
