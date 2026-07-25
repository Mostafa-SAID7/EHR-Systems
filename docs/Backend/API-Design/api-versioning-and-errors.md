# API Versioning, Error Handling & Pagination Best Practices

Guidelines for API lifecycle management, standard error responses, and performant pagination strategies.

---

## 1. API Versioning Strategies

```csharp
// 1. URL Path Versioning (Recommended for Public APIs)
[ApiController]
[Route("api/v{version:apiVersion}/patients")]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
public class PatientsController : ControllerBase
{
    [HttpGet]
    [MapToApiVersion("1.0")]
    public IActionResult GetV1() => Ok("V1 Data");

    [HttpGet]
    [MapToApiVersion("2.0")]
    public IActionResult GetV2() => Ok("V2 Data with enhanced fields");
}
```

---

## 2. Standardized Error Handling (RFC 7807 Problem Details)

```csharp
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unhandled error occurred",
                Detail = ex.Message,
                Instance = context.Request.Path
            };

            context.Response.StatusCode = problemDetails.Status.Value;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}
```

---

## 3. Cursor-Based Pagination vs Offset Pagination

```csharp
// Cursor-based pagination for high-volume database queries (avoids SQL OFFSET performance penalty)
public async Task<PagedResult<Patient>> GetPatientsPagedAsync(int? lastSeenId, int pageSize = 20)
{
    var query = _context.Patients.AsNoTracking().OrderBy(p => p.Id);

    if (lastSeenId.HasValue)
    {
        query = query.Where(p => p.Id > lastSeenId.Value);
    }

    var items = await query.Take(pageSize).ToListAsync();
    return new PagedResult<Patient>(items, items.LastOrDefault()?.Id);
}
```
