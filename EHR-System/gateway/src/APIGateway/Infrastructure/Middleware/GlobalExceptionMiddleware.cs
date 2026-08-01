namespace EHRPlatform.Gateway.Infrastructure.Middleware;

using System.Text.Json;

/// <summary>
/// Global exception handler middleware for unified error responses.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = new ApiErrorResponse
        {
            TraceId = context.TraceIdentifier,
            Timestamp = DateTime.UtcNow,
            Message = exception.Message
        };

        // Determine status code
        var statusCode = exception switch
        {
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            ArgumentNullException or ArgumentException => StatusCodes.Status400BadRequest,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            TimeoutException => StatusCodes.Status504GatewayTimeout,
            HttpRequestException ex => ex.StatusCode ?? System.Net.HttpStatusCode.BadGateway,
            _ => StatusCodes.Status500InternalServerError
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;
        response.StatusCode = statusCode;

        if (context.RequestServices.GetService<ILogger<GlobalExceptionMiddleware>>() is { } logger)
        {
            if (statusCode >= 500)
            {
                logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
            }
            else
            {
                logger.LogWarning(exception, "Handled exception: {Message}", exception.Message);
            }
        }

        return context.Response.WriteAsJsonAsync(response);
    }
}

public class ApiErrorResponse
{
    public string TraceId { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
    public string? Details { get; set; }
}
