# API Gateway Metrics Guide

**Status**: ✅ **ALL 7 KEY INDICATORS ENABLED**  
**Date**: July 26, 2026

---

## 📊 API Gateway Metrics Overview

The API Gateway now exposes comprehensive metrics for monitoring reverse proxy performance and security.

### All 7 Key Metrics

| # | Metric | Type | Description | Labels | Unit |
|---|--------|------|-------------|--------|------|
| 1 | `gateway_requests_total` | Counter | Requests/sec processed | route, service | requests |
| 2 | `gateway_latency_seconds` | Histogram | Gateway round-trip latency | route | seconds |
| 3 | `route_latency_seconds` | Histogram | Per-route request latency | route | seconds |
| 4 | `gateway_auth_failures_total` | Counter | Authentication failures (401) | route | failures |
| 5 | `gateway_authz_failures_total` | Counter | Authorization failures (403) | route | failures |
| 6 | `gateway_http_5xx_total` | Counter | Server errors (5xx) | route, status | errors |
| 7 | `gateway_http_4xx_total` | Counter | Client errors (4xx) | route, status | errors |

**Result**: All 7 indicators collected ✅

---

## 🔧 Implementation

### File Created

**File**: `backend/src/EHRPlatform.Common/Extensions/ApiGatewayMetricsExtensions.cs`

```csharp
public static class ApiGatewayMetricsExtensions
{
    // Register metrics
    public static IServiceCollection AddApiGatewayMetrics(
        this IServiceCollection services)

    // Collect metrics in middleware
    public static WebApplication UseApiGatewayMetrics(
        this WebApplication app)
}
```

### API Gateway Configuration

**File**: `backend/src/EHRPlatform.Services.ApiGateway/Program.cs`

```csharp
// 1. Register metrics
builder.Services.AddOpenTelemetryMetrics("api-gateway");
builder.Services.AddApiGatewayMetrics();  // ← Gateway-specific metrics

// 2. Add middleware (early in pipeline, after exception handler)
app.UseEHRGlobalExceptionHandler();
app.UseApiGatewayMetrics();  // ← Collect metrics
```

---

## 📈 Detailed Metrics

### 1. Requests/Second

**Metric**: `gateway_requests_total`  
**Type**: Counter  
**Labels**: route, service

```promql
rate(gateway_requests_total[5m])
```

**Shows**: Requests per second flowing through gateway by route

**Example**:
```
{route="patients", service="api-gateway"}    15 req/sec
{route="clinical", service="api-gateway"}    8 req/sec
{route="billing", service="api-gateway"}     3 req/sec
```

---

### 2. Gateway Latency (Round-Trip)

**Metric**: `gateway_latency_seconds`  
**Type**: Histogram (buckets: 5ms, 10ms, 25ms, 50ms, 75ms, 100ms, 250ms, 500ms, etc.)  
**Labels**: route

```promql
histogram_quantile(0.95, rate(gateway_latency_seconds_bucket[5m]))
```

**Shows**: P95 latency for entire gateway round-trip (from request arrival to response sent)

**Includes**:
- Request parsing
- Authentication verification
- Route matching
- Load balancing
- Service latency (proxied)
- Response marshaling

**Example**:
```
P50: 32ms
P95: 87ms
P99: 156ms
```

---

### 3. Route-Specific Latency

**Metric**: `route_latency_seconds`  
**Type**: Histogram  
**Labels**: route

```promql
histogram_quantile(0.95, 
  sum by (route, le) (
    rate(route_latency_seconds_bucket[5m])
  )
)
```

**Shows**: Per-route latency percentiles

**Use**: Identify slow routes

**Example**:
```
/api/v1/patients  → P95 = 45ms
/api/v1/clinical  → P95 = 120ms  ← Slow!
/api/v1/billing   → P95 = 78ms
```

---

### 4. Authentication Failures

**Metric**: `gateway_auth_failures_total`  
**Type**: Counter  
**Labels**: route

```promql
rate(gateway_auth_failures_total[5m])
```

**Shows**: Authentication failures per second (401 Unauthorized)

**Causes**:
- Missing/invalid JWT token
- Expired token
- Invalid signature
- Malformed Authorization header

**Example**:
```
{route="patients"}    2 failures/sec  ← Possible attack or misconfigured client
{route="billing"}     0 failures/sec
```

---

### 5. Authorization Failures

**Metric**: `gateway_authz_failures_total`  
**Type**: Counter  
**Labels**: route

```promql
rate(gateway_authz_failures_total[5m])
```

**Shows**: Authorization failures per second (403 Forbidden)

**Causes**:
- User authenticated but lacks required role/permission
- Route requires specific policy (e.g., "Bearer" policy)
- Insufficient privilege level

**Example**:
```
{route="billing"}     5 failures/sec  ← Users lacking billing role
{route="audit"}       1 failure/sec   ← Users lacking audit role
```

---

### 6. Server Errors (5xx)

**Metric**: `gateway_http_5xx_total`  
**Type**: Counter  
**Labels**: route, status

```promql
rate(gateway_http_5xx_total[5m])
```

**Shows**: 500/502/503/504 errors per second

**Causes**:
- Proxied service crashed/down (502 Bad Gateway)
- Proxied service error (500 Internal Server Error)
- Gateway itself error (500)
- Service timeout (504 Gateway Timeout)
- Service unavailable (503 Service Unavailable)

**Example**:
```
{route="clinical", status="502"}      3 errors/sec  ← Clinical service down
{route="patient", status="500"}       1 error/sec   ← Intermittent error
{route="notification", status="504"}  2 errors/sec  ← Timeout
```

**Alert**: Any 5xx rate > 1/sec indicates serious problem ⚠️

---

### 7. Client Errors (4xx)

**Metric**: `gateway_http_4xx_total`  
**Type**: Counter  
**Labels**: route, status  
**Note**: Excludes 401 and 403 (tracked separately)

```promql
rate(gateway_http_4xx_total[5m])
```

**Shows**: 400/404/409/422/etc. errors per second

**Common Status Codes**:
- 400 Bad Request — Invalid input/format
- 404 Not Found — Resource doesn't exist
- 409 Conflict — State conflict (e.g., duplicate)
- 422 Unprocessable Entity — Validation failed
- 429 Too Many Requests — Rate limit exceeded

**Example**:
```
{route="patients", status="404"}      12 errors/sec  ← Non-existent resources
{route="clinical", status="400"}      5 errors/sec   ← Bad input
{route="billing", status="429"}       20 errors/sec  ← Rate limit
```

---

## 🎯 PromQL Queries for API Gateway

### Query 1: Request Rate by Route

```promql
sum by (route) (rate(gateway_requests_total[1m]))
```

**Shows**: Requests per second per route

### Query 2: P95 Latency by Route

```promql
histogram_quantile(0.95,
  sum by (route, le) (
    rate(gateway_latency_seconds_bucket[5m])
  )
)
```

**Shows**: P95 round-trip latency per route

### Query 3: Authentication Failures by Route

```promql
sum by (route) (rate(gateway_auth_failures_total[5m]))
```

**Shows**: Auth failures per second per route

### Query 4: Authorization Failures by Route

```promql
sum by (route) (rate(gateway_authz_failures_total[5m]))
```

**Shows**: Authz failures per second per route

### Query 5: 5xx Error Rate

```promql
sum by (route, status) (rate(gateway_http_5xx_total[5m]))
```

**Shows**: 5xx errors per second by route and status code

### Query 6: 4xx Error Rate

```promql
sum by (route, status) (rate(gateway_http_4xx_total[5m]))
```

**Shows**: 4xx errors per second by route and status code

### Query 7: Total Error Rate

```promql
(
  sum(rate(gateway_http_4xx_total[5m])) +
  sum(rate(gateway_http_5xx_total[5m])) +
  sum(rate(gateway_auth_failures_total[5m])) +
  sum(rate(gateway_authz_failures_total[5m]))
) / sum(rate(gateway_requests_total[5m])) * 100
```

**Shows**: Total error rate as percentage of requests

### Query 8: Slow Routes Alert

```promql
histogram_quantile(0.95,
  rate(gateway_latency_seconds_bucket[5m])
) > 0.2
```

**Alert**: Fires if P95 latency > 200ms on any route

### Query 9: Auth Problem Alert

```promql
rate(gateway_auth_failures_total[5m]) > 5
```

**Alert**: Fires if > 5 auth failures/sec (possible attack or misconfiguration)

### Query 10: 5xx Alert

```promql
rate(gateway_http_5xx_total[5m]) > 1
```

**Alert**: Fires if > 1 server error/sec (service health issue)

---

## 🔍 Real Examples

### Example 1: Normal Traffic

```
Request Rate:     100 req/sec total
  /patients       40 req/sec
  /clinical       35 req/sec
  /billing        15 req/sec
  /audit          10 req/sec

Latency P95:      ~80ms
Auth Failures:    0/sec
Authz Failures:   0/sec
5xx Errors:       0/sec
4xx Errors:       2/sec (normal, mostly 404s)
```

**Status**: ✅ Healthy

---

### Example 2: Authentication Attack

```
Request Rate:     150 req/sec total
Auth Failures:    45/sec  ← Spike!
Authz Failures:   2/sec
5xx Errors:       0/sec
4xx Errors:       8/sec

Pattern: Repeated requests with invalid tokens
```

**Alert**: Authentication failure rate spiked > 5/sec  
**Action**: Block source IP via WAF or rate limiter

---

### Example 3: Downstream Service Degradation

```
Request Rate:     100 req/sec (normal)
  /clinical       35 req/sec

Latency P95:      450ms  ← Increased from 80ms!
5xx Errors:       3/sec  ← 502 Bad Gateway

Pattern: /clinical requests slow and failing
```

**Alert**: P95 latency > 200ms on /clinical route  
**Action**: Check Clinical Service health

---

### Example 4: Route Authorization Misconfiguration

```
Request Rate:     20 req/sec
  /audit          20 req/sec

Authz Failures:   18/sec  ← 90% failure rate!
Success Rate:     2/sec

Pattern: Users making requests to /audit but lacking role
```

**Alert**: Authz failure rate > 5/sec on /audit  
**Action**: Review /audit route policy or user roles

---

## 📊 Grafana Dashboard Panels

### Panel 1: Request Rate (Overall)

```promql
sum(rate(gateway_requests_total[1m]))
```

**Type**: Gauge or Graph  
**Y-axis**: Requests/sec

### Panel 2: Latency (P50/P95/P99)

```promql
histogram_quantile(0.50, rate(gateway_latency_seconds_bucket[5m]))
histogram_quantile(0.95, rate(gateway_latency_seconds_bucket[5m]))
histogram_quantile(0.99, rate(gateway_latency_seconds_bucket[5m]))
```

**Type**: Graph (3 lines)  
**Shows**: Latency trend by percentile

### Panel 3: Error Rate %

```promql
(sum(rate(gateway_http_4xx_total[5m])) + 
 sum(rate(gateway_http_5xx_total[5m])) +
 sum(rate(gateway_auth_failures_total[5m])) +
 sum(rate(gateway_authz_failures_total[5m]))) / 
sum(rate(gateway_requests_total[5m])) * 100
```

**Type**: Gauge  
**Thresholds**: Green < 1%, Yellow 1-5%, Red > 5%

### Panel 4: Error Breakdown

```promql
label_replace(
  sum by (status) (rate(gateway_http_5xx_total[5m])),
  "type", "5xx", "", ""
)
or
label_replace(
  sum by (status) (rate(gateway_http_4xx_total[5m])),
  "type", "4xx", "", ""
)
or
label_replace(
  sum(rate(gateway_auth_failures_total[5m])),
  "type", "401", "", ""
)
or
label_replace(
  sum(rate(gateway_authz_failures_total[5m])),
  "type", "403", "", ""
)
```

**Type**: Pie chart  
**Shows**: Error types as percentage

### Panel 5: Requests by Route

```promql
sum by (route) (rate(gateway_requests_total[1m]))
```

**Type**: Graph (multi-line)  
**Shows**: Traffic per route over time

### Panel 6: Latency by Route

```promql
histogram_quantile(0.95,
  sum by (route, le) (
    rate(gateway_latency_seconds_bucket[5m])
  )
)
```

**Type**: Graph (multi-line)  
**Shows**: P95 latency per route

---

## 🚨 Alert Rules

### Alert 1: High Error Rate

```yaml
alert: ApiGatewayHighErrorRate
expr: |
  (sum(rate(gateway_http_4xx_total[5m])) + 
   sum(rate(gateway_http_5xx_total[5m])) +
   sum(rate(gateway_auth_failures_total[5m])) +
   sum(rate(gateway_authz_failures_total[5m]))) / 
  sum(rate(gateway_requests_total[5m])) > 0.05
for: 5m
annotations:
  summary: "API Gateway error rate > 5%"
  description: "{{ $value | humanizePercentage }} of requests failing"
```

### Alert 2: High Latency

```yaml
alert: ApiGatewayHighLatency
expr: |
  histogram_quantile(0.95, rate(gateway_latency_seconds_bucket[5m])) > 0.2
for: 10m
annotations:
  summary: "API Gateway P95 latency > 200ms"
  description: "P95 latency: {{ $value | humanizeDuration }}"
```

### Alert 3: Authentication Attack

```yaml
alert: ApiGatewayAuthAttack
expr: sum(rate(gateway_auth_failures_total[5m])) > 5
for: 2m
annotations:
  summary: "API Gateway auth failures > 5/sec"
  description: "{{ $value | humanize }} auth failures/sec - possible attack"
```

### Alert 4: Server Errors

```yaml
alert: ApiGateway5xxErrors
expr: sum(rate(gateway_http_5xx_total[5m])) > 1
for: 5m
annotations:
  summary: "API Gateway 5xx errors > 1/sec"
  description: "{{ $value | humanize }} server errors/sec"
```

### Alert 5: Route-Specific Latency

```yaml
alert: ApiGatewayRouteLatency
expr: |
  histogram_quantile(0.95, rate(route_latency_seconds_bucket{route!~"health|swagger|metrics"}[5m])) > 0.3
for: 10m
annotations:
  summary: "API Gateway route {{ $labels.route }} P95 latency > 300ms"
  description: "Route {{ $labels.route }}: {{ $value | humanizeDuration }}"
```

---

## ✅ Verification

- [x] All 7 metrics implemented
- [x] Gateway latency collected
- [x] Route latency per-route
- [x] Auth failures tracked
- [x] Authz failures tracked
- [x] 4xx errors tracked
- [x] 5xx errors tracked
- [x] Prometheus export enabled
- [x] PromQL queries available
- [x] Grafana panels possible
- [x] Alert rules can be created
- [x] Zero duplicates
- [x] Production ready

---

## 📝 Summary

✅ API Gateway now exposes all 7 key metrics  
✅ Gateway latency measured end-to-end  
✅ Route-specific latency tracked  
✅ Authentication/authorization failures monitored  
✅ Error rates broken down by type  
✅ Metrics exported to Prometheus  
✅ Grafana dashboards possible  
✅ Alerts can detect problems early  

**Status: PRODUCTION READY** ✅
