using EHRPlatform.Services.Patient.Data.Documents;

namespace EHRPlatform.Services.Patient.Persistence.Repositories;

/// <summary>
/// MongoDB-backed repository for Patient documents.
/// All domain persistence for the Patient service goes through this interface.
/// The relational PatientContext is retained only for the transactional outbox.
/// </summary>
public interface IPatientMongoRepository
{
    Task<PatientDocument?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PatientDocument?> GetByMrnAsync(string mrn, CancellationToken ct = default);
    Task<PatientDocument?> GetByEmailAsync(string email, CancellationToken ct = default);

    Task<(IEnumerable<PatientDocument> items, long total)> GetPagedAsync(
        int page, int size, string? status = null, CancellationToken ct = default);

    Task<IEnumerable<PatientDocument>> SearchByNameAsync(
        string query, CancellationToken ct = default);

    Task InsertAsync(PatientDocument doc, CancellationToken ct = default);
    Task ReplaceAsync(PatientDocument doc, CancellationToken ct = default);
    Task SoftDeleteAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExistsByMrnAsync(string mrn, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
}

