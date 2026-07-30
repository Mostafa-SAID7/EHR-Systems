using EHRPlatform.Common.Messaging;
using EHRPlatform.Common.Infrastructure.Resilience;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Common.Shared.Extensions;

/// <summary>
/// DI extensions for Polly-based resilience wiring across EHR services.
///
/// Provides two capabilities:
///   1. Wrap IEventPublisher with retry + circuit-breaker (existing).
///   2. Register named resilient HttpClients for inter-service communication.
/// </summary>
public static class ResilienceExtensions
{
    // ── Event publisher resilience ────────────────────────────────────────────

    /// <summary>
    /// Wrap the registered <see cref="IEventPublisher"/> with retry + circuit-breaker.
    /// Register AFTER AddKafkaMessaging() to ensure the inner publisher is in the container.
    /// </summary>
    public static IServiceCollection AddResilientEventPublisher(this IServiceCollection services)
    {
        var innerDescriptor = services.LastOrDefault(d => d.ServiceType == typeof(IEventPublisher));
        if (innerDescriptor == null)
            throw new InvalidOperationException(
                "IEventPublisher must be registered before calling AddResilientEventPublisher.");

        services.RemoveAll<IEventPublisher>();

        services.AddSingleton<IEventPublisher>(sp =>
        {
            IEventPublisher inner;
            if (innerDescriptor.ImplementationInstance != null)
                inner = (IEventPublisher)innerDescriptor.ImplementationInstance;
            else if (innerDescriptor.ImplementationFactory != null)
                inner = (IEventPublisher)innerDescriptor.ImplementationFactory(sp);
            else
                inner = (IEventPublisher)ActivatorUtilities.CreateInstance(sp, innerDescriptor.ImplementationType!);

            return new ResilientEventPublisher(
                inner,
                sp.GetRequiredService<ILogger<ResilientEventPublisher>>());
        });

        return services;
    }

    // ── Resilient HttpClient factories ────────────────────────────────────────

    /// <summary>
    /// Register a named HttpClient with EHR standard resilience:
    /// Retry (3 attempts, exponential backoff) + Circuit Breaker + Timeout.
    /// Uses Microsoft.Extensions.Http.Resilience (Polly v8 API).
    ///
    /// Usage:
    ///   services.AddEHRHttpClient("patient-service", "http://localhost:5002");
    ///   ...
    ///   var client = httpClientFactory.CreateClient("patient-service");
    /// </summary>
    public static IServiceCollection AddEHRHttpClient(
        this IServiceCollection services,
        string clientName,
        string baseAddress,
        int timeoutSeconds = 10)
    {
        services.AddHttpClient(clientName, client =>
            {
                client.BaseAddress = new Uri(baseAddress);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.Timeout = TimeSpan.FromSeconds(timeoutSeconds + 5); // outer safety net
            })
            .AddStandardResilienceHandler(opts =>
            {
                opts.Retry.MaxRetryAttempts = 3;
                opts.AttemptTimeout.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                opts.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(timeoutSeconds * 4);
            });

        return services;
    }

    /// <summary>
    /// Register a typed HttpClient with EHR resilience policies.
    ///
    /// Usage:
    ///   services.AddEHRHttpClient&lt;IPatientServiceClient, PatientServiceClient&gt;("http://localhost:5002");
    /// </summary>
    public static IServiceCollection AddEHRHttpClient<TClient, TImplementation>(
        this IServiceCollection services,
        string baseAddress,
        int timeoutSeconds = 10)
        where TClient : class
        where TImplementation : class, TClient
    {
        services.AddHttpClient<TClient, TImplementation>(client =>
            {
                client.BaseAddress = new Uri(baseAddress);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.Timeout = TimeSpan.FromSeconds(timeoutSeconds + 5);
            })
            .AddStandardResilienceHandler(opts =>
            {
                opts.Retry.MaxRetryAttempts = 3;
                opts.AttemptTimeout.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                opts.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(timeoutSeconds * 4);
            });

        return services;
    }
}

