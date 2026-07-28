using EHRPlatform.Services.Clinical.Data.Documents;

namespace EHRPlatform.Services.Clinical.Data.Repositories;

/// <summary>
/// MongoDB-backed repository for ClinicalNote documents.
/// Vitals, Diagnoses and Procedures are embedded in the note document —
/// no separate collections needed.
/// </summary>
public interface IClinicalNoteMongoRepository
{
    Task<ClinicalNoteDocument?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<IEnumerable<ClinicalNoteDocument>> GetByPatientIdAsync(
        Guid patientId, CancellationToken ct = default);

    Task<(IEnumerable<ClinicalNoteDocument> items, long total)> GetPagedByPatientAsync(
        Guid patientId, int page, int size, CancellationToken ct = default);

    Task InsertAsync(ClinicalNoteDocument doc, CancellationToken ct = default);
    Task ReplaceAsync(ClinicalNoteDocument doc, CancellationToken ct = default);
    Task SoftDeleteAsync(Guid id, CancellationToken ct = default);
}
