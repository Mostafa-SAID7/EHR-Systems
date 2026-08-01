using EHRPlatform.Common.Domain.Events;
using EHRPlatform.Common.Application.Features.EventDriven.Publishing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Common.Application.Features.EventDriven.Handlers;

/// <summary>
/// Background service that polls and publishes outbox events to message broker.
/// Single responsibility: Background service lifecycle only.
/// Delegates to OutboxProcessingService for actual work.
/// </summary>
public sealed class OutboxProcessorBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxProcessorBackgroundService> _logger;
    private const int PollIntervalSeconds = 5;

    public OutboxProcessorBackgroundService(IServiceProvider serviceProvider, ILogger<OutboxProcessorBackgroundService> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox processor background service starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var processingService = scope.ServiceProvider.GetRequiredService<OutboxProcessingService>();
                
                await processingService.ProcessBatchAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in outbox processor loop");
            }

            await Task.Delay(TimeSpan.FromSeconds(PollIntervalSeconds), stoppingToken);
        }

        _logger.LogInformation("Outbox processor background service stopped");
    }
}
