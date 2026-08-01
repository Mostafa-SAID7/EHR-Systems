# EHR-System API Gateway - Complete Architecture & Design

**Status**: DESIGN PHASE  
**Version**: 1.0 - Complete Specification  
**Date**: August 1, 2026  

---

## Table of Contents

1. [Executive Overview](#executive-overview)
2. [Gateway Responsibilities](#gateway-responsibilities)
3. [Architecture Patterns](#architecture-patterns)
4. [Service Routing Configuration](#service-routing-configuration)
5. [Request/Response Processing](#requestresponse-processing)
6. [Security & Authentication](#security--authentication)
7. [Rate Limiting & Throttling](#rate-limiting--throttling)
8. [Request Transformation](#request-transformation)
9. [Response Aggregation](#response-aggregation)
10. [Error Handling](#error-handling)
11. [Monitoring & Observability](#monitoring--observability)
12. [Implementation Guide](#implementation-guide)

---

## Executive Overview

### What is the API Gateway?

The API Gateway is the **single entry point** for all client requests to the EHR-System microservices. It acts as a façade, providing:

- **Unified API contract** - Clients don't know about internal service topology
- **Request routing** - Directs requests to correct microservice based on path
- **Request/response transformation** - Adapts between external API and internal service contracts
- **Authentication delegation** - Validates JWT and enriches requests with user info
- **Rate limiting** - Protects backend services from overload
- **Response aggregation** - Combines data from multiple services
- **API versioning** - Supports multiple API versions
- **Monitoring & logging** - Central point for observability

### Design Principles

1. **Single Responsibility** - Gateway handles only cross-cutting concerns
2. **No Business Logic** - All business logic stays in microservices
3. **Stateless** - Enables horizontal scaling
4. **High Performance** - Minimal latency overhead
5. **Backward Compatibility** - Support multiple API versions
6. **Loose Coupling** - Changes to services don't break gateway

---

## Gateway Responsibilities

### ✅ Gateway DOES Handle

| Responsibility | Example |
|---|---|
| **Routing** | Route `/patients/*` → Patient Service |
| **JWT Validation** | Verify token signature, expiry, claims |
| **Rate Limiting** | Max 100 req/min per user |
| **Request Logging** | Log all requests with correlation IDs |
| **Response Aggregation** | Combine Patient + Appointments for dashboard |
| **API Versioning** | `/api/v1/patients` vs `/api/v2/patients` |
| **Request Transformation** | Convert external DTO to internal DTO |
| **Response Transformation** | Convert internal response to external format |
| **Health Checks** | Verify all services are up |
| **Monitoring** | Prometheus metrics for gateway performance |

### ❌ Gateway Does NOT Handle

| What | Why | Where It Goes |
|---|---|---|
| **Business Logic** | Services own their domain | Patient/Appointment/etc services |
| **Data Validation** | Services validate their domain | Each service's validators |
| **Authorization** | Services check RBAC permissions | Identity Service |
| **Caching** | Services cache their data | Each service's cache layer |
| **Database Queries** | Services own their data | Service databases |

---

## Architecture Patterns

### Pattern 1: Routing Pattern (Most Common)

```
Client Request
     │
     ▼
┌─────────────────────┐
│   API Gateway       │
│ ┌─────────────────┐ │
│ │ 1. Parse URL    │ │
│ │ 2. Route lookup │ │
│ │ 3. Validate JWT │ │
│ │ 4. Rate limit   │ │
│ └─────────────────┘ │
└──────────┬──────────┘
           │
    ┌──────┴────────┐
    │               │
┌───▼──┐       ┌───▼──────┐
│Route │ ...   │Route N   │
│1     │       │          │
└───┬──┘       └───┬──────┘
    │              │
┌───▼──────────────▼──┐
│ Microservice Pool   │
│ ┌──────────────┐    │
│ │ Patient Svc  │    │
│ │ 5004         │    │
│ └──────────────┘    │
└─────────────────────┘
```

### Pattern 2: Aggregation Pattern (Composite Response)

```
GET /api/v1/dashboard/patient/{id}
          │
          ▼
┌──────────────────────────┐
│   API Gateway            │
│   Orchestrator           │
└──────────────────────────┘
          │
    ┌─────┼─────┬─────┐
    │     │     │     │
┌───▼──┐ ┌─▼───┐ ┌───▼───┐ ┌─────▼──┐
│Patient│ │Apps │ │Billing│ │Clinical│
│Svc    │ │Svc  │ │Svc    │ │Svc     │
│GET{id}│ │GET{id}
│GET{id}│ │GET{id}
└───┬──┘ └─┬───┘ └───┬───┘ └─────┬──┘
    │     │     │     │
    └─────┼─────┼─────┘
          │
    ┌─────▼─────────┐
    │ Aggregate     │
    │ Responses     │
    │ Merge/Join    │
    └─────┬─────────┘
          │
          ▼
    Return Combined
    Dashboard DTO
```

### Pattern 3: Protocol Translation

```
HTTP/REST Client
     │ JSON
     ▼
┌─────────────────┐
│ API Gateway     │ ◄── Translates between protocols
│                 │
└─────────────────┘
     │ gRPC
     ▼
Internal gRPC Service
(for high-performance internal calls)
```

---

## Service Routing Configuration

### Routing Registry (Fluent Configuration)

```csharp
public class RouteConfiguration
{
    public static void ConfigureRoutes(IEndpointRouteBuilder app)
    {
        // Identity Service Routes
        app.MapIdentityRoutes();
        
        // Patient Service Routes
        app.MapPatientRoutes();
        
        // Appointment Service Routes
        app.MapAppointmentRoutes();
        
        // ... etc for all 10 services
    }
}

// Extension Method Pattern
public static class IdentityRouteExtensions
{
    public static void MapIdentityRoutes(this IEndpointRouteBuilder app)
    {
        app.MapGroup("/api/v1/auth")
            .WithName("Identity")
            .WithOpenApi()
            .MapIdentityEndpoints();
    }
}
```

### Route Definitions (Complete Matrix)

```
SERVICE ROUTING MATRIX
═══════════════════════════════════════════════════════════════

IDENTITY SERVICE (Port 5003)
  POST   /api/v1/auth/login                    → Identity Service
  POST   /api/v1/auth/register                 → Identity Service
  POST   /api/v1/auth/refresh-token            → Identity Service
  GET    /api/v1/auth/me                       → Identity Service
  POST   /api/v1/auth/mfa/setup                → Identity Service
  GET    /api/v1/users/{id}                    → Identity Service (admin)
  GET    /api/v1/users                         → Identity Service (admin)

PATIENT SERVICE (Port 5004)
  POST   /api/v1/patients                      → Patient Service
  GET    /api/v1/patients/{id}                 → Patient Service
  PUT    /api/v1/patients/{id}                 → Patient Service
  GET    /api/v1/patients/search?q={term}      → Patient Service
  POST   /api/v1/patients/{id}/allergies       → Patient Service
  GET    /api/v1/patients/{id}/conditions      → Patient Service

APPOINTMENT SERVICE (Port 5006)
  POST   /api/v1/appointments                  → Appointment Service
  GET    /api/v1/appointments/{id}             → Appointment Service
  POST   /api/v1/appointments/{id}/confirm     → Appointment Service
  GET    /api/v1/appointments/patient/{id}     → Appointment Service

NOTIFICATION SERVICE (Port 5007)
  POST   /api/v1/notifications                 → Notification Service
  GET    /api/v1/notifications/user/{userId}   → Notification Service
  POST   /api/v1/notifications/preferences     → Notification Service

AUDIT SERVICE (Port 5005)
  GET    /api/v1/audit/resource/{id}           → Audit Service
  GET    /api/v1/audit/user/{userId}           → Audit Service

ANALYTICS SERVICE (Port 5008)
  GET    /api/v1/analytics/kpi                 → Analytics Service
  GET    /api/v1/analytics/dashboards/{id}     → Analytics Service
  POST   /api/v1/analytics/dashboards          → Analytics Service

[... etc for remaining services ...]
```

---

## Request/Response Processing

### Request Pipeline (Order Matters)

```
┌─────────────────────────────────────┐
│ 1. INCOMING REQUEST                 │
│    POST /api/v1/patients            │
│    Authorization: Bearer {token}    │
│    X-Correlation-ID: uuid           │
└──────────────┬──────────────────────┘
               │
               ▼
┌─────────────────────────────────────┐
│ 2. PARSE & VALIDATE ROUTING         │
│    - Extract service path           │
│    - Match against route registry   │
│    - Fail if no route found         │
└──────────────┬──────────────────────┘
               │
               ▼
┌─────────────────────────────────────┐
│ 3. AUTHENTICATE                     │
│    - Extract JWT from header        │
│    - Validate signature             │
│    - Check expiry                   │
│    - Extract claims (user, roles)   │
└──────────────┬──────────────────────┘
               │
               ▼
┌─────────────────────────────────────┐
│ 4. AUTHORIZE (Optional)             │
│    - Check user roles               │
│    - Check resource permissions     │
│    - Return 403 if forbidden        │
└──────────────┬──────────────────────┘
               │
               ▼
┌─────────────────────────────────────┐
│ 5. RATE LIMITING                    │
│    - Get rate limit bucket for user │
│    - Check quota remaining          │
│    - Return 429 if exceeded         │
└──────────────┬──────────────────────┘
               │
               ▼
┌─────────────────────────────────────┐
│ 6. ENRICH REQUEST                   │
│    - Add User ID to headers         │
│    - Add Correlation ID             │
│    - Add Trace ID                   │
│    - Add User Roles                 │
└──────────────┬──────────────────────┘
               │
               ▼
┌─────────────────────────────────────┐
│ 7. TRANSFORM REQUEST                │
│    - Convert external DTO format    │
│    - Map to internal service format │
│    - Validate schema                │
└──────────────┬──────────────────────┘
               │
               ▼
┌─────────────────────────────────────┐
│ 8. FORWARD TO SERVICE               │
│    - HTTP call to service port      │
│    - Include enriched headers       │
│    - Timeout: 30s                   │
└──────────────┬──────────────────────┘
               │
               ▼
┌─────────────────────────────────────┐
│ 9. RECEIVE RESPONSE                 │
│    - Status code                    │
│    - Headers                        │
│    - Body (JSON)                    │
└──────────────┬──────────────────────┘
               │
               ▼
┌─────────────────────────────────────┐
│ 10. HANDLE ERRORS (If applicable)   │
│     - 5xx → Retry? Circuit breaker  │
│     - 4xx → Pass through to client  │
│     - Timeout → 504 Gateway Timeout │
└──────────────┬──────────────────────┘
               │
               ▼
┌─────────────────────────────────────┐
│ 11. TRANSFORM RESPONSE              │
│     - Map internal format to        │
│       external/client format        │
│     - Wrap in ApiResponse           │
└──────────────┬──────────────────────┘
               │
               ▼
┌─────────────────────────────────────┐
│ 12. LOG & METRICS                   │
│     - Log response time             │
│     - Record success/error          │
│     - Update Prometheus metrics     │
└──────────────┬──────────────────────┘
               │
               ▼
┌─────────────────────────────────────┐
│ RETURN TO CLIENT                    │
│ 200 OK                              │
│ Content-Type: application/json      │
│ {result, timestamp, traceId}        │
└─────────────────────────────────────┘
```

---

## Security & Authentication

### JWT Validation Flow

```csharp
// Pseudo-code showing validation flow
public class JwtValidationMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"]
            .ToString()
            .Replace("Bearer ", "");

        try
        {
            // 1. Parse token
            var principal = _jwtService.ValidateToken(token);
            
            // 2. Check expiry
            var exp = principal.FindFirst("exp");
            if (DateTime.UtcNow > UnixTimeStampToDateTime(exp.Value))
            {
                context.Response.StatusCode = 401;
                return;
            }
            
            // 3. Check signature (public key from Identity Service)
            var publicKey = _publicKeyProvider.GetPublicKey(principal.FindFirst("iss").Value);
            var isValid = _jwtService.VerifySignature(token, publicKey);
            
            if (!isValid)
            {
                context.Response.StatusCode = 401;
                return;
            }
            
            // 4. Attach user info to context
            context.Items["UserId"] = principal.FindFirst("sub").Value;
            context.Items["Email"] = principal.FindFirst("email").Value;
            context.Items["Roles"] = principal.FindAll("role");
            
            await _next(context);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid token" });
        }
    }
}
```

### Authorization Strategy

```csharp
// Role-based Authorization
public class AuthorizationMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        var requiredRoles = endpoint?.Metadata
            .GetMetadata<AuthorizeAttribute>()
            ?.Roles;
        
        if (requiredRoles != null)
        {
            var userRoles = (List<Claim>)context.Items["Roles"];
            
            if (!userRoles.Any(r => requiredRoles.Contains(r.Value)))
            {
                context.Response.StatusCode = 403;
                return;
            }
        }
        
        await _next(context);
    }
}
```

---

## Rate Limiting & Throttling

### Strategy 1: Token Bucket Algorithm

```
User: user@example.com
┌─────────────────────────┐
│ Bucket Capacity: 100    │
│ Refill Rate: 10/minute  │
│ Current Tokens: 45      │
└─────────────────────────┘

Request arrives:
  - Check: 45 >= 1? YES
  - Deduct: 45 - 1 = 44 tokens
  - Allow request ✓

Request arrives:
  - Every second, bucket gains (10/60) tokens
```

### Implementation

```csharp
public class RateLimitingMiddleware
{
    private readonly IDistributedCache _cache;
    
    public async Task InvokeAsync(HttpContext context)
    {
        var userId = context.Items["UserId"]?.ToString() ?? "anonymous";
        var cacheKey = $"ratelimit:{userId}";
        
        // Get current bucket
        var tokenCount = await _cache.GetAsync<int>(cacheKey) ?? 100;
        
        // Check limit
        if (tokenCount <= 0)
        {
            context.Response.StatusCode = 429; // Too Many Requests
            context.Response.Headers.Add("Retry-After", "60");
            return;
        }
        
        // Deduct token
        tokenCount--;
        await _cache.SetAsync(cacheKey, tokenCount, TimeSpan.FromMinutes(1));
        
        // Add headers for client
        context.Response.Headers.Add("X-RateLimit-Limit", "100");
        context.Response.Headers.Add("X-RateLimit-Remaining", tokenCount.ToString());
        
        await _next(context);
    }
}
```

### Rate Limits Per Tier

| User Type | Requests/Min | Burst |
|---|---|---|
| Anonymous | 10 | 2 |
| Free Tier | 100 | 10 |
| Paid Tier | 1000 | 100 |
| Admin | Unlimited | - |

---

## Request Transformation

### Scenario: External API v1 → Internal Service Format

```csharp
// EXTERNAL FORMAT (Client sends)
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john@example.com",
  "dob": "1990-01-15"
}

// INTERNAL FORMAT (Service expects)
{
  "first_name": "John",
  "last_name": "Doe",
  "email_address": "john@example.com",
  "date_of_birth": "1990-01-15T00:00:00Z",
  "created_at": "2026-08-01T10:30:00Z"
}

// TRANSFORMATION CODE
public class PatientDtoTransformer
{
    public static CreatePatientRequest Transform(PatientCreateDto external)
    {
        return new CreatePatientRequest
        {
            first_name = external.FirstName,
            last_name = external.LastName,
            email_address = external.Email,
            date_of_birth = DateTime.Parse(external.Dob),
            created_at = DateTime.UtcNow
        };
    }
}
```

---

## Response Aggregation

### Multi-Service Aggregation Example

Request: `GET /api/v1/dashboard/patient/123`

```csharp
public async Task<PatientDashboardResponse> GetPatientDashboard(string patientId)
{
    // Call multiple services in parallel
    var patientTask = _httpClient.GetAsync($"http://patient-service/api/v1/patients/{patientId}");
    var appointmentsTask = _httpClient.GetAsync($"http://appointment-service/api/v1/appointments/patient/{patientId}");
    var billingTask = _httpClient.GetAsync($"http://billing-service/api/v1/invoices/patient/{patientId}");
    var clinicalTask = _httpClient.GetAsync($"http://clinical-service/api/v1/notes/patient/{patientId}");
    
    await Task.WhenAll(patientTask, appointmentsTask, billingTask, clinicalTask);
    
    var patient = await patientTask.Result.Content.ReadAsAsync<PatientDto>();
    var appointments = await appointmentsTask.Result.Content.ReadAsAsync<List<AppointmentDto>>();
    var billing = await billingTask.Result.Content.ReadAsAsync<BillingDto>();
    var clinical = await clinicalTask.Result.Content.ReadAsAsync<List<ClinicalNoteDto>>();
    
    // Aggregate
    return new PatientDashboardResponse
    {
        Patient = patient,
        UpcomingAppointments = appointments.Where(a => a.Date > DateTime.UtcNow).Take(5),
        OutstandingBalance = billing.BalanceDue,
        RecentClinicalNotes = clinical.OrderByDescending(c => c.CreatedAt).Take(3)
    };
}
```

---

## Error Handling

### Unified Error Response Format

```csharp
public class ApiErrorResponse
{
    public string TraceId { get; set; }
    public int StatusCode { get; set; }
    public string Message { get; set; }
    public Dictionary<string, string[]> Errors { get; set; }
    public string Details { get; set; }
}

// Examples

// 400 Bad Request
{
  "traceId": "0HMVQ2R4GQN51:00000001",
  "statusCode": 400,
  "message": "Validation failed",
  "errors": {
    "email": ["Email is required", "Email format is invalid"],
    "dob": ["Date of birth must be in past"]
  }
}

// 401 Unauthorized
{
  "traceId": "0HMVQ2R4GQN51:00000002",
  "statusCode": 401,
  "message": "Authentication required",
  "details": "Missing or invalid JWT token"
}

// 500 Internal Server Error
{
  "traceId": "0HMVQ2R4GQN51:00000003",
  "statusCode": 500,
  "message": "An unexpected error occurred",
  "details": "Please contact support with trace ID"
}
```

### Error Handling Middleware

```csharp
public class GlobalExceptionMiddleware
{
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
    
    private static Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var response = new ApiErrorResponse
        {
            TraceId = context.TraceIdentifier,
            Details = ex.Message
        };
        
        var statusCode = ex switch
        {
            UnauthorizedAccessException => 401,
            KeyNotFoundException => 404,
            ArgumentException => 400,
            _ => 500
        };
        
        context.Response.StatusCode = statusCode;
        response.StatusCode = statusCode;
        
        return context.Response.WriteAsJsonAsync(response);
    }
}
```

---

## Monitoring & Observability

### Metrics Collection

```csharp
public class GatewayMetricsCollector
{
    private readonly IHistogram _requestDuration;
    private readonly ICounter _requestTotal;
    private readonly IUpDownCounter _activeRequests;
    
    public async Task<T> MeasureAsync<T>(
        string serviceName,
        string endpoint,
        Func<Task<T>> operation)
    {
        using var activity = _activitySource.StartActivity($"gateway.{serviceName}");
        _activeRequests.Add(1);
        
        var startTime = DateTime.UtcNow;
        
        try
        {
            var result = await operation();
            _requestTotal.Add(1, new KeyValuePair<string, object?>("status", "success"));
            return result;
        }
        catch (Exception ex)
        {
            _requestTotal.Add(1, new KeyValuePair<string, object?>("status", "error"));
            throw;
        }
        finally
        {
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _requestDuration.Record(duration);
            _activeRequests.Add(-1);
        }
    }
}

// Sample Metrics
gateway_request_duration_ms{service="patient",endpoint="create",status="success"} 45
gateway_request_total{service="patient",endpoint="get",status="success"} 1250
gateway_request_total{service="patient",endpoint="get",status="error"} 3
gateway_active_requests{service="appointment"} 12
```

### Distributed Tracing

```csharp
// Every request gets a correlation ID
public class CorrelationIdMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers
            .TryGetValue("X-Correlation-ID", out var value)
            ? value.ToString()
            : Guid.NewGuid().ToString();
        
        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers.Add("X-Correlation-ID", correlationId);
        
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}

// Trace example
[2026-08-01T10:30:45.123Z] [CorrelationId: 550e8400-e29b-41d4-a716-446655440000]
POST /api/v1/patients
→ (10ms) Authenticate
→ (5ms) Authorize
→ (2ms) Rate limit
→ (150ms) Forward to Patient Service
→ (3ms) Transform response
= 170ms total
```

---

## Implementation Guide

### Phase 1: Core Gateway (YARP)

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add YARP
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(/* config */);

builder.Services.AddAuthorization();

// Middleware
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiting();
app.UseCorrelationId();
app.UseGlobalExceptionHandler();

// Routes
app.MapReverseProxy();
app.MapHealthChecks("/health");
```

### Phase 2: Advanced Features

- Response aggregation
- Request transformation
- Circuit breakers
- Caching
- Service discovery

### Phase 3: Production Hardening

- Performance optimization
- Load testing
- Security audit
- Monitoring dashboards
- Disaster recovery

---

## Deployment Architecture

```
┌──────────────────────────────────────────┐
│   AWS/Azure/K8s Load Balancer           │
│   (Distribute traffic)                   │
└──────────────────────────────────────────┘
                    │
        ┌───────────┼───────────┐
        │           │           │
    ┌───▼──┐    ┌──▼───┐   ┌──▼───┐
    │ GW 1 │    │ GW 2 │   │ GW 3 │
    │ 5000 │    │ 5000 │   │ 5000 │
    └───┬──┘    └──┬───┘   └──┬───┘
        │           │           │
        └───────────┼───────────┘
                    │
        ┌───────────┼───────────┐
        │           │           │
    ┌───▼──┐    ┌──▼───┐   ┌──▼───┐
    │Auth  │    │Patient│  │Audit │
    │5003  │    │5004   │  │5005  │
    └──────┘    └───────┘  └──────┘
```

---

## Success Criteria

✅ **Functional Requirements**
- [ ] All 10 services accessible via gateway
- [ ] JWT authentication working
- [ ] Rate limiting enforced
- [ ] Response aggregation working
- [ ] Error handling centralized
- [ ] Health checks passing

✅ **Non-Functional Requirements**
- [ ] P99 latency < 200ms
- [ ] 99.9% uptime
- [ ] Horizontal scaling (3+ instances)
- [ ] Full request tracing
- [ ] Prometheus metrics
- [ ] HIPAA audit logging

---

## Next Steps

1. Create YARP configuration
2. Implement middleware pipeline
3. Set up route registry
4. Configure service discovery
5. Add rate limiting
6. Implement response aggregation
7. Deploy to Kubernetes
8. Performance testing
9. Production monitoring

