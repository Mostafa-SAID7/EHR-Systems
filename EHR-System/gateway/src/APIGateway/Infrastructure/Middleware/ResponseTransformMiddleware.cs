namespace EHRPlatform.Gateway.Infrastructure.Middleware;

/// <summary>
/// Middleware to transform responses from microservices into unified format.
/// </summary>
public class ResponseTransformMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ResponseTransformMiddleware> _logger;

    public ResponseTransformMiddleware(RequestDelegate next, ILogger<ResponseTransformMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Capture original response body
        var originalBodyStream = context.Response.Body;

        using (var responseBody = new MemoryStream())
        {
            context.Response.Body = responseBody;

            await _next(context);

            // Read the response body
            responseBody.Seek(0, SeekOrigin.Begin);
            using (var reader = new StreamReader(responseBody))
            {
                var responseText = await reader.ReadToEndAsync();

                // Write back to original stream
                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }

        context.Response.Body = originalBodyStream;
    }
}
