# API Security — Rate Limiting, Service-to-Service & Input Validation

## 1. Rate Limiting in ASP.NET Core 7+

```csharp
// Program.cs
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("ApiLimit", cfg =>
    {
        cfg.PermitLimit = 100;
        cfg.Window = TimeSpan.FromMinutes(1);
        cfg.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        cfg.QueueLimit = 10;
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

app.UseRateLimiter();

// Apply to controller
[EnableRateLimiting("ApiLimit")]
[ApiController]
[Route("api/v1/coding")]
public class CodingController : ControllerBase { ... }
```

---

## 2. Service-to-Service Authentication (mTLS / API Keys)

### Option A: Shared API Keys (simple, internal)
```csharp
// Middleware that validates X-Api-Key header for internal service calls
public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _validKey;

    public ApiKeyMiddleware(RequestDelegate next, IConfiguration config)
    { _next = next; _validKey = config["ServiceApiKey"]!; }

    public async Task InvokeAsync(HttpContext ctx)
    {
        if (!ctx.Request.Headers.TryGetValue("X-Api-Key", out var key) || key != _validKey)
        {
            ctx.Response.StatusCode = 401;
            return;
        }
        await _next(ctx);
    }
}
```

### Option B: JWT Client Credentials (OAuth2 M2M)
```csharp
// Service requesting a token
var tokenResponse = await _httpClient.RequestClientCredentialsTokenAsync(new()
{
    Address = "https://auth.tachyhealth.com/connect/token",
    ClientId = "coding-service",
    ClientSecret = "secret",
    Scope = "claim-service"
});

httpClient.SetBearerToken(tokenResponse.AccessToken);
```

---

## 3. Input Validation & SQL Injection Prevention

```csharp
// ❌ NEVER — raw SQL with user input
var sql = $"SELECT * FROM Patients WHERE Name = '{name}'"; // SQL injection!

// ✅ ALWAYS — parameterized via Dapper
var patient = await db.QueryFirstOrDefaultAsync<Patient>(
    "SELECT * FROM Patients WHERE Name = @Name", new { Name = name });

// ✅ ALWAYS — EF Core (auto-parameterized)
var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Name == name);
```

---

## 4. Secrets Management (No Hardcoded Credentials)

```csharp
// ❌ NEVER
var conn = "Server=prod-db;Password=Admin123;"; // Hardcoded!

// ✅ Azure Key Vault / Environment Variables
builder.Configuration.AddAzureKeyVault(
    new Uri("https://tachyhealth-vault.vault.azure.net/"),
    new DefaultAzureCredential());

var conn = builder.Configuration["Database:ConnectionString"];
```
