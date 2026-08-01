using MediatR;
using EHRPlatform.Services.Patient.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Patient.Application.Features.MedicalHistory.Queries;

/// <summary>
/// Handler for GetPatientMedicalHistoryQuery.
/// Retrieves paginated medical history for patient.
/// </summary>
public class GetPatientMedicalHistoryQueryHandler : IRequestHandler<GetPatientMedicalHistoryQuery, GetPatientMedicalHistoryResponse>
{
    private readonly IPatientDbContext _context;
    private readonly ILogger<GetPatientMedicalHistoryQueryHandler> _logger;

    public GetPatientMedicalHistoryQueryHandler(
        IPatientDbContext context,
        ILogger<GetPatientMedicalHistoryQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<GetPatientMedicalHistoryResponse> Handle(
        GetPatientMedicalHistoryQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Getting medical history for patient {PatientId}, page {PageNumber}",
            request.PatientId, request.PageNumber);

        try
        {
            var query = _context.Patients
                .Where(p => p.Id == request.PatientId)
                .SelectMany(p => p.MedicalHistory ?? new List<Domain.MedicalHistoryEntry>());

            if (!string.IsNullOrEmpty(request.HistoryType))
                query = query.Where(h => h.HistoryType == request.HistoryType);

            var totalCount = await query.CountAsync(cancellationToken);

            var entries = await query
                .OrderByDescending(h => h.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new GetPatientMedicalHistoryResponse
            {
                Entries = entries.Select(MapToDto).ToList(),
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting medical history for patient {PatientId}", request.PatientId);
            throw;
        }
    }

    private static MedicalHistoryDto MapToDto(Domain.MedicalHistoryEntry entry)
    {
        return new MedicalHistoryDto
        {
            Id = entry.Id,
            HistoryType = entry.HistoryType,
            Description = entry.Description,
            ICD10Code = entry.ICD10Code,
            OnsetDate = entry.OnsetDate,
            ResolvedDate = entry.ResolvedDate,
            Notes = entry.Notes,
            CreatedAt = entry.CreatedAt,
            UpdatedAt = entry.UpdatedAt
        };
    }
}

/// <summary>
/// Domain entity reference
/// </summary>
namespace EHRPlatform.Services.Patient.Application.Features.MedicalHistory.Domain
{
    public class MedicalHistoryEntry
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string HistoryType { get; set; }
        public string Description { get; set; }
        public string? ICD10Code { get; set; }
        public DateTime? OnsetDate { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
