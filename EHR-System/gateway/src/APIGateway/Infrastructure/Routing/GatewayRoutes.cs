namespace EHRPlatform.Gateway.Infrastructure.Routing;

/// <summary>
/// Extension methods for mapping gateway routes.
/// </summary>
public static class GatewayRoutesExtensions
{
    public static void MapGatewayRoutes(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1")
            .WithOpenApi()
            .RequireAuthorization();

        // Health endpoint (no auth required)
        app.MapGet("/health", () => new { status = "healthy", timestamp = DateTime.UtcNow })
            .WithName("Health")
            .WithOpenApi()
            .AllowAnonymous();

        // API Gateway endpoints
        group.MapGet("/gateway/services", GetAvailableServices)
            .WithName("GetServices")
            .WithSummary("Get all registered services");

        group.MapGet("/gateway/routes", GetAvailableRoutes)
            .WithName("GetRoutes")
            .WithSummary("Get all available routes");
    }

    private static IResult GetAvailableServices(IServiceRegistry registry)
    {
        var services = new[]
        {
            new { name = "Identity", port = 5003, status = "running" },
            new { name = "Patient", port = 5004, status = "running" },
            new { name = "Audit", port = 5005, status = "running" },
            new { name = "Appointment", port = 5006, status = "running" },
            new { name = "Notification", port = 5007, status = "running" },
            new { name = "Analytics", port = 5008, status = "running" },
            new { name = "Clinical", port = 5001, status = "running" },
            new { name = "Billing", port = 5002, status = "running" }
        };

        return Results.Ok(services);
    }

    private static IResult GetAvailableRoutes(ILogger<GatewayRoutesExtensions> logger)
    {
        var routes = new
        {
            authentication = new
            {
                login = "POST /api/v1/auth/login",
                register = "POST /api/v1/auth/register",
                refreshToken = "POST /api/v1/auth/refresh-token"
            },
            patients = new
            {
                create = "POST /api/v1/patients",
                get = "GET /api/v1/patients/{id}",
                update = "PUT /api/v1/patients/{id}",
                search = "GET /api/v1/patients/search?q={term}"
            },
            appointments = new
            {
                create = "POST /api/v1/appointments",
                get = "GET /api/v1/appointments/{id}",
                confirm = "POST /api/v1/appointments/{id}/confirm",
                cancel = "POST /api/v1/appointments/{id}/cancel"
            },
            notifications = new
            {
                send = "POST /api/v1/notifications",
                getUserNotifications = "GET /api/v1/notifications/user/{userId}",
                setPreferences = "POST /api/v1/notifications/preferences"
            },
            audit = new
            {
                getResourceTrail = "GET /api/v1/audit/resource/{resourceType}/{resourceId}",
                getUserActivity = "GET /api/v1/audit/user/{userId}"
            },
            analytics = new
            {
                getKpi = "GET /api/v1/analytics/kpi",
                getDashboards = "GET /api/v1/analytics/dashboards",
                createDashboard = "POST /api/v1/analytics/dashboards"
            }
        };

        return Results.Ok(routes);
    }
}
