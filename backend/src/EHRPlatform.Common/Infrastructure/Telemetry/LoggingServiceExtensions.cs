#nullable enable

using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace EHRPlatform.Common.Infrastructure.Telemetry;

/// <summary>
/// Extension methods for registering Serilog logging services.
/// Single responsibility: Manage structured logging configuration.
/// </summary>
public static class LoggingServiceExtensions
{
    /// <summary>
    /// Add Serilog structured logging with console and file sinks.
    /// Writes daily rolling logs to logs/ehr-platform-.txt.
    /// Call this early in Program.cs before any other services.
    /// </summary>
    public static IServiceCollection AddSerilogLogging(this IServiceCollection services)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File(
                "logs/ehr-platform-.txt",
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .Enrich.FromLogContext()
            .CreateLogger();

        services.AddLogging(logBuilder =>
        {
            logBuilder.ClearProviders();
            logBuilder.AddSerilog();
        });

        return services;
    }
}
