# OpenTelemetry Vendor-Neutral Architecture — EHR Platform

## Overview

The EHR Platform observability stack is **vendor-neutral** using OpenTelemetry (OTEL). This means:

1. **No Prometheus-specific code** in the application
2. **No Jaeger/Tempo-specific code** in the application
3. **No Loki-specific code** in the application
4. **All telemetry flows through OTEL Collector**
5. **Easy to swap backends without application changes**

---

## Architecture

```
┌────────────────────────────────────────┐
│    EHR Microservices (10 services)     │
│  - API Gateway, Identity, Patient      │
│  - Clinical, Appointment, Notification │
│  - Audit, Billing, Prescription        │
│  - Analytics                           │
└────────────┬─────────────────────────┘
             │
             │ OpenTelemetry SDK
             │ - Metrics (OTLP/gRPC)
             │ - Traces (OTLP/gRPC)
             │ - Logs (OTLP/gRPC)
             │
             ↓
┌────────────────────────────────────────┐
│  OpenTelemetry Collector               │
│  (otel-collector:4317)                 │
│  - Aggregation & sampling              │
│  - Batch processing                    │
│  - Filtering & redaction (PHI)         │
│  - Protocol conversion (OTLP → ...)    │
└─┬──────────┬─────────────┬──────────┬─┘
  │          │             │          │
  ↓          ↓             ↓          ↓
Prometheus  Tempo        Loki      Others
(metrics)  (traces)    (logs)     (optional)
  │          │             │
  └──────────┴─────────────┘
             │
             ↓
┌────────────────────────────────────────┐
│     Grafana (Dashboard)                │
│  - Prometheus datasource (metrics)     │
│  - Tempo datasource (traces)           │
│  - Loki datasource (logs)              │
│  - Unified dashboards                  │
└────────────────────────────────────────┘
```

---

## Why Vendor-Neutral?

### ✅ Benefits

**Flexibility:**
- Swap Prometheus for Datadog, New Relic, Elastic, etc.
- Swap Tempo for Jaeger, Dynatrace, Honeycomb, etc.
- Swap Loki for ELK Stack, Splunk, CloudWatch, etc.

**Future-Proof:**
- Adopt new observability tools without rewriting application code
- Test new backends by just updating OTEL Collector config
- Multi-backend deployments (e.g., Prometheus + Datadog simultaneously)

**Standards-Based:**
- OpenTelemetry is CNCF-backed (same org as Kubernetes, Prometheus)
- Wide vendor support (50+ vendors)
- Language-agnostic (Java, Go, Python, Node.js, .NET, etc.)

**Reduced Lock-in:**
- Not tied to Prometheus-specific APIs
- Not tied to Jaeger-specific SDKs
- Not tied to Loki-specific formats

---

## How It Works

### 1. Application Sends Telemetry (OTLP)

```csharp
// In Program.cs
builder.Services.AddOpenTelemetryObservability("EHRPlatform.Identity");
builder.Logging.AddOpenTelemetryLogging();
```

Application collects:
- **Metrics**: HTTP requests, RabbitMQ throughput, database queries, custom business metrics
- **Traces**: Request flow across services, external API calls, database operations
- **Logs**: Structured JSON with trace correlation

All sent via **OTLP (OpenTelemetry Protocol)** on gRPC to the Collector.

### 2. OTEL Collector Processes Telemetry

Located at `otel-collector:4317`, the Collector:

1. **Receives** telemetry from all 10 services
2. **Aggregates** metrics across instances
3. **Samples** traces (e.g., 1% sample rate for high-volume services)
4. **Redacts** PHI (patient IDs, emails, etc.) via attributes
5. **Batches** for efficiency
6. **Exports** to configured backends

### 3. Backends Store & Query Telemetry

**Prometheus** (metrics store):
```yaml
# otel-collector.yml
exporters:
  prometheusremotewrite:
    endpoint: "http://prometheus:9090/api/v1/write"
```

**Tempo** (traces store):
```yaml
exporters:
  otlp/tempo:
    endpoint: "tempo:4317"
    tls:
      insecure: true
```

**Loki** (logs store):
```yaml
exporters:
  loki:
    endpoint: "http://loki:3100/loki/api/v1/push"
```

### 4. Grafana Queries All Backends

```
Grafana
  ├── Prometheus datasource → Query metrics (http_request_duration_seconds)
  ├── Tempo datasource → Query traces (trace_id → 10ms latency breakdown)
  └── Loki datasource → Query logs (level=ERROR, service=patient)
```

---

## Configuration

### Application Configuration

**No vendor-specific setup needed.** Just enable OTEL:

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add standard OpenTelemetry (not Prometheus-specific)
builder.Services.AddOpenTelemetryObservability("EHRPlatform.Patient");
builder.Logging.AddOpenTelemetryLogging();

var app = builder.Build();
app.Run();
```

### Environment Variables

```bash
# OTEL Collector endpoint (default: http://otel-collector:4317)
OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4317

# Optional: trace sampling (0.1 = 10% sample rate)
OTEL_TRACES_SAMPLER=parentbased_traceidratio
OTEL_TRACES_SAMPLER_ARG=0.1

# Optional: metrics interval (15s)
OTEL_METRIC_EXPORT_INTERVAL=15000
```

### OTEL Collector Configuration

File: `devops/monitoring/otel-collector.yml`

```yaml
receivers:
  otlp:
    protocols:
      grpc:
        endpoint: 0.0.0.0:4317
      http:
        endpoint: 0.0.0.0:4318

processors:
  batch:
    timeout: 10s
    send_batch_size: 1024
  
  # Optional: redact PHI
  attributes:
    actions:
      - key: patient.ssn
        action: delete
      - key: user.email
        action: delete

exporters:
  # Metrics → Prometheus (via remote_write)
  prometheusremotewrite:
    endpoint: "http://prometheus:9090/api/v1/write"

  # Traces → Tempo
  otlp/tempo:
    endpoint: "tempo:4317"
    tls:
      insecure: true

  # Logs → Loki
  loki:
    endpoint: "http://loki:3100/loki/api/v1/push"

service:
  pipelines:
    metrics:
      receivers: [otlp]
      processors: [batch]
      exporters: [prometheusremotewrite]
    
    traces:
      receivers: [otlp]
      processors: [batch]
      exporters: [otlp/tempo]
    
    logs:
      receivers: [otlp]
      processors: [attributes, batch]
      exporters: [loki]
```

---

## Migration from Prometheus-Specific Code

### ❌ Before (Prometheus-Specific)

```csharp
// In OpenTelemetryExtensions.cs (old)
services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddPrometheusExporter();  // ← Prometheus-specific
    });

// In Program.cs
app.MapPrometheusScrapingEndpoint();  // ← Prometheus-specific
```

**Problems:**
- Can't use Datadog without rewriting code
- Can't add Jaeger without changing application
- No distributed tracing without additional setup

### ✅ After (Vendor-Neutral OTEL)

```csharp
// In OpenTelemetryExtensions.cs (new)
services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddOtlpExporter();  // ← Vendor-neutral
    })
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddOtlpExporter();  // ← Vendor-neutral
    });

// In Program.cs
// (No prometheus-specific endpoint mapping)
```

**Benefits:**
- Same code works with Prometheus, Datadog, New Relic, etc.
- Distributed tracing included by default
- Structured logging included by default
- Just change OTEL Collector config to swap backends

---

## Telemetry Collection (What Gets Sent)

### Metrics

**HTTP Requests** (automatic via ASP.NET Core instrumentation):
```promql
http.server.request.duration{service="patient", method="POST", endpoint="/records"}
http.client.request.duration{service="patient", http.target="http://external-api/..."}
```

**RabbitMQ** (automatic via MassTransit instrumentation):
```promql
messaging.publish.messages{queue="patient-created"}
messaging.receive.messages{queue="patient-created"}
rabbitmq.queue.message_count{queue="patient-created"}
```

**Database** (automatic via SQL instrumentation):
```promql
db.client.operation.duration{database="ehr_patient", operation="SELECT"}
db.client.operations.count{database="ehr_patient", operation="INSERT"}
```

**Custom Identity Metrics**:
```promql
identity.login_success{method="password"}
identity.login_failure{reason="invalid_credentials"}
identity.unauthorized_requests{endpoint="patients"}
```

### Traces

**Example trace (distributed across 3 services):**

```
Trace ID: 550e8400-e29b-41d4-a716-446655440000

Span 1: HTTP POST /api/patients (API Gateway)
  ├─ Duration: 45ms
  ├─ Status: OK
  └─ Child spans: 2

  Span 2: gRPC call to Patient Service (Patient)
    ├─ Duration: 30ms
    ├─ Status: OK
    └─ Child spans: 1

    Span 3: SELECT from PostgreSQL (Patient Service)
      ├─ Duration: 15ms
      ├─ Status: OK
      └─ Attributes: query_type=SELECT, rows_affected=1
```

**Visible in Grafana/Tempo:**
- Latency breakdown: Gateway (15ms) + RPC (10ms) + DB (20ms)
- Identify bottleneck: Database query is slowest
- Root cause: Missing index on `patient_id`

### Logs

**Example structured log entry:**

```json
{
  "timestamp": "2024-01-15T10:30:45Z",
  "level": "error",
  "message": "Failed to create patient record",
  "service.name": "patient",
  "trace_id": "550e8400-e29b-41d4-a716-446655440000",
  "span_id": "abc123def456",
  "exception": "DbUpdateException",
  "context": {
    "user_id": "[redacted]",
    "operation": "CreatePatient",
    "duration_ms": 1250
  }
}
```

---

## Switching Backends

### Example: From Prometheus to Datadog

**Step 1: Update OTEL Collector config**

```yaml
# devops/monitoring/otel-collector-datadog.yml
exporters:
  datadog/api:
    api:
      key: ${DD_API_KEY}  # Set environment variable
  
  datadog/logs:
    api:
      key: ${DD_API_KEY}

service:
  pipelines:
    metrics:
      exporters: [datadog/api]
    
    logs:
      exporters: [datadog/logs]
```

**Step 2: Deploy**

```bash
docker compose -f docker-compose.yml -f docker-compose-datadog.yml up
```

**Step 3: No application code changes needed** ✓

The 10 services continue sending OTLP to the same endpoint; only the Collector destination changes.

### Example: From Tempo to Datadog APM

Same process—only Collector config changes:

```yaml
exporters:
  datadog/apm:
    api:
      key: ${DD_API_KEY}

service:
  pipelines:
    traces:
      exporters: [datadog/apm]
```

---

## Best Practices

### 1. Use Resource Attributes (Not Metrics Labels)

**❌ Bad (high-cardinality metrics):**
```csharp
counter.Add(1, new("user_id", userId));  // Creates 100k time series
```

**✅ Good (low-cardinality metrics + trace attributes):**
```csharp
counter.Add(1, new("endpoint", "patients"));  // 30 time series
Activity.Current?.SetTag("user.id", userId);  // In trace context
```

### 2. Use Trace Context for Correlation

All logs, metrics, and traces share:
- **Trace ID**: Unique per request (e.g., `550e8400...`)
- **Span ID**: Unique per operation
- **Parent Span ID**: Links to previous operation

Grafana automatically correlates:
- Trace details → jump to related logs
- Error logs → jump to failed span
- Slow metric → jump to trace showing bottleneck

### 3. Sampling for High-Volume Traces

Default: 100% of traces (heavy storage cost)

```yaml
# otel-collector.yml
processors:
  probabilistic_sampler:
    sampling_percentage: 10  # 10% sample rate
```

Or per-service:
```bash
export OTEL_TRACES_SAMPLER=parentbased_traceidratio
export OTEL_TRACES_SAMPLER_ARG=0.1  # 10%
```

### 4. PHI Redaction

Remove sensitive data in Collector:

```yaml
processors:
  attributes:
    actions:
      - key: db.statement
        action: delete  # Don't log SQL (may contain PII)
      - key: http.request.body
        action: delete  # Don't log request bodies
      - key: user.email
        pattern: .*@.*
        action: upsert
        new_value: "[redacted]"  # Redact emails
```

---

## Troubleshooting

### Metrics Not Appearing in Prometheus

```bash
# Check OTEL Collector logs
docker logs ehr-otel-collector | grep -i error

# Verify Collector is receiving from services
docker logs ehr-otel-collector | grep "accepted_metric_points"

# Check Prometheus remote write endpoint
curl -s http://localhost:9090/api/v1/targets | jq .
```

### Traces Not Appearing in Tempo

```bash
# Check trace export
docker logs ehr-otel-collector | grep -i "traces"

# Verify Tempo is receiving
docker logs ehr-tempo | grep "traces_in_total"

# Check Tempo API
curl -s http://localhost:3200/api/search | jq .
```

### Logs Not Appearing in Loki

```bash
# Check log export
docker logs ehr-otel-collector | grep -i "logs"

# Verify Loki is receiving
docker logs ehr-loki | tail -20

# Query Loki directly
curl -s 'http://localhost:3100/loki/api/v1/query?query={service="patient"}' | jq .
```

---

## References

- [OpenTelemetry Official Docs](https://opentelemetry.io/docs/)
- [OTEL Collector Configuration](https://opentelemetry.io/docs/collector/configuration/)
- [OTEL .NET Getting Started](https://opentelemetry.io/docs/instrumentation/net/)
- [Prometheus Operator Compatibility](https://opentelemetry.io/docs/reference/specification/protocol/exporter/)
- [Jaeger vs Tempo vs Others](https://github.com/grafana/tempo)

---

## Summary

✅ **Application code is vendor-neutral**  
✅ **All telemetry flows through OTEL Collector**  
✅ **Easy to swap backends (Prometheus, Datadog, Elastic, etc.)**  
✅ **Metrics, traces, and logs automatically correlated**  
✅ **Production-ready with PHI redaction**  
✅ **Standards-based (CNCF)**
