using Elastic.Clients.Elasticsearch;

namespace EHRPlatform.Services.Appointment.Application.Services;

public interface IAppointmentSearchService
{
    Task<bool> IsAvailableAsync(CancellationToken ct = default);
}

public class AppointmentSearchService : IAppointmentSearchService
{
    private readonly ElasticsearchClient? _client;
    private readonly ILogger<AppointmentSearchService> _logger;
    private const string AppointmentsIndex = "appointment-appointments";

    public AppointmentSearchService(ElasticsearchClient? client, ILogger<AppointmentSearchService> logger)
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
