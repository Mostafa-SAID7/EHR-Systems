#nullable enable

namespace EHRPlatform.Common.Application.Behaviors;

using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

/// <summary>
/// MediatR pipeline behavior for logging requests and responses.
/// Logs execution time and any exceptions that occur.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var correlationId = GetCorrelationIdFromRequest(request);

        _logger.LogInformation(
            "Handling {RequestName} {@Request} [CorrelationId: {CorrelationId}]",
            requestName,
            request,
            correlationId);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await next();
            stopwatch.Stop();

            _logger.LogInformation(
                "Handled {RequestName} successfully in {ElapsedMilliseconds}ms [CorrelationId: {CorrelationId}]",
                requestName,
                stopwatch.ElapsedMilliseconds,
                correlationId);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Error handling {RequestName} after {ElapsedMilliseconds}ms: {ErrorMessage} [CorrelationId: {CorrelationId}]",
                requestName,
                stopwatch.ElapsedMilliseconds,
                ex.Message,
                correlationId);

            throw;
        }
    }

    private static string? GetCorrelationIdFromRequest(TRequest request)
    {
        // Try to get CorrelationId if the request has it
        var correlationIdProperty = typeof(TRequest)
            .GetProperties()
            .FirstOrDefault(p => p.Name == "CorrelationId");

        return correlationIdProperty?.GetValue(request) as string;
    }
}

