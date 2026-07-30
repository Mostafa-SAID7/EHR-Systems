using Elastic.Clients.Elasticsearch;
using EHRPlatform.Common.Infrastructure.Security;
using EHRPlatform.Common.Search;
using Microsoft.Extensions.DependencyInjection;

namespace EHRPlatform.Common.Shared.Extensions.Data;

/// <summary>
/// DI extensions for Elasticsearch search service.
///
/// Typical microservice Program.cs usage:
/// <code>
/// builder.Services
///     .AddElasticsearchSearch(elasticsearchUrl);
/// </code>
/// </summary>
public static class SearchExtensions
{
    /// <summary>
    /// Register Elasticsearch search service.
    /// </summary>
    public static IServiceCollection AddElasticsearchSearch(
        this IServiceCollection services,
        string? elasticsearchUrl)
    {
        if (string.IsNullOrEmpty(elasticsearchUrl))
            throw new ArgumentException("Elasticsearch URL is required.", nameof(elasticsearchUrl));

        var settings = new ElasticsearchClientSettings(new Uri(elasticsearchUrl))
            .DisableDirectStreaming()
            .ThrowExceptions();

        services.AddSingleton(new ElasticsearchClient(settings));
        services.AddSingleton<ISearchService, ElasticsearchService>();

        return services;
    }
}
