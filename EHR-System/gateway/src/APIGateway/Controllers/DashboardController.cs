using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Text.Json;

namespace EHRPlatform.Gateway.Controllers
{
    /// <summary>
    /// Dashboard controller for aggregating data from multiple microservices.
    /// 
    /// Demonstrates the aggregation pattern where the gateway combines data from
    /// multiple services into a single comprehensive response.
    /// 
    /// Example flow:
    /// 1. Client requests /api/v1/dashboard/patient/{patientId}
    /// 2. Gateway calls Patient Service, Appointment Service, Billing Service, Clinical Service in parallel
    /// 3. Gateway aggregates responses into single PatientDashboard model
    /// 4. Returns unified response to client
    /// </summary>
    [ApiController]
    [Route("api/v1/dashboard")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IServiceRegistry _serviceRegistry;
        private readonly IGatewayMetrics _metrics;
        private readonly ILogger<DashboardController> _logger;
        private readonly IMemoryCache _cache;

        public DashboardController(
            IHttpClientFactory httpClientFactory,
            IServiceRegistry serviceRegistry,
            IGatewayMetrics metrics,
            ILogger<DashboardController> logger,
            IMemoryCache cache)
        {
            _httpClientFactory = httpClientFactory;
            _serviceRegistry = serviceRegistry;
            _metrics = metrics;
            _logger = logger;
            _cache = cache;
        }

        /// <summary>
        /// Get complete patient dashboard - aggregates data from multiple services.
        /// 
        /// Calls:
        /// - Patient Service → Patient details
        /// - Appointment Service → Upcoming appointments
        /// - Billing Service → Outstanding invoices
        /// - Clinical Service → Recent clinical notes
        /// 
        /// Returns all data in single response.
        /// </summary>
        [HttpGet("patient/{patientId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPatientDashboard(string patientId, CancellationToken cancellationToken = default)
        {
            try
            {
                var cacheKey = $"dashboard:patient:{patientId}";
                if (_cache.TryGetValue(cacheKey, out PatientDashboardResponse cachedResponse))
                {
                    _logger.LogInformation("Dashboard cache hit for patient {PatientId}", patientId);
                    _metrics.RecordCacheHit(cacheKey);
                    return Ok(cachedResponse);
                }

                _metrics.RecordCacheMiss(cacheKey);

                var userId = User.FindFirst("sub")?.Value ?? "unknown";
                var httpClient = _httpClientFactory.CreateClient();

                var patientService = _serviceRegistry.GetService("patient");
                var appointmentService = _serviceRegistry.GetService("appointment");
                var billingService = _serviceRegistry.GetService("billing");
                var clinicalService = _serviceRegistry.GetService("clinical");

                // Call all services in parallel
                var patientTask = FetchServiceDataAsync<PatientData>(
                    httpClient,
                    $"{patientService.BaseUrl}/api/v1/patients/{patientId}",
                    "Patient Service",
                    cancellationToken);

                var appointmentsTask = FetchServiceDataAsync<List<AppointmentData>>(
                    httpClient,
                    $"{appointmentService.BaseUrl}/api/v1/appointments/patient/{patientId}",
                    "Appointment Service",
                    cancellationToken);

                var billingTask = FetchServiceDataAsync<BillingData>(
                    httpClient,
                    $"{billingService.BaseUrl}/api/v1/invoices/patient/{patientId}",
                    "Billing Service",
                    cancellationToken);

                var clinicalTask = FetchServiceDataAsync<List<ClinicalNoteData>>(
                    httpClient,
                    $"{clinicalService.BaseUrl}/api/v1/notes/patient/{patientId}",
                    "Clinical Service",
                    cancellationToken);

                // Wait for all calls
                await Task.WhenAll(patientTask, appointmentsTask, billingTask, clinicalTask);

                // Build response
                var response = new PatientDashboardResponse
                {
                    PatientId = patientId,
                    Patient = patientTask.Result,
                    UpcomingAppointments = appointmentsTask.Result?
                        .Where(a => a.DateTime > DateTime.UtcNow)
                        .OrderBy(a => a.DateTime)
                        .Take(5)
                        .ToList() ?? new(),
                    Billing = billingTask.Result,
                    RecentClinicalNotes = clinicalTask.Result?
                        .OrderByDescending(c => c.CreatedAt)
                        .Take(5)
                        .ToList() ?? new(),
                    GeneratedAt = DateTime.UtcNow,
                    TraceId = HttpContext.TraceIdentifier
                };

                // Cache for 5 minutes
                _cache.Set(cacheKey, response, TimeSpan.FromMinutes(5));

                _logger.LogInformation(
                    "Patient dashboard aggregated successfully. PatientId: {PatientId}, UserId: {UserId}",
                    patientId, userId);

                return Ok(response);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error fetching patient dashboard data");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    error = "One or more backend services are unavailable",
                    details = ex.Message,
                    traceId = HttpContext.TraceIdentifier
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating patient dashboard");
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    error = "An error occurred while generating the dashboard",
                    details = ex.Message,
                    traceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Get provider dashboard - aggregates appointments and patient feedback.
        /// </summary>
        [HttpGet("provider/{providerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProviderDashboard(string providerId, CancellationToken cancellationToken = default)
        {
            try
            {
                var cacheKey = $"dashboard:provider:{providerId}";
                if (_cache.TryGetValue(cacheKey, out ProviderDashboardResponse cachedResponse))
                {
                    _metrics.RecordCacheHit(cacheKey);
                    return Ok(cachedResponse);
                }

                _metrics.RecordCacheMiss(cacheKey);

                var httpClient = _httpClientFactory.CreateClient();
                var appointmentService = _serviceRegistry.GetService("appointment");
                var analyticsService = _serviceRegistry.GetService("analytics");

                var appointmentsTask = FetchServiceDataAsync<List<AppointmentData>>(
                    httpClient,
                    $"{appointmentService.BaseUrl}/api/v1/appointments/provider/{providerId}",
                    "Appointment Service",
                    cancellationToken);

                var analyticsTask = FetchServiceDataAsync<ProviderAnalyticsData>(
                    httpClient,
                    $"{analyticsService.BaseUrl}/api/v1/analytics/provider/{providerId}",
                    "Analytics Service",
                    cancellationToken);

                await Task.WhenAll(appointmentsTask, analyticsTask);

                var response = new ProviderDashboardResponse
                {
                    ProviderId = providerId,
                    TodayAppointments = appointmentsTask.Result?
                        .Where(a => a.DateTime.Date == DateTime.Today)
                        .ToList() ?? new(),
                    UpcomingAppointments = appointmentsTask.Result?
                        .Where(a => a.DateTime > DateTime.UtcNow && a.DateTime.Date > DateTime.Today)
                        .OrderBy(a => a.DateTime)
                        .Take(10)
                        .ToList() ?? new(),
                    Analytics = analyticsTask.Result,
                    GeneratedAt = DateTime.UtcNow,
                    TraceId = HttpContext.TraceIdentifier
                };

                _cache.Set(cacheKey, response, TimeSpan.FromMinutes(5));

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating provider dashboard");
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    error = "An error occurred while generating the dashboard",
                    traceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// System-wide analytics dashboard - aggregates KPIs from all services.
        /// </summary>
        [HttpGet("analytics")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAnalyticsDashboard(CancellationToken cancellationToken = default)
        {
            try
            {
                const string cacheKey = "dashboard:analytics:system";
                if (_cache.TryGetValue(cacheKey, out AnalyticsDashboardResponse cachedResponse))
                {
                    _metrics.RecordCacheHit(cacheKey);
                    return Ok(cachedResponse);
                }

                _metrics.RecordCacheMiss(cacheKey);

                var httpClient = _httpClientFactory.CreateClient();
                var analyticsService = _serviceRegistry.GetService("analytics");

                var kpis = await FetchServiceDataAsync<SystemKpis>(
                    httpClient,
                    $"{analyticsService.BaseUrl}/api/v1/analytics/kpi",
                    "Analytics Service",
                    cancellationToken);

                var response = new AnalyticsDashboardResponse
                {
                    Kpis = kpis,
                    GeneratedAt = DateTime.UtcNow,
                    TraceId = HttpContext.TraceIdentifier
                };

                _cache.Set(cacheKey, response, TimeSpan.FromMinutes(15));

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating analytics dashboard");
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    error = "An error occurred while generating the dashboard",
                    traceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Helper method to fetch and deserialize data from a service.
        /// </summary>
        private async Task<T?> FetchServiceDataAsync<T>(
            HttpClient httpClient,
            string url,
            string serviceName,
            CancellationToken cancellationToken)
        {
            try
            {
                var response = await httpClient.GetAsync(url, cancellationToken);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    return JsonSerializer.Deserialize<T>(content);
                }

                _logger.LogWarning(
                    "Service {ServiceName} returned status {StatusCode} when fetching {Url}",
                    serviceName, response.StatusCode, url);

                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching data from {ServiceName} at {Url}", serviceName, url);
                throw;
            }
        }
    }

    // Response models
    public class PatientDashboardResponse
    {
        public string PatientId { get; set; } = string.Empty;
        public PatientData? Patient { get; set; }
        public List<AppointmentData> UpcomingAppointments { get; set; } = new();
        public BillingData? Billing { get; set; }
        public List<ClinicalNoteData> RecentClinicalNotes { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
        public string TraceId { get; set; } = string.Empty;
    }

    public class ProviderDashboardResponse
    {
        public string ProviderId { get; set; } = string.Empty;
        public List<AppointmentData> TodayAppointments { get; set; } = new();
        public List<AppointmentData> UpcomingAppointments { get; set; } = new();
        public ProviderAnalyticsData? Analytics { get; set; }
        public DateTime GeneratedAt { get; set; }
        public string TraceId { get; set; } = string.Empty;
    }

    public class AnalyticsDashboardResponse
    {
        public SystemKpis? Kpis { get; set; }
        public DateTime GeneratedAt { get; set; }
        public string TraceId { get; set; } = string.Empty;
    }

    // Data models from services
    public class PatientData
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Mrn { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }

    public class AppointmentData
    {
        public string Id { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string ProviderId { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class BillingData
    {
        public string PatientId { get; set; } = string.Empty;
        public decimal TotalBalance { get; set; }
        public decimal OutstandingBalance { get; set; }
        public List<InvoiceData> RecentInvoices { get; set; } = new();
    }

    public class InvoiceData
    {
        public string Id { get; set; } = string.Empty;
        public DateTime IssuedDate { get; set; }
        public decimal Amount { get; set; }
        public decimal Paid { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class ClinicalNoteData
    {
        public string Id { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string ProviderId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string NoteType { get; set; } = string.Empty;
    }

    public class ProviderAnalyticsData
    {
        public string ProviderId { get; set; } = string.Empty;
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
    }

    public class SystemKpis
    {
        public int TotalPatients { get; set; }
        public int TotalProviders { get; set; }
        public int AppointmentsThisMonth { get; set; }
        public int CompletedAppointmentsThisMonth { get; set; }
        public decimal AveragePatientSatisfaction { get; set; }
        public decimal TotalBillingThisMonth { get; set; }
        public int ActiveUsers { get; set; }
    }

    /// <summary>
    /// Service registry for dashboard aggregation.
    /// </summary>
    public interface IServiceRegistry
    {
        ServiceInfo GetService(string serviceName);
        List<ServiceInfo> GetAllServices();
    }

    public class ServiceInfo
    {
        public string Name { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public int Port { get; set; }
        public bool IsHealthy { get; set; }
    }
}
