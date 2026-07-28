using EHRPlatform.Common.Data;
using EHRPlatform.Services.Patient.Data.Documents;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace EHRPlatform.Services.Patient.Data.Repositories;

/// <summary>
/// MongoDB implementation of IPatientMongoRepository.
/// Wraps the generic IMongoRepository with patient-specific query helpers.
/// </summary>
public sealed class PatientMongoRepository : IPatientMongoRepository
{
    private readonly IMongoRepository<PatientDocument> _repo;

    public PatientMongoRepository(IMongoRepository<PatientDocument> repo)
        => _repo = repo;

    public Task<PatientDocument?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _repo.GetByEntityIdAsync(id, ct);

    public Task<PatientDocument?> GetByMrnAsync(string mrn, CancellationToken ct = default)
        => _repo.FindOneAsync(p => p.MRN == mrn && p.DeletedAt == null, ct);

    public Task<PatientDocument?> GetByEmailAsync(string email, CancellationToken ct = default)
        => _repo.FindOneAsync(p => p.Email == email && p.DeletedAt == null, ct);

    public async Task<(IEnumerable<PatientDocument> items, long total)> GetPagedAsync(
        int page, int size, string? status = null, CancellationToken ct = default)
    {
        Expression<Func<PatientDocument, bool>>? filter = status == null
            ? null
            : p => p.Status == status && p.DeletedAt == null;

        var result = await _repo.GetPagedAsync(page, size, filter, ct);
        return (result.items, result.totalCount);
    }

    public Task<IEnumerable<PatientDocument>> SearchByNameAsync(string query, CancellationToken ct = default)
        => _repo.FindAsync(p =>
            p.DeletedAt == null &&
            (p.FirstName.ToLower().Contains(query.ToLower()) ||
             p.LastName.ToLower().Contains(query.ToLower())), ct);

    public Task InsertAsync(PatientDocument doc, CancellationToken ct = default)
        => _repo.InsertAsync(doc, ct);

    public Task ReplaceAsync(PatientDocument doc, CancellationToken ct = default)
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

    public Task<bool> ExistsByMrnAsync(string mrn, CancellationToken ct = default)
        => _repo.AnyAsync(p => p.MRN == mrn && p.DeletedAt == null, ct);

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
        => _repo.AnyAsync(p => p.Email == email && p.DeletedAt == null, ct);
}
