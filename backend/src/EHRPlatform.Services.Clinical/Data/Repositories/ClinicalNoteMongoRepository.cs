using EHRPlatform.BuildingBlocks.Common.Data;
using EHRPlatform.Services.Clinical.Data.Documents;

namespace EHRPlatform.Services.Clinical.Data.Repositories;

/// <summary>
/// MongoDB implementation of IClinicalNoteMongoRepository.
/// </summary>
public sealed class ClinicalNoteMongoRepository : IClinicalNoteMongoRepository
{
    private readonly IMongoRepository<ClinicalNoteDocument> _repo;

    public ClinicalNoteMongoRepository(IMongoRepository<ClinicalNoteDocument> repo)
        => _repo = repo;

    public Task<ClinicalNoteDocument?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _repo.GetByEntityIdAsync(id, ct);

    public Task<IEnumerable<ClinicalNoteDocument>> GetByPatientIdAsync(
        Guid patientId, CancellationToken ct = default)
        => _repo.FindAsync(n => n.PatientId == patientId && n.DeletedAt == null, ct);

    public async Task<(IEnumerable<ClinicalNoteDocument> items, long total)> GetPagedByPatientAsync(
        Guid patientId, int page, int size, CancellationToken ct = default)
    {
        var result = await _repo.GetPagedAsync(
            page, size,
            filter: n => n.PatientId == patientId && n.DeletedAt == null,
            cancellationToken: ct);
        return (result.items, result.totalCount);
    }

    public Task InsertAsync(ClinicalNoteDocument doc, CancellationToken ct = default)
        => _repo.InsertAsync(doc, ct);

    public Task ReplaceAsync(ClinicalNoteDocument doc, CancellationToken ct = default)
    {
        doc.UpdatedAt = DateTime.UtcNow;
        return _repo.ReplaceAsync(doc, ct);
    }

    public Task SoftDeleteAsync(Guid id, CancellationToken ct = default)
        => _repo.GetByEntityIdAsync(id, ct).ContinueWith(async t =>
        {
            if (t.Result != null)
                await _repo.DeleteAsync(t.Result.Id, ct);
        }, ct).Unwrap();
}

