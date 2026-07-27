using Elastic.Clients.Elasticsearch;

namespace EHRPlatform.Services.Patient.Application.Services;

public interface IPatientSearchService
{
    Task<bool> IsAvailableAsync(CancellationToken ct = default);
}

public class PatientSearchService : IPatientSearchService
{
    private readonly ElasticsearchClient? _client;
    private readonly ILogger<PatientSearchService> _logger;

    public PatientSearchService(ElasticsearchClient? client, ILogger<PatientSearchService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<bool> IsAvailableAsync(CancellationToken ct = default)
    {
        if (_client == null)
            return false;

        try
        {
            var response = await _client.PingAsync(ct);
            return response.IsValidResponse;
        }
        catch
        {
            return false;
        }
    }
}
