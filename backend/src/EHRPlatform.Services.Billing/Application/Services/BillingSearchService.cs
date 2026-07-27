using Elastic.Clients.Elasticsearch;

namespace EHRPlatform.Services.Billing.Application.Services;

/// <summary>
/// Billing search service using Elasticsearch.
/// Provides full-text search on invoices, payments, insurance claims.
/// Gracefully degrades if Elasticsearch unavailable.
/// </summary>
public interface IBillingSearchService
{
    Task<IEnumerable<Invoice>> SearchInvoicesAsync(string query, int limit = 20, CancellationToken ct = default);
    Task<IEnumerable<Payment>> SearchPaymentsAsync(string query, int limit = 20, CancellationToken ct = default);
    Task<IEnumerable<InsuranceClaim>> SearchClaimsAsync(string query, int limit = 20, CancellationToken ct = default);
    Task IndexInvoiceAsync(Invoice invoice, CancellationToken ct = default);
    Task IndexPaymentAsync(Payment payment, CancellationToken ct = default);
    Task IndexClaimAsync(InsuranceClaim claim, CancellationToken ct = default);
    Task<bool> IsAvailableAsync(CancellationToken ct = default);
}

public class BillingSearchService : IBillingSearchService
{
    private readonly ElasticsearchClient? _client;
    private readonly ILogger<BillingSearchService> _logger;
    private const string InvoicesIndex = "billing-invoices";
    private const string PaymentsIndex = "billing-payments";
    private const string ClaimsIndex = "billing-claims";

    public BillingSearchService(ElasticsearchClient? client, ILogger<BillingSearchService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<IEnumerable<Invoice>> SearchInvoicesAsync(string query, int limit = 20, CancellationToken ct = default)
    {
        if (_client == null || string.IsNullOrWhiteSpace(query))
            return Enumerable.Empty<Invoice>();

        try
        {
            _logger.LogInformation("Searching invoices: {Query}", query);

            var response = await _client.SearchAsync<Invoice>(s => s
                .Index(InvoicesIndex)
                .Query(q => q
                    .MultiMatch(mm => mm
                        .Query(query)
                        .Fields(new Elastic.Clients.Elasticsearch.Field[] { "invoiceNumber^2", "patientName", "description" })))
                .Size(limit),
                ct);

            if (!response.IsValidResponse)
            {
                _logger.LogWarning("Elasticsearch search failed: {Error}", response.DebugInformation);
                return Enumerable.Empty<Invoice>();
            }

            return response.Documents;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error searching invoices");
            return Enumerable.Empty<Invoice>();
        }
    }

    public async Task<IEnumerable<Payment>> SearchPaymentsAsync(string query, int limit = 20, CancellationToken ct = default)
    {
        if (_client == null || string.IsNullOrWhiteSpace(query))
            return Enumerable.Empty<Payment>();

        try
        {
            _logger.LogInformation("Searching payments: {Query}", query);

            var response = await _client.SearchAsync<Payment>(s => s
                .Index(PaymentsIndex)
                .Query(q => q
                    .MultiMatch(mm => mm
                        .Query(query)
                        .Fields(new Elastic.Clients.Elasticsearch.Field[] { "referenceNumber^2", "method", "status" })))
                .Size(limit),
                ct);

            if (!response.IsValidResponse)
            {
                _logger.LogWarning("Elasticsearch search failed: {Error}", response.DebugInformation);
                return Enumerable.Empty<Payment>();
            }

            return response.Documents;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error searching payments");
            return Enumerable.Empty<Payment>();
        }
    }

    public async Task<IEnumerable<InsuranceClaim>> SearchClaimsAsync(string query, int limit = 20, CancellationToken ct = default)
    {
        if (_client == null || string.IsNullOrWhiteSpace(query))
            return Enumerable.Empty<InsuranceClaim>();

        try
        {
            _logger.LogInformation("Searching insurance claims: {Query}", query);

            var response = await _client.SearchAsync<InsuranceClaim>(s => s
                .Index(ClaimsIndex)
                .Query(q => q
                    .MultiMatch(mm => mm
                        .Query(query)
                        .Fields(new Elastic.Clients.Elasticsearch.Field[] { "claimNumber^2", "status", "insuranceProvider" })))
                .Size(limit),
                ct);

            if (!response.IsValidResponse)
            {
                _logger.LogWarning("Elasticsearch search failed: {Error}", response.DebugInformation);
                return Enumerable.Empty<InsuranceClaim>();
            }

            return response.Documents;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error searching claims");
            return Enumerable.Empty<InsuranceClaim>();
        }
    }

    public async Task IndexInvoiceAsync(Invoice invoice, CancellationToken ct = default)
    {
        if (_client == null || invoice == null)
            return;

        try
        {
            await _client.IndexAsync(invoice, i => i
                .Index(InvoicesIndex)
                .Id(invoice.Id.ToString()),
                ct);

            _logger.LogDebug("Indexed invoice: {InvoiceId}", invoice.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to index invoice: {InvoiceId}", invoice.Id);
        }
    }

    public async Task IndexPaymentAsync(Payment payment, CancellationToken ct = default)
    {
        if (_client == null || payment == null)
            return;

        try
        {
            await _client.IndexAsync(payment, i => i
                .Index(PaymentsIndex)
                .Id(payment.Id.ToString()),
                ct);

            _logger.LogDebug("Indexed payment: {PaymentId}", payment.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to index payment: {PaymentId}", payment.Id);
        }
    }

    public async Task IndexClaimAsync(InsuranceClaim claim, CancellationToken ct = default)
    {
        if (_client == null || claim == null)
            return;

        try
        {
            await _client.IndexAsync(claim, i => i
                .Index(ClaimsIndex)
                .Id(claim.Id.ToString()),
                ct);

            _logger.LogDebug("Indexed claim: {ClaimId}", claim.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to index claim: {ClaimId}", claim.Id);
        }
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
