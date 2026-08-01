using MediatR;
using EHRPlatform.Services.Patient.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Patient.Application.Features.MedicalHistory.Queries;

/// <summary>
/// Handler for SearchMedicalHistoryQuery.
/// Searches medical history by keyword or ICD-10 code.
/// </summary>
public class SearchMedicalHistoryQueryHandler : IRequestHandler<SearchMedicalHistoryQuery, SearchMedicalHistoryResponse>
{
    private readonly IPatientDbContext _context;
    private readonly ILogger<SearchMedicalHistoryQueryHandler> _logger;

    public SearchMedicalHistoryQueryHandler(
        IPatientDbContext context,
        ILogger<SearchMedicalHistoryQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<SearchMedicalHistoryResponse> Handle(
        SearchMedicalHistoryQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Searching medical history for patient {PatientId}, term: {SearchTerm}",
            request.PatientId, request.SearchTerm);

        try
        {
            if (string.IsNullOrWhiteSpace(request.SearchTerm))
                return new SearchMedicalHistoryResponse
                {
                    Results = new(),
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalCount = 0,
                    TotalPages = 0
                };

            var searchTerm = request.SearchTerm.ToLowerInvariant();

            var query = _context.Patients
                .Where(p => p.Id == request.PatientId)
                .SelectMany(p => p.MedicalHistory ?? new List<Domain.MedicalHistoryEntry>())
                .Where(h =>
                    h.Description.ToLower().Contains(searchTerm) ||
                    (h.ICD10Code != null && h.ICD10Code.Contains(searchTerm)) ||
                    (h.Notes != null && h.Notes.ToLower().Contains(searchTerm)));

            var totalCount = await query.CountAsync(cancellationToken);

            var results = await query
                .OrderByDescending(h => h.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new SearchMedicalHistoryResponse
            {
                Results = results.Select(MapToDto).ToList(),
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching medical history for patient {PatientId}", request.PatientId);
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
