namespace EHRPlatform.Services.Patient.Infrastructure.Services;

using EHRPlatform.Services.Patient.Application.Services;
using EHRPlatform.Services.Patient.Application.Features.Patients.Queries;
using StackExchange.Redis;
using System.Text.Json;
using Microsoft.Extensions.Logging;

/// <summary>
/// Service for caching patient data in Redis.
/// </summary>
public class PatientCacheService : IPatientCacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<PatientCacheService> _logger;
    private const string PatientKeyPrefix = "patient:";
    private const string PatientListKeyPrefix = "patient:list:";
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(10);

    public PatientCacheService(IConnectionMultiplexer redis, ILogger<PatientCacheService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<PatientDto?> GetPatientAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = $"{PatientKeyPrefix}{patientId}";
            var value = await db.StringGetAsync(key);

            if (value.IsNull)
            {
                _logger.LogInformation("Patient not in cache: {PatientId}", patientId);
                return null;
            }

            var patient = JsonSerializer.Deserialize<PatientDto>(value.ToString());
            _logger.LogInformation("Patient retrieved from cache: {PatientId}", patientId);
            return patient;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting patient from cache");
            return null;
        }
    }

    public async Task SetPatientAsync(Guid patientId, PatientDto patient, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = $"{PatientKeyPrefix}{patientId}";
            var value = JsonSerializer.Serialize(patient);
            var expiry = ttl ?? DefaultTtl;

            await db.StringSetAsync(key, value, expiry);
            _logger.LogInformation("Patient cached: {PatientId}", patientId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting patient in cache");
        }
    }

    public async Task InvalidatePatientAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = $"{PatientKeyPrefix}{patientId}";
            await db.KeyDeleteAsync(key);
            _logger.LogInformation("Patient cache invalidated: {PatientId}", patientId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating patient cache");
        }
    }

    public async Task<List<PatientDto>?> GetPatientListAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = $"{PatientListKeyPrefix}{pageNumber}:{pageSize}";
            var value = await db.StringGetAsync(key);

            if (value.IsNull)
                return null;

            return JsonSerializer.Deserialize<List<PatientDto>>(value.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting patient list from cache");
            return null;
        }
    }

    public async Task SetPatientListAsync(int pageNumber, int pageSize, List<PatientDto> patients, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = $"{PatientListKeyPrefix}{pageNumber}:{pageSize}";
            var value = JsonSerializer.Serialize(patients);

            await db.StringSetAsync(key, value, TimeSpan.FromMinutes(10));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting patient list in cache");
        }
    }

    public async Task InvalidatePatientListAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var keys = server.Keys(pattern: $"{PatientListKeyPrefix}*");

            foreach (var key in keys)
            {
                await db.KeyDeleteAsync(key);
            }

            _logger.LogInformation("All patient list caches invalidated");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating patient list cache");
        }
    }
}
