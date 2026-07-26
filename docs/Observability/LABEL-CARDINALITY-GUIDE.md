# Label Cardinality Guide — EHR Platform Observability

## Overview

High-cardinality labels in Prometheus metrics cause:
- **Storage explosion** — 10x or 100x more data per label
- **Query timeouts** — Prometheus struggles with millions of time series
- **Cost increases** — Higher memory, CPU, and disk requirements
- **Performance degradation** — Grafana dashboards become slow

This guide documents label standards for the EHR Platform.

---

## Label Classification

### ✅ LOW-CARDINALITY (Good)

**Definition:** 10-100 unique values across the entire system lifetime.

**Examples:**
```
service           → api-gateway, identity, patient, clinical, appointment, notification, audit, billing, prescription, analytics (10 values)
environment       → development, staging, production (3 values)
endpoint          → patients, appointments, clinical, billing, audit (20-30 values)
method            → GET, POST, PUT, DELETE, PATCH (5 values)
http_status       → 2xx, 3xx, 4xx, 5xx (4 classes)
reason            → invalid_credentials, account_locked, invalid_code (10-15 values)
role              → admin, doctor, patient, nurse, specialist (10 values)
database          → ehr_identity, ehr_patient, ehr_clinical, ehr_billing (5-10 values)
queue             → patient-created, appointment-scheduled, lab-result-ready (20-30 values)
status_class      → 5xx, 4xx, 2xx (3 values)
deployment_env    → dev, staging, prod (3 values)
version           → 1.0.0, 1.1.0, 1.2.0 (< 100 values)
```

### ❌ HIGH-CARDINALITY (Bad)

**Definition:** Thousands or millions of unique values (or unbounded).

**NEVER use these as labels:**
```
patientId              → ~100k+ patients
doctorId               → ~10k doctors
userId                 → ~100k+ users
email                  → ~100k+ unique emails
phone                  → ~100k+ phone numbers
username               → ~100k+ usernames
sessionId              → ~1M+ sessions (explodes over time)
jwtToken               → ~1M+ unique tokens
appointmentId          → ~1M+ appointments
prescriptionId         → ~1M+ prescriptions
traceId                → unique per request (unbounded)
requestId              → unique per request (unbounded)
full_endpoint_path     → /api/patients/123456/records/789 (unbounded)
database_query         → SELECT * FROM... (unbounded)
error_message          → "User not found", "Invalid token" (unbounded)
ip_address             → 192.168.1.1, 10.0.0.5 (thousands)
user_agent             → Mozilla/5.0..., Chrome/..., Safari/... (thousands)
hostname               → pod-abc123, pod-def456 (unbounded in auto-scaling)
```

---

## Best Practices

### 1. Use Attributes for High-Cardinality Data

**Problem:** Adding `user_id` to a counter causes explosion.

```csharp
// ❌ BAD: High-cardinality label
counter.Add(1, new("user_id", userId));  // Creates 100k+ time series
```

**Solution:** Store user_id in span attributes (for tracing) or logs, not metrics.

```csharp
// ✅ GOOD: Low-cardinality metric with trace correlation
counter.Add(1, new("method", "password"));  // Only 5-10 unique values

// Optional: Store user_id in Activity (trace context) for debugging
Activity.Current?.SetTag("user.id", userId);  // Doesn't affect cardinality
```

### 2. Pre-Aggregate High-Cardinality Data

**Problem:** You want to track patient login failures.

```csharp
// ❌ BAD: Would create 100k+ time series
foreach (var failure in loginFailures)
{
    counter.Add(1, new("patient_id", failure.PatientId));
}
```

**Solution:** Count failures, don't label them.

```csharp
// ✅ GOOD: Aggregate first, then label low-cardinality data
var failureCount = loginFailures.GroupBy(f => f.Reason)
    .Select(g => (g.Key, g.Count()));

foreach (var (reason, count) in failureCount)
{
    counter.Add(count, new("reason", reason));  // ~10 reasons
}
```

### 3. Use Label Values from Enums, Not User Data

**Problem:** Endpoint path contains user IDs.

```csharp
// ❌ BAD: Full path causes explosion
var fullPath = context.Request.Path.Value;  // /api/patients/12345/records
counter.Add(1, new("endpoint", fullPath));  // Unbounded cardinality
```

**Solution:** Extract only the resource type.

```csharp
// ✅ GOOD: Resource category (low-cardinality)
var resourceType = ExtractResourceType(context.Request.Path);  // "patients"
counter.Add(1, new("endpoint", resourceType));  // ~30 unique values
```

### 4. Avoid Dynamic Status Codes, Use Classes

**Problem:** Tracking every possible HTTP status code.

```csharp
// ❌ BAD: 100+ possible status codes
counter.Add(1, new("status", statusCode.ToString()));  // 200, 201, 400, 401, ..., 599
```

**Solution:** Group into classes.

```csharp
// ✅ GOOD: Class-based grouping
var statusClass = statusCode switch
{
    >= 500 => "5xx",
    >= 400 => "4xx",
    >= 300 => "3xx",
    >= 200 => "2xx",
    _ => "1xx"
};
counter.Add(1, new("status_class", statusClass));  // Only 5 values
```

### 5. Document Label Limitations

Always add comments when labels are intentionally bounded:

```csharp
/// <summary>
/// Tracks login failures.
/// Label: reason (low-cardinality, ~10-15 values: invalid_credentials, account_locked, etc.)
/// Do NOT add user_id or email as labels (high-cardinality).
/// For user-specific debugging, correlate via trace IDs.
/// </summary>
public static void RecordLoginFailure(string email, string reason)
{
    var counter = Meter.CreateCounter<long>("identity.login_failure");
    counter.Add(1, new("reason", reason));  // Only this label, NOT email
}
```

---

## EHR Platform Label Standards

### HTTP Metrics
```
service:         api-gateway, identity, patient, clinical, appointment, notification, audit, billing, prescription, analytics
method:          GET, POST, PUT, DELETE, PATCH
endpoint:        patients, appointments, clinical, records, billing (low-cardinality)
status_class:    2xx, 3xx, 4xx, 5xx
environment:     development, staging, production
```

### Authentication Metrics
```
method:          password, oauth2, mfa, refresh_token
reason:          invalid_credentials, account_locked, mfa_failed, invalid_code
endpoint:        login, refresh, authorize (low-cardinality)
role:            admin, doctor, patient, nurse, specialist
```

### RabbitMQ Metrics
```
queue:           patient-created, appointment-scheduled, lab-result-ready, audit-log, prescription-created
status:          acked, nacked, redelivered
consumer_tag:    (DO NOT USE - use queue name instead)
```

### Database Metrics
```
database:        ehr_identity, ehr_patient, ehr_clinical, ehr_billing, ehr_audit
operation:       select, insert, update, delete
status:          success, failed
duration_class:  fast (<10ms), normal (10-100ms), slow (>100ms)
```

### Business Metrics
```
resource_type:   patient, appointment, doctor, prescription, admission, claim
status:          pending, active, completed, cancelled, failed
specialization:  cardiology, neurology, pediatrics, orthopedics (20-30 values)
```

---

## Cardinality Audit Checklist

When adding a new metric, ask:

- [ ] Is this label derived from user data (ID, email, phone)? → Use span attributes instead
- [ ] Could this label value be > 1,000? → Aggregate or use a class
- [ ] Does the full label path become /api/resource/id/sub-id? → Extract just resource type
- [ ] Am I adding all possible enum values (e.g., HTTP status)? → Use classes instead
- [ ] Is this label documented with expected cardinality? → Add JSDoc comment
- [ ] Can I reproduce this metric cardinality growth in tests? → Test label explosion

---

## Monitoring Label Cardinality

### Check Prometheus Time Series Count

```bash
# SSH into Prometheus or port-forward
docker exec ehr-prometheus curl -s http://localhost:9090/api/v1/query | jq '.data.result | length'

# Or query directly
curl -s 'http://localhost:9090/api/v1/query?query=count(ALERTS)' | jq .
```

### Query High-Cardinality Metrics

```promql
# Find metrics with > 10k time series
topk(10, count by (__name__) (count by (__name__) ({__name__=~".+"}) > 10000))

# Find metrics growing over time
topk(10, count({__name__=~".*"}) - count({__name__=~".*"} offset 1h))

# List all labels for a metric
label_names()
```

### Grafana Alert for High Cardinality

Create an alert in Grafana if metrics exceed thresholds:

```yaml
- alert: HighCardinalityMetric
  expr: |
    topk(1, count by (__name__) ({__name__=~".*"})) > 50000
  for: 5m
  labels:
    severity: warning
  annotations:
    summary: "High cardinality detected: {{ $value }} time series"
```

---

## Implementation Examples

### ❌ Before (High-Cardinality)

```csharp
public static void RecordLoginSuccess(string userId, string email)
{
    var counter = Meter.CreateCounter<long>("identity.login_success");
    counter.Add(1, 
        new("user_id", userId),        // ❌ 100k+ unique
        new("email", email),           // ❌ 100k+ unique
        new("timestamp", DateTime.Now.ToShortTimeString()));  // ❌ Unbounded
}
```

**Result:** 100k × 100k × 1440 = 144 BILLION time series (unrealistic, but Prometheus will OOM)

### ✅ After (Low-Cardinality)

```csharp
public static void RecordLoginSuccess(string userId, string email, string authMethod = "password")
{
    var counter = Meter.CreateCounter<long>("identity.login_success");
    counter.Add(1, new("method", authMethod));  // ✅ 5-10 unique values
    
    // User-specific data stays in trace/logs, not metrics
    Activity.Current?.SetTag("user.id", userId);
    Activity.Current?.SetTag("email", email);
}
```

**Result:** 5-10 time series (realistic and queryable)

---

## References

- [Prometheus Best Practices: Cardinality](https://prometheus.io/docs/practices/naming/#labels)
- [Google SRE Book: Monitoring Distributed Systems](https://sre.google/sre-book/monitoring-distributed-systems/)
- [OpenTelemetry: Metrics Data Model](https://opentelemetry.io/docs/reference/specification/metrics/data-model/)
- [Cortex: High-Cardinality Labels](https://cortexmetrics.io/docs/faq/#what-would-cause-an-out-of-memory-error)

---

## Summary

**Rule of thumb:**
- ✅ LOW-CARDINALITY: Labels with 10-100 unique values (service, method, endpoint, role, reason)
- ❌ HIGH-CARDINALITY: Labels with 1k+ unique values (user_id, email, phone, session_id, trace_id, full_path)

**For debugging user-specific issues:** Use trace IDs, span attributes, and structured logs instead of metric labels.
