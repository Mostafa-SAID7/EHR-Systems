namespace Identity.API.Extensions;

using Microsoft.AspNetCore.Builder;

/// <summary>
/// Extension methods for application builder configuration
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Configures health check endpoints
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <returns>The application builder</returns>
    public static IApplicationBuilder UseHealthChecks(this IApplicationBuilder app)
    {
        app.Map("/health", builder =>
        {
            builder.Run(async context =>
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { status = "healthy" });
            });
        });

        app.Map("/ready", builder =>
        {
            builder.Run(async context =>
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { status = "ready" });
            });
        });

        return app;
    }
}
