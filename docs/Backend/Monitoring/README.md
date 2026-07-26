# Backend Monitoring, Observability & Reliability Guide

Comprehensive guide on application performance monitoring, structured logging, distributed tracing, and metrics collection for enterprise backend microservices.

This guide aligns with the observability stack defined in `devops/monitoring/` and follows HIPAA compliance requirements for audit trails and security monitoring.

---

## 🏗️ Observability Architecture

```
Microservices (OpenTelemetry SDK)
    ↓
    └─→ OpenTelemetry Collector (devops/monitoring/otel-collector.yml)
        ├─→ Prometheus (metrics)
        │   └─→ Grafana (visualization)
        ├─→ Jaeger (traces)
        └─→ Loki (logs)

        ↓
    Alertmanager (devops/monitoring/alertmanager.yml)
        ├─→ PagerDuty (critical)
        └─→ Slack (warnings)
```

**Configuration Files**:
- `devops/monitoring/prometheus.yml` - Prometheus scrape config + alert rule references
- `devops/monitoring/otel-collector.yml` - OpenTelemetry receiver/processor/exporter config
- `devops/monitoring/alertmanager.yml` - Alert routing rules
- `devops/monitoring/alert-rules/ehr-alerts.yml` - Prometheus alert definitions (20+ rules)
- `devops/monitoring/grafana/dashboards/ehr-overview.json` - Pre-built dashboard

---

## 📊 Core Observability Pillars

### 1. Structured Logging (Serilog / OpenTelemetry)

**Implementation**:
- **Format**: JSON structured logs with correlation IDs (`CorrelationId`, `TraceId`, `SpanId`).
- **Context Injection**: Attach tenant IDs (hospital/vendor), user context, and execution environment to every log event.
- **Destination**: Logs flow to OpenTelemetry Collector → Loki or Elasticsearch for long-term storage (90 days).

**Log Levels**:
- `Fatal`: Application crash, DB connection pool exhaustion, HIPAA audit failures.
- `Error`: Unhandled exception in request execution, unauthorized access attempts.
- `Warning`: Transient failures (e.g., retried HTTP calls, high memory consumption), slow queries.
- `Information`: Business transaction milestones (e.g., claim submitted, billing processed), authentication events.
- `Debug`: Request/response payloads (redacted for PHI), internal state changes.

**Example (C#)**:
```csharp
logger.LogInformation(
  "Patient record accessed by {UserId} from {IPAddress} | PatientId: {PatientId}",
  userId, ipAddress, patientId
);
// Produces JSON: { "UserId": "...", "IPAddress": "...", "PatientId": "...", "Timestamp": "..." }
```

---

### 2. Metrics Collection (Prometheus & Grafana)

**Metrics Flow**:
1. Services expose metrics on `/metrics` endpoint (Prometheus format)
2. Prometheus scrapes every 15s (configured in `devops/monitoring/prometheus.yml`)
3. Data stored in Prometheus TSDB for 30 days
4. Grafana queries Prometheus for visualization

**RED Method** (Application Health):
- **Rate**: Requests per second across API gateways and internal services.
  ```
  rate(http_requests_total[5m]) by (service)
  ```
- **Errors**: HTTP 5xx / 4xx error counts and failed background queue items.
  ```
  rate(http_requests_total{status=~"5.."}[5m]) by (service)
  ```
- **Duration**: P50, P95, and P99 response latencies.
  ```
  histogram_quantile(0.99, rate(http_request_duration_seconds_bucket[5m])) by (service)
  ```

**USE Method** (Infrastructure Health):
- **Utilization**: CPU (85%+ threshold), RAM (85%+ threshold), database connection pool (80%+ threshold).
- **Saturation**: Message queue depth (Kafka consumer lag > 10k messages), disk queue backlog.
- **Errors**: Network drop rates, disk I/O errors, connection timeouts.

**Alerts Triggered** (see `devops/monitoring/alert-rules/ehr-alerts.yml`):
- ✋ `HighErrorRate`: Error rate > 2% for 5 minutes → **Critical** (PagerDuty)
- ✋ `HighLatencyP99`: P99 > 2000ms for 5 minutes → **Critical** (PagerDuty)
- ⚠️ `HighLatencyP95`: P95 > 1000ms for 10 minutes → **Warning** (Slack)
- ✋ `ServiceHealthCheckFailing`: Service down for 2+ minutes → **Critical** (PagerDuty)

---

### 3. Distributed Tracing (Jaeger / OpenTelemetry)

**Trace Flow**:
1. Services instrument with OpenTelemetry SDK
2. Spans sent to OpenTelemetry Collector (gRPC on 4317)
3. Collector exports to Jaeger on port 4317
4. Jaeger stores traces (in-memory for dev, Tempo backend for prod)

**Trace Example** - Patient Appointment Booking:
```
HTTP POST /appointments
├─ Span: validate-input (1ms)
├─ Span: check-patient-exists (50ms) → PostgreSQL query
├─ Span: publish-event (10ms) → Kafka push
│  └─ Span: appointment-created-consumer (200ms)
│     ├─ Span: send-notification (30ms) → SMTP
│     └─ Span: update-billing (50ms) → PostgreSQL update
└─ HTTP Response: 201 Created (311ms total)
```

**Use Cases**:
- Identify bottleneck service in multi-service requests
- Trace async event flow through Kafka consumers
- Correlate errors across service boundaries using `TraceId`

---

## 🚨 Alerting Strategies & Escalation

All alerts are defined in `devops/monitoring/alert-rules/ehr-alerts.yml` and routed via `devops/monitoring/alertmanager.yml`.

### 1. **Critical Alerts** (PagerDuty / Incident Escalation)

These page on-call engineers immediately:

| Alert | Threshold | Duration | Severity |
|-------|-----------|----------|----------|
| High Error Rate | > 2% | 5 min | 🔴 CRITICAL |
| High P99 Latency | > 2000ms | 5 min | 🔴 CRITICAL |
| Service Health Failing | Down | 2 min | 🔴 CRITICAL |
| Database Error Rate | > 5% | 5 min | 🔴 CRITICAL |
| Audit Log Write Failure | Any failure | 5 min | 🔴 CRITICAL |
| Unencrypted Data Transfer | Any attempt | 5 min | 🔴 CRITICAL |
| Disk Space Critical | < 5GB | 5 min | 🔴 CRITICAL |
| Redis Down | Down | 2 min | 🔴 CRITICAL |
| Connection Pool Exhausted | > 80% | 5 min | 🔴 CRITICAL |

### 2. **Warning Alerts** (Slack / Teams Notification)

These notify teams but don't page on-call:

| Alert | Threshold | Duration | Severity |
|-------|-----------|----------|----------|
| High P95 Latency | > 1000ms | 10 min | 🟡 WARNING |
| Kafka Consumer Lag Growing | Growing | 20 min | 🟡 WARNING |
| High CPU Utilization | > 85% | 10 min | 🟡 WARNING |
| High Memory Utilization | > 85% | 10 min | 🟡 WARNING |
| High Disk Usage | > 80% | 10 min | 🟡 WARNING |
| PostgreSQL Slow Queries | > 10/10min | 10 min | 🟡 WARNING |
| Redis Memory > 90% | > 90% | 5 min | 🟡 WARNING |
| Unauthorized Access Attempts | > 10/5min | 5 min | 🟡 WARNING |
| Auth Failure Rate High | > 10% | 5 min | 🟡 WARNING |
| External API High Failure | > 10% | 5 min | 🟡 WARNING |

### 3. **HIPAA Compliance Alerts** (Slack #ehr-compliance)

Special routing for audit and security events:

| Alert | Trigger | Recipient |
|-------|---------|-----------|
| Audit Log Write Failure | Any write error | #ehr-compliance (CRITICAL) |
| Unencrypted Data Transfer | TLS violation | #ehr-compliance (CRITICAL) |
| Unauthorized Access | Multiple failed attempts | #ehr-compliance (WARNING) |
| High Auth Failure Rate | > 10% failures | #ehr-compliance (WARNING) |

---

## 📈 Grafana Dashboard

**Dashboard**: `devops/monitoring/grafana/dashboards/ehr-overview.json`

**Panels** (what's monitored):
1. **Request Rate** - Requests/sec by service (color-coded by status)
2. **Error Rate** - 5xx/4xx errors as % of total traffic
3. **Latency Heatmap** - P50/P95/P99 distribution over time
4. **Service Status** - Health check indicator for each microservice
5. **Kafka Consumer Lag** - Lag by consumer group (growing = slow processing)
6. **Database Connections** - Active vs. max pool connections
7. **Redis Memory** - Used bytes vs. maxmemory limit
8. **CPU/Memory/Disk** - Infrastructure host metrics
9. **Top Slow Queries** - PostgreSQL slow query log

**How to Import**:
1. Open Grafana → Dashboards → Import
2. Upload `devops/monitoring/grafana/dashboards/ehr-overview.json`
3. Select Prometheus data source
4. Save

---

## 🔧 Adding New Metrics to an EHR Microservice

### Step 1: Add OpenTelemetry SDK to .csproj
```xml
<PackageReference Include="OpenTelemetry" Version="1.6.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.6.0" />
<PackageReference Include="OpenTelemetry.Exporter.Prometheus" Version="1.6.0" />
```

### Step 2: Configure in Program.cs
```csharp
builder.Services.AddOpenTelemetry()
  .WithMetrics(m => m
    .AddAspNetCoreInstrumentation()
    .AddHttpClientInstrumentation()
    .AddPrometheusExporter()
  )
  .WithTracing(t => t
    .AddAspNetCoreInstrumentation()
    .AddHttpClientInstrumentation()
    .AddOtlpExporter(options => options.Endpoint = new Uri("http://otel-collector:4317"))
  );
```

### Step 3: Add to Prometheus scrape config
Edit `devops/monitoring/prometheus.yml`:
```yaml
- job_name: my-new-service
  static_configs:
    - targets: ["my-new-service:8080"]
  relabel_configs:
    - source_labels: [__address__]
      target_label: service
      replacement: "my-new-service"
```

### Step 4: Verify metrics
```bash
curl http://my-new-service:8080/metrics
```

---

## 🧪 Testing Alerts Locally

### Trigger High Error Rate Alert
```bash
# Generate 5xx errors on identity service
for i in {1..100}; do
  curl -X GET http://localhost:5001/api/invalid-endpoint
done

# Wait 5 minutes for alert to fire
# Check Alertmanager: http://localhost:9093
```

### Trigger High Latency Alert
```csharp
// Add deliberate delay in endpoint
[HttpGet("slow")]
public async Task<IActionResult> SlowEndpoint()
{
  await Task.Delay(3000); // 3 seconds
  return Ok("Response");
}

// Generate traffic
for i in {1..50}; do
  curl http://localhost:5000/slow
done
```

---

## 📚 Related Documentation

- **Configuration Guide**: See `Configuration-Guide.md` for detailed setup instructions
- **Grafana Dashboard Guide**: See `Grafana-Dashboard-Guide.md` for panel customization
- **DevOps README**: See `devops/README.md` for full infrastructure setup
- **Alert Rules**: See `devops/monitoring/alert-rules/ehr-alerts.yml` for all 20+ alert definitions
