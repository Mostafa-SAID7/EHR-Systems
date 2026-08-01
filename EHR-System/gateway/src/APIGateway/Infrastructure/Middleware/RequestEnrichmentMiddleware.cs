namespace EHRPlatform.Gateway.Infrastructure.Middleware;

using System.Security.Claims;

/// <summary>
/// Middleware to enrich incoming requests with user information from JWT.
/// Adds user ID, roles, and permissions to request headers for downstream services.
/// </summary>
public class RequestEnrichmentMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestEnrichmentMiddleware> _logger;

    public RequestEnrichmentMiddleware(RequestDelegate next, ILogger<RequestEnrichmentMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Extract user info from JWT claims
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                      ?? context.User.FindFirst("sub")?.Value;
            var email = context.User.FindFirst(ClaimTypes.Email)?.Value;
            var roles = string.Join(",", context.User.FindAll(ClaimTypes.Role).Select(c => c.Value));

            // Add to request headers (for downstream services)
            if (!string.IsNullOrEmpty(userId))
            {
                context.Request.Headers.Add("X-User-Id", userId);
            }

            if (!string.IsNullOrEmpty(email))
            {
                context.Request.Headers.Add("X-User-Email", email);
            }

            if (!string.IsNullOrEmpty(roles))
            {
                context.Request.Headers.Add("X-User-Roles", roles);
            }

            // Add to context items
            context.Items["UserId"] = userId;
            context.Items["Email"] = email;
            context.Items["Roles"] = roles;

            _logger.LogInformation("Request enriched for user {UserId}", userId);
        }

        await _next(context);
    }
}
